import React from "react";
import { Link, useNavigate } from "react-router-dom";
export default function SearchBar() {
    const navigate = useNavigate();

    const handleLogout = () => {
        localStorage.removeItem("user");
        navigate("/login");
        window.location.reload();
    };



    return (
        <div
            className="absolute right-0 mt-2 w-40 bg-white rounded-lg shadow-lg"
        >
            <Link to="/profile" className="block px-4 py-2 hover:bg-gray-100">
                Profile
            </Link>
            <button
                onClick={handleLogout}
                className="block w-full text-left px-4 py-2 hover:bg-gray-100"
            >
                Logout
            </button>
        </div>

    );
}
