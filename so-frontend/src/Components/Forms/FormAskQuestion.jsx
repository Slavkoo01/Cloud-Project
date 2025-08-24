import React, { useState, useRef  } from "react";
import axios from "axios";

export default function AskQuestionModal({ isOpen, onClose }) {
  const storedUser = JSON.parse(localStorage.getItem("user"));
  const fileInputRef = useRef(null);

  const [form, setForm] = useState({
    Title: "",
    Description: ""
  });

  const [imagePreview, setImagePreview] = useState("");
  const [file, setFile] = useState(null);
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState({ type: "", text: "" });

  if (!isOpen) return null;

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleFileChange = (e) => {
    const selectedFile = e.target.files[0];
    if (selectedFile) {
      setFile(selectedFile);
      setImagePreview(URL.createObjectURL(selectedFile));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setMsg({ type: "", text: "" });

    try {
      // 1. Create the question first
      const createPayload = { ...form };
      const resCreateQuestion = await axios.post(
        `${import.meta.env.VITE_API_URL}/questions/${storedUser.Username}`,
        createPayload,
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      const questionId = resCreateQuestion.data.QuestionId;

      // 2. Upload screenshot if file is selected
      if (file) {
        const formData = new FormData();
        formData.append("file", file);

        await axios.post(
          `${import.meta.env.VITE_API_URL}/questions/${questionId}/upload-picture`,
          formData,
          {
            headers: {
              "Content-Type": "multipart/form-data",
            },
          }
        );
      }

      setMsg({ type: "success", text: "Asked Question Successfully!" });

      setTimeout(() => {
        resetForm();
        onClose();
        window.location.reload();
      }, 1500);
    } catch (err) {
      console.error(err);
      setMsg({ type: "error", text: "Failed to ask a question" });
    } finally {
      setSaving(false);
    }
  };
  const resetImagePreview = () => {
    setFile(null);
    setImagePreview("");
     if (fileInputRef.current) {
    fileInputRef.current.value = ""; 
  }
  }
  const resetForm = () => {
    setForm({ Title: "", Description: "" });
    setImagePreview("");
    setFile(null);
  };

  const handleOnClose = () => {
    resetForm();
    onClose();
  };

  return (
    <div className="fixed inset-0 shadow-xl flex items-center justify-center z-50 bg-black/50">
      <div className="bg-white rounded-lg p-5 w-full max-w-2xl"> 
        <h2 className="text-xl font-bold mb-4">Ask a Question</h2>

        {msg.text && (
          <div
            className={`mb-3 p-2 rounded text-sm font-medium ${
              msg.type === "success"
                ? "bg-emerald-200 text-emerald-600"
                : "bg-red-200 text-red-600"
            }`}
          >
            {msg.text}
          </div>
        )}

        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          {/* Screenshot Preview */}
          <div className="text-center">
            {imagePreview && (
                 <div className="relative">
                    <button
                    className="absolute top-2 right-2 rounded-full font-bold  w-7 h-7  text-red-600 hover:bg-red-500 hover:text-white flex items-center justify-center"
                    onClick={resetImagePreview}
                    >
                    X
                    </button>
                    <img
                    src={imagePreview}
                    alt="Question screenshot preview"
                    className="shadow-lg rounded-md h-64 w-full object-contain mb-3 border"
                    />
                </div>
            )}
            <input
              ref={fileInputRef}
              type="file"
              accept="image/*"
              onChange={handleFileChange}
              className="block w-full text-sm text-gray-500"
            />
          </div>

          <input
            type="text"
            name="Title"
            value={form.Title}
            onChange={handleChange}
            placeholder="Title"
            className="p-2 border rounded"
          />

          <textarea
            name="Description"
            value={form.Description}
            onChange={handleChange}
            placeholder="Describe your problem..."
            rows="6"
            className="p-2 border rounded"
          />

          {/* Buttons */}
          <div className="flex justify-end gap-2 mt-4">
            <button
              type="button"
              onClick={handleOnClose}
              className="px-4 py-2 rounded bg-gray-300"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={saving}
              className="px-4 py-2 rounded bg-blue-500 text-white disabled:opacity-50"
            >
              {saving ? "Asking..." : "Ask"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
