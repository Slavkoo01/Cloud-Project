import React, { useEffect, useState } from "react";
import axios from "axios";

export default function EditProfileModal({ isOpen, onClose }) {
  const storedUser = JSON.parse(localStorage.getItem("user")); // current logged-in user
  const [form, setForm] = useState({
    Name: "",
    LastName: "",
    Gender: "",
    Address: "",
    City: "",
    Country: "",
    ProfileImageUrl: ""
  });

  const [imagePreview, setImagePreview] = useState("");
  const [file, setFile] = useState(null);
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState({ type: "", text: "" });

  // preload user data only when modal opens
  useEffect(() => {
    if (isOpen && storedUser) {
      setForm({
        Name: storedUser.Name || "",
        LastName: storedUser.LastName || "",
        Gender: storedUser.Gender || "",
        Address: storedUser.Address || "",
        City: storedUser.City || "",
        Country: storedUser.Country || "",
        ProfileImageUrl: storedUser.ProfileImageUrl || ""
      });
      setImagePreview(storedUser.ProfileImageUrl || "");
      setFile(null);
    }
  }, [isOpen]); 

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
     

      let uploadedImageUrl = form.ProfileImageUrl;

      // 1. Upload image if file selected
      if (file) {
        const formData = new FormData();
        formData.append("file", file);

        const uploadRes = await axios.post(
          `${import.meta.env.VITE_API_URL}/users/${storedUser.Username}/upload-picture`,
          formData,
          {
            headers: {
              "Content-Type": "multipart/form-data",
            },
          }
        );

        uploadedImageUrl = uploadRes.data.ImageUrl;
      }

      // 2. Update user profile
      const updatePayload = { ...form, ProfileImageUrl: uploadedImageUrl };
      const res = await axios.put(
        `${import.meta.env.VITE_API_URL}/users/${storedUser.Username}`,
        updatePayload,
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      // 3. Update localStorage
      localStorage.setItem("user", JSON.stringify(res.data));

      setMsg({ type: "success", text: "Profile updated successfully!" });

      setTimeout(() => {
        setMsg({ type: "", text: "" });
        onClose();
        window.location.reload();
      }, 1500);
    } catch (err) {
      console.error(err);
      setMsg({ type: "error", text: "Failed to update profile" });
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 shadow-xl flex items-center justify-center z-50 bg-black/50">
      <div className="bg-white rounded-lg p-5 w-full max-w-md">
        <h2 className="text-xl font-bold mb-4">Edit Profile</h2>

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
          {/* Profile picture preview */}
          <div className="text-center">
            <img
              src={
                imagePreview ||
                "https://i.pinimg.com/736x/98/1d/6b/981d6b2e0ccb5e968a0618c8d47671da.jpg"
              }
              alt="Profile preview"
              className="shadow-xl rounded-full h-40 w-40 object-cover mx-auto mb-3"
            />
            <input
              type="file"
              accept="image/*"
              onChange={handleFileChange}
              className="block w-full text-sm text-gray-500"
            />
          </div>

          <input
            type="text"
            name="Name"
            value={form.Name}
            onChange={handleChange}
            placeholder="First name"
            className="p-2 border rounded"
          />
          <input
            type="text"
            name="LastName"
            value={form.LastName}
            onChange={handleChange}
            placeholder="Last name"
            className="p-2 border rounded"
          />

          {/* Gender dropdown */}
          <select
            name="Gender"
            value={form.Gender}
            onChange={handleChange}
            className="p-2 border rounded"
          >
            <option value="">Select Gender</option>
            <option value="M">Male</option>
            <option value="F">Female</option>
            <option value="O">Other</option>
          </select>

          <input
            type="text"
            name="Address"
            value={form.Address}
            onChange={handleChange}
            placeholder="Street Address"
            className="p-2 border rounded"
          />
          <input
            type="text"
            name="City"
            value={form.City}
            onChange={handleChange}
            placeholder="City"
            className="p-2 border rounded"
          />
          <input
            type="text"
            name="Country"
            value={form.Country}
            onChange={handleChange}
            placeholder="Country"
            className="p-2 border rounded"
          />

          <div className="flex justify-end gap-2 mt-4">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 rounded bg-gray-300"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={saving}
              className="px-4 py-2 rounded bg-blue-500 text-white disabled:opacity-50"
            >
              {saving ? "Saving..." : "Save"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
