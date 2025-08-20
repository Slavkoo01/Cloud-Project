import React from "react";
import { Link, useLocation } from "react-router-dom";

export default function Navbar() {
  const location = useLocation();

  // Decide button label and target based on current URL
  let buttonText = "Sign Up";
  let buttonLink = "/register";

  if (location.pathname === "/register") {
    buttonText = "Login";
    buttonLink = "/login";
  } else if (location.pathname === "/login") {
    buttonText = "Sign Up";
    buttonLink = "/register";
  }

  return (
    <nav className="w-full bg-cyan-400 shadow-lg px-6 py-3 flex items-center justify-between">
      {/* Logo + Title */}
      <Link to="/" className="flex items-center space-x-2">
       <h2 className="text-4xl w-full text-transparent bg-clip-text font-extrabold bg-blue-600 p-2">
      StackOverflow
      </h2>
      </Link>

      {/* Auth Button */}
      <Link to={buttonLink}>
        <button className="px-4 py-2 rounded-lg text-lg font-medium text-gray-100 bg-blue-500 shadow-lg hover:bg-blue-400 transition">
          {buttonText}
        </button>
      </Link>
    </nav>
  );
}
