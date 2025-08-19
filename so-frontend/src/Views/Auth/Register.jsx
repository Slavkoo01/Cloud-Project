import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import axios from "axios";

export default function Register() {
  const [formData, setFormData] = useState({
    Name: "",
    LastName: "",
    Gender: "",
    Country: "",
    City: "",
    Address: "",
    Email: "",
    Username: "",
    Password: "",
  });
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const navigate = useNavigate();

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
  e.preventDefault();
  setError("");
  setSuccess("");

  // enforce uppercase keys
  const payload = {
    Name: formData.Name,
    LastName: formData.LastName,
    Gender: formData.Gender,
    Country: formData.Country,
    City: formData.City,
    Address: formData.Address,
    Email: formData.Email,
    Username: formData.Username,
    Password: formData.Password,
  };

  try {
    const res = await axios.post(
      `${import.meta.env.VITE_API_URL}/auth/register`,
      payload
    );

    if (res.status === 200 && res.data === "User registered successfully") {
      setSuccess(res.data);
      setTimeout(() => navigate("/login"), 1500);
    }
  } catch (err) {
    console.error(err);
    if (err.response && err.response.data) {
      setError(err.response.data);
    } else {
      setError("Registration failed. Try again.");
    }
  }
};


  return (
    <div className="max-w-md mx-auto mt-10 p-6 bg-white shadow-lg rounded-2xl">
      <h2 className="text-2xl font-bold text-center mb-4">Register</h2>
      <form className="space-y-4" onSubmit={handleSubmit}>
         <input type="email" name="Email" placeholder="Email" value={formData.Email}
          onChange={handleChange} required
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-blue-400" />

        <input type="text" name="Username" placeholder="Username" value={formData.Username}
          onChange={handleChange} required
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-blue-400" />

        <input type="password" name="Password" placeholder="Password" value={formData.Password}
          onChange={handleChange} required
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-blue-400" />

        <input type="text" name="Name" placeholder="Name" value={formData.Name}
          onChange={handleChange} required
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-blue-400" />

        <input type="text" name="LastName" placeholder="Last Name" value={formData.LastName}
          onChange={handleChange} required
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-blue-400" />

        <select name="Gender" value={formData.Gender}
          onChange={handleChange} required
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-blue-400">
          <option value="">Select Gender</option>
          <option value="M">Male</option>
          <option value="F">Female</option>
          <option value="O">Other</option>
        </select>

        <input type="text" name="Country" placeholder="Country" value={formData.Country}
          onChange={handleChange} required
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-blue-400" />

        <input type="text" name="City" placeholder="City" value={formData.City}
          onChange={handleChange} required
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-blue-400" />

        <input type="text" name="Address" placeholder="Address" value={formData.Address}
          onChange={handleChange} required
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-blue-400" />

        {error && <p className="text-red-500 text-center">{error}</p>}
        {success && <p className="text-green-500 text-center">{success}</p>}

        <button type="submit"
          className="w-full bg-emerald-500 text-white py-2 rounded-lg hover:bg-emerald-600 transition">
          Register
        </button>
      </form>

      <p className="mt-4 text-center text-gray-600">
        Already have an account?{" "}
        <Link to="/login" className="text-blue-500 hover:underline">
          Login
        </Link>
      </p>
    </div>
  );
}
