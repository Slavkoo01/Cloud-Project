import React, { useEffect, useState } from "react";
import axios from "axios";

const DEFAULT_PROFILE_IMG =
  "https://i.pinimg.com/736x/98/1d/6b/981d6b2e0ccb5e968a0618c8d47671da.jpg";

export default function QuestionList() {
  const [questions, setQuestions] = useState([]);
  const [expandedQuestionId, setExpandedQuestionId] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    axios
      .get(`${import.meta.env.VITE_API_URL}/questions`)
      .then((res) => {
        setQuestions(res.data || []);
        setLoading(false);
      })
      .catch((err) => {
        console.error("Failed to load questions", err);
        setLoading(false);
      });
  }, []);

  if (loading) {
    return <p className="text-gray-500">Loading questions...</p>;
  }

  const toggleAnswers = (id) => {
    setExpandedQuestionId(expandedQuestionId === id ? null : id);
  };

  const handleVote = (questionId, answerId, vote) => {
    console.log("Vote:", { questionId, answerId, vote });

    axios
      .post(
        `${import.meta.env.VITE_API_URL}/questions/${questionId}/answers/${answerId}/vote`,
        { vote }
      )
      .then((res) => {
        // Update vote count in state (optional)
        setQuestions((prev) =>
          prev.map((q) =>
            q.id === questionId
              ? {
                  ...q,
                  answers: (q.answers || []).map((a) =>
                    a.id === answerId
                      ? { ...a, votes: res.data.votes, userVote: vote }
                      : a
                  ),
                }
              : q
          )
        );
      })
      .catch((err) => {
        console.error("Failed to send vote", err);
      });
  };

  return (
    <div className="max-w-4xl mx-auto p-4 space-y-6">
      {questions.map((q) => {
        const user = q.user || {};
        const answers = q.answers || [];

        return (
          <div key={q.id} className="bg-white rounded-2xl shadow p-4">
            {/* Question Header */}
            <div className="flex items-center mb-3">
              <img
                src={user.profileImageUrl || DEFAULT_PROFILE_IMG}
                alt={user.username || "Unknown User"}
                className="w-10 h-10 rounded-full mr-3 object-cover"
              />
              <div>
                <p className="font-semibold">{user.username || "Unknown"}</p>
                <p className="text-xs text-gray-400">
                  {q.createdAt
                    ? new Date(q.createdAt).toLocaleString()
                    : "Unknown date"}
                </p>
              </div>
            </div>

            {/* Question Title + Body */}
            <h2 className="text-xl font-bold mb-2">{q.title || "Untitled"}</h2>
            <p className="text-gray-700 mb-2">{q.decription || ""}</p>
            {q.imageUrl && (
              <img
                src={q.imageUrl}
                alt="Question"
                className="rounded-lg max-h-60 object-cover mb-3"
              />
            )}

            {/* Toggle Answers */}
            <button
              onClick={() => toggleAnswers(q.id)}
              className="text-blue-600 hover:underline text-sm"
            >
              {expandedQuestionId === q.id
                ? "Hide Answers"
                : `Show Answers (${answers.length})`}
            </button>

            {/* Answers Section */}
            {expandedQuestionId === q.id && (
              <div className="mt-4 space-y-3">
                {answers.map((a) => {
                  const aUser = a.user || {};
                  return (
                    <div
                      key={a.id}
                      className={`border rounded-lg p-3 flex items-start justify-between ${
                        a.isAccepted
                          ? "border-green-500 bg-green-50"
                          : "border-gray-200"
                      }`}
                    >
                      {/* Answer content */}
                      <div className="flex">
                        <img
                          src={aUser.profileImageUrl || DEFAULT_PROFILE_IMG}
                          alt={aUser.username || "Unknown"}
                          className="w-8 h-8 rounded-full mr-2 object-cover"
                        />
                        <div>
                          <p className="text-gray-800">{a.text || ""}</p>
                          <p className="text-xs text-gray-400">
                            {aUser.username || "Unknown"} •{" "}
                            {a.createdAt
                              ? new Date(a.createdAt).toLocaleString()
                              : "Unknown date"}
                          </p>
                        </div>
                      </div>

                      {/* Voting buttons */}
                      <div className="flex flex-col items-center ml-4">
                        <button
                          onClick={() => handleVote(q.id, a.id, 1)}
                          className={`p-1 ${
                            a.userVote === 1 ? "text-blue-600" : "text-gray-400"
                          }`}
                        >
                          ▲
                        </button>
                        <span className="text-sm font-semibold">
                          {a.votes ?? 0}
                        </span>
                        <button
                          onClick={() => handleVote(q.id, a.id, -1)}
                          className={`p-1 ${
                            a.userVote === -1
                              ? "text-red-600"
                              : "text-gray-400"
                          }`}
                        >
                          ▼
                        </button>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
