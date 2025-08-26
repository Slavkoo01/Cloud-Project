import React, { useEffect, useState } from "react";
import axios from "axios";
import CardImage from "../../Components/Cards/CardImage";
import FormAnswerQuestion from "../../Components/Forms/FormAnswerQuestion";
import EditAnswerModal from "../../Components/Forms/FormAnswerQuestionEdit";
import EditQuestionModal from "../../Components/Forms/FormAskQuestionEdit";

const DEFAULT_PROFILE_IMG =
    "https://i.pinimg.com/736x/98/1d/6b/981d6b2e0ccb5e968a0618c8d47671da.jpg";

export default function QuestionList({ searchQuery, onlyMine }) {
    const storedUser = JSON.parse(localStorage.getItem("user"));
    const [questions, setQuestions] = useState([]);
    const [expandedQuestionId, setExpandedQuestionId] = useState(null);
    const [loading, setLoading] = useState(true);
    const [activeImage, setActiveImage] = useState(null);

    // modal state
    const [isAnswerModalOpen, setAnswerModal] = useState(false);
    const [selectedQuestionId, setSelectedQuestionId] = useState(null);

    const [isEditAnswerModalOpen, setEditAnswerModal] = useState(false);
    const [selectedAnswer, setSelectedAnswer] = useState(null);

    const [isEditQuestionModalOpen, setEditQuestionModal] = useState(false);
    const [selectedQuestion, setSelectedQuestion] = useState(null);


    useEffect(() => {
        setLoading(true); // show loading state for new searches
        axios.get(`${import.meta.env.VITE_API_URL}/questions`, {
            params: { search: searchQuery, LoggedInUser: storedUser.Username, onlyMine : onlyMine }
        })
            .then((res) => {
                console.log("data: ", res.data);
                setQuestions(res.data || []);
                setLoading(false);
            })
            .catch((err) => {
                console.error("Failed to load questions", err);
                setLoading(false);
            });
    }, [searchQuery, storedUser.Username]);


    if (loading) {
        return <p className="text-gray-500">Loading questions...</p>;
    }

    const toggleAnswers = (id) => {
        setExpandedQuestionId(expandedQuestionId === id ? null : id);
    };

    const handleVote = (questionId, answerId, vote) => {
        console.log("Vote:", { questionId, answerId, vote });

        axios.post(
            `${import.meta.env.VITE_API_URL}/votes/${questionId}/${answerId}`,
            {
                vote: vote, // make sure this matches backend VoteDto (lowercase "vote")
                Username: storedUser.Username
            }
        )
            .then((res) => {

                const updatedAnswer = res.data;
                console.log("isUpvodeted: ", res.data);

                setQuestions((prev) =>
                    prev.map((q) =>
                        q.Id === questionId
                            ? {
                                ...q,
                                Answers: (q.Answers || []).map((a) =>
                                    a.Id === answerId
                                        ? {
                                            ...a,
                                            Votes: updatedAnswer.VoteCount,
                                            UserVote: updatedAnswer.UserVote
                                        }
                                        : a
                                )
                            }
                            : q
                    )
                );
            })
            .catch((err) => {
                console.error("Failed to send vote", err);
            });
    };

    const handleAcceptAnswer = (questionId, answerId) => {
        axios.post(`${import.meta.env.VITE_API_URL}/answers/${questionId}/${answerId}/toggle-accept`)
            .then((res) => {
                const { AnswerId, IsAccepted, IsThemeOpen } = res.data;

                setQuestions((prev) =>
                    prev.map((q) =>
                        q.Id === questionId
                            ? {
                                ...q,
                                IsThemeOpen: IsThemeOpen,
                                Answers: q.Answers.map((a) =>
                                    a.Id === AnswerId ? { ...a, IsAccepted: IsAccepted } : a
                                )
                            }
                            : q
                    )
                );
            })
            .catch((err) => {
                console.error("Failed to toggle accept answer", err);
            });
    };


    const handleDeleteQuestion = async (questionId) => {
        if (!window.confirm("Are you sure you want to delete this question?")) return;

        try {
            await axios.delete(`${import.meta.env.VITE_API_URL}/questions/${questionId}`);
            setQuestions((prev) => prev.filter((q) => q.Id !== questionId));
        } catch (err) {
            console.error("Failed to delete question", err);
        }
    };


    const handleDeleteAnswer = async (questionId, answerId) => {
        try {
            const res = await axios.delete(
                `${import.meta.env.VITE_API_URL}/answers/${questionId}/${answerId}`
            );

            const { IsThemeOpen } = res.data;

            setQuestions((prev) =>
                prev.map((q) =>
                    q.Id === questionId
                        ? {
                            ...q,
                            IsThemeOpen,
                            Answers: q.Answers.filter((a) => a.Id !== answerId),
                        }
                        : q
                )
            );
        } catch (err) {
            console.error("Failed to delete answer", err);
        }
    };
    const sortAnswers = (questionId, criteria) => {
        setQuestions((prev) =>
            prev.map((q) => {
                if (q.Id !== questionId) return q;

                const sortedAnswers = [...(q.Answers || [])];

                if (criteria === "date") {
                    sortedAnswers.sort((a, b) => new Date(a.CreatedAt) - new Date(b.CreatedAt));
                } else if (criteria === "votes") {
                    sortedAnswers.sort((a, b) => (b.Votes ?? 0) - (a.Votes ?? 0));
                }

                return { ...q, Answers: sortedAnswers };
            })
        );
    };


    const openEditAnswerModal = (questionId, answer) => {
        setSelectedQuestionId(questionId);
        setSelectedAnswer(answer);
        setEditAnswerModal(true);
    };

    const openEditQuestionModal = (question) => {
        setSelectedQuestion(question);
        setEditQuestionModal(true);
    };


    const openAnswerModal = (questionId) => {
        setSelectedQuestionId(questionId);
        setAnswerModal(true);
    };

    return (
        <div className="max-w-4xl bg-black/10 rounded-3xl shadow-lg mx-auto p-6 space-y-8">
            {questions.map((q) => {
                const user = q.User || {};
                const answers = q.Answers || [];

                return (
                    <div key={q.Id} className="relative bg-white rounded-2xl shadow-md p-4">
                        {/* Top-right controls for Question */}
                        {storedUser.Username === q.User?.Username && (
                            <div className="absolute top-4 right-4 flex gap-4">
                                <button
                                    onClick={() => openEditQuestionModal(q)}
                                    className="border rounded-md p-2 bg-gray-100 cursor-pointer hover:bg-blue-100 hover:border-blue-700"
                                    title="Edit Question"
                                >
                                    ✏️
                                </button>
                                <button
                                    onClick={() => handleDeleteQuestion(q.Id)}
                                    className="border rounded-md p-2 bg-gray-100 cursor-pointer hover:bg-red-100 hover:border-red-700"
                                    title="Delete Question"
                                >
                                    🗑️
                                </button>
                            </div>
                        )}


                        {/* Question Header */}
                        <div className="flex items-center mb-3">
                            <img
                                src={user.ProfileImageUrl || DEFAULT_PROFILE_IMG}
                                alt={user.Username || "Unknown User"}
                                className="w-10 h-10 rounded-full mr-3 object-cover"
                            />
                            <div>
                                <p className="font-semibold">{user.Username || "Unknown"}</p>
                                <p className="text-xs text-gray-400">
                                    {q.CreatedAt
                                        ? new Date(q.CreatedAt).toLocaleString()
                                        : "Unknown date"}
                                </p>
                            </div>
                        </div>

                        {/* Question Title + Body */}
                        <h2 className="text-xl font-bold mb-2">{q.Title || "Untitled"}</h2>
                        <p className="text-gray-700 mb-2">{q.Decription || ""}</p>
                        {q.ImageUrl && (
                            <div className="h-65">
                                <img
                                    onClick={() => setActiveImage(q.ImageUrl)}
                                    src={q.ImageUrl}
                                    alt="Question"
                                    className="rounded-lg max-h-60 object-cover cursor-pointer hover:max-h-62 mb-3"
                                />
                            </div>
                        )}
                        {/* Sort Answers Buttons */}
                        {expandedQuestionId === q.Id && answers.length > 0 && (
                            <div className="flex gap-2 mb-2">
                                <button
                                    onClick={() => sortAnswers(q.Id, "date")}
                                    className="px-2 py-1 bg-gray-200 rounded hover:bg-gray-300 text-sm"
                                >
                                    Sort by Date
                                </button>
                                <button
                                    onClick={() => sortAnswers(q.Id, "votes")}
                                    className="px-2 py-1 bg-gray-200 rounded hover:bg-gray-300 text-sm"
                                >
                                    Sort by Votes
                                </button>
                            </div>
                        )}

                        {/* Toggle Answers */}
                        <button
                            onClick={() => toggleAnswers(q.Id)}
                            className="text-sky-600 cursor-pointer hover:underline text-sm"
                        >
                            {expandedQuestionId === q.Id
                                ? "Hide Answers"
                                : `Show Answers (${answers.length})`}
                        </button>

                        {/* Answers Section */}
                        {expandedQuestionId === q.Id && (
                            <div className="mt-4 space-y-3">
                                {answers.map((a) => {
                                    const aUser = a.User || {};
                                    return (
                                        <div
                                            key={a.Id}
                                            className={`border rounded-lg p-3 flex items-start justify-between ${a.IsAccepted
                                                ? "border-green-500 bg-green-50"
                                                : "border-gray-200"
                                                }`}
                                        >
                                            {/* Top-right controls for Answer */}

                                            {/* Answer content */}
                                            <div className="flex">
                                                <img
                                                    src={aUser.ProfileImageUrl || DEFAULT_PROFILE_IMG}
                                                    alt={aUser.Username || "Unknown"}
                                                    className="w-8 h-8 rounded-full mr-2 object-cover"
                                                />

                                                <div>
                                                    <p className="text-xs text-gray-400">
                                                        {aUser.Username || "Unknown"} •{" "}
                                                        {a.CreatedAt
                                                            ? new Date(a.CreatedAt).toLocaleString()
                                                            : "Unknown date"}
                                                    </p>
                                                    <p className="text-gray-800">{a.Text || ""}</p>

                                                </div>
                                            </div>


                                            {/* Voting buttons */}
                                            <div className="flex flex-col items-end">

                                                {storedUser.Username === a.User?.Username && (
                                                    <div className="my-1 mb-2 flex gap-2">
                                                        <button
                                                            onClick={() => openEditAnswerModal(q.Id, a)}
                                                            className="border rounded-md p-1 text-sm cursor-pointer bg-gray-100 hover:bg-blue-100 hover:border-blue-700"
                                                            title="Edit Answer"
                                                        >
                                                            ✏️
                                                        </button>

                                                        <button
                                                            onClick={() => handleDeleteAnswer(q.Id, a.Id)}
                                                            className="border rounded-md p-1 text-sm cursor-pointer bg-gray-100 hover:bg-red-100 hover:border-red-700"
                                                            title="Delete Answer"
                                                        >
                                                            🗑️
                                                        </button>
                                                    </div>
                                                )}
                                                <div className="flex flex-col items-center">

                                                    {storedUser.Username === q.User?.Username && (
                                                        (q.IsThemeOpen || a.IsAccepted) && (
                                                            <button
                                                                onClick={() => handleAcceptAnswer(q.Id, a.Id)}
                                                                className={`mb-2 text-lg cursor-pointer ${a.IsAccepted ? "text-green-600" : "text-gray-400"
                                                                    }`}
                                                                title={a.IsAccepted ? "Unaccept answer" : "Accept answer"}
                                                            >
                                                                {a.IsAccepted ? "❌" : "✔️"}
                                                            </button>
                                                        )
                                                    )}

                                                    <button
                                                        onClick={() => handleVote(q.Id, a.Id, 1)}
                                                        className={`p-1 ${a.UserVote === 1
                                                            ? "text-blue-600"
                                                            : "text-gray-400"
                                                            }`}
                                                    >
                                                        ▲
                                                    </button>
                                                    <span className="text-sm font-semibold">
                                                        {a.Votes ?? 0}
                                                    </span>
                                                    <button
                                                        onClick={() => handleVote(q.Id, a.Id, -1)}
                                                        className={`p-1 ${a.UserVote === -1
                                                            ? "text-red-600"
                                                            : "text-gray-400"
                                                            }`}
                                                    >
                                                        ▼
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    );
                                })}

                                {/* Button for answering */}
                                {q.IsThemeOpen && (
                                    <div className="flex justify-center items-center h-8">
                                        <button
                                            className="rounded-lg cursor-pointer hover:bg-emerald-500 hover:py-1.5 hover:px-3.5 bg-emerald-600 py-2 px-4 text-white"
                                            onClick={() => openAnswerModal(q.Id)}
                                        >
                                            Answer!
                                        </button>
                                    </div>
                                )}

                            </div>
                        )}
                    </div>
                );
            })}

            {/* Global Modal (only one instance) */}
            <FormAnswerQuestion
                isOpen={isAnswerModalOpen}
                onClose={() => setAnswerModal(false)}
                questionId={selectedQuestionId}
            />

            <EditQuestionModal
                isOpen={isEditQuestionModalOpen}
                onClose={(updatedQuestion) => {
                    setEditQuestionModal(false);
                    if (updatedQuestion) {
                        setQuestions((prev) =>
                            prev.map((q) =>
                                q.Id === updatedQuestion.Id
                                    ? {
                                        ...q,
                                        Title: updatedQuestion.Title,
                                        Description: updatedQuestion.Description,
                                        ImageUrl: updatedQuestion.ImageUrl
                                    }
                                    : q
                            )
                        );
                    }
                }}
                question={selectedQuestion}
            />



            <EditAnswerModal
                isOpen={isEditAnswerModalOpen}
                onClose={(updatedAnswer) => {
                    setEditAnswerModal(false);
                    if (updatedAnswer) {
                        setQuestions((prev) =>
                            prev.map((q) =>
                                q.Id === selectedQuestionId
                                    ? {
                                        ...q,
                                        Answers: q.Answers.map((a) =>
                                            a.Id === updatedAnswer.Id ? { ...a, Text: updatedAnswer.Text } : a
                                        ),
                                    }
                                    : q
                            )
                        );
                    }
                }}
                questionId={selectedQuestionId}
                answer={selectedAnswer}
            />

            {activeImage && (
                <CardImage
                    image={activeImage}
                    onClose={() => setActiveImage(null)}
                    isOpen={!!activeImage}
                />
            )}
        </div>
    );
}
