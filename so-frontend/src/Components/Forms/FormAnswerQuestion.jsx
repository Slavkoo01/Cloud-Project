import React, { useState } from "react";
import axios from "axios";

export default function AnswerModal({ isOpen, onClose, questionId }) {
  const storedUser = JSON.parse(localStorage.getItem("user"));

  const [form, setForm] = useState({
    Text: "",
    Username: storedUser?.Username
  });

  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState({ type: "", text: "" });

  if (!isOpen) return null;

  const handleChange = (e) => {
    const { value } = e.target;
    setForm((prev) => ({ ...prev, Text: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setMsg({ type: "", text: "" });

    try {
        console.log("questioIdL: ",questionId)
      await axios.post(
        `${import.meta.env.VITE_API_URL}/answers/${questionId}`,
        form,
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      setMsg({ type: "success", text: "Answer posted successfully!" });

      setTimeout(() => {
        resetForm();
        onClose();
        window.location.reload();
      }, 1500);
    } catch (err) {
      console.error(err);
      setMsg({ type: "error", text: "Failed to post answer" });
    } finally {
      setSaving(false);
    }
  };

  const resetForm = () => {
    setForm({ Text: "", User: form.User });
  };

  const handleOnClose = () => {
    resetForm();
    onClose();
  };

  return (
    <div className="fixed inset-0 shadow-xl flex items-center justify-center z-50 bg-black/50">
      <div className="bg-white rounded-lg p-5 w-full max-w-2xl">
        <h2 className="text-xl font-bold mb-4">Write an Answer</h2>

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
          <textarea
            name="Text"
            value={form.Text}
            onChange={handleChange}
            placeholder="Write your answer..."
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
              {saving ? "Posting..." : "Post Answer"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
