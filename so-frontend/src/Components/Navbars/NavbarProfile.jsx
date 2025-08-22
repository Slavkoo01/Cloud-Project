/*eslint-disable*/
import React from "react";
import { Link, useNavigate } from "react-router-dom";

export default function Navbar(props) {

  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.removeItem("user");
    navigate("/login"); // redirect to login page
    window.location.reload()
  };

  return (
    <>
      <nav className="top-0 absolute z-50 w-full flex flex-wrap items-center justify-between px-2 py-3 navbar-expand-lg">
        <div className="container px-4 mx-auto flex flex-wrap items-center justify-between">
          {/* Left side links */}
          <div className="w-full relative flex justify-between lg:w-auto lg:static lg:block lg:justify-start">
            <Link
              className="text-white text-sm font-bold leading-relaxed inline-block mr-4 py-2 whitespace-nowrap uppercase"
              to="/"
            >
              StackOverflow
            </Link>
          </div>

          {/* Right side logout button */}
          <div className="flex items-center">
            <button
              onClick={handleLogout}
              className="bg-red-800 cursor-pointer hover:bg-red-700 text-white text-sm font-bold py-2 px-4 rounded-lg uppercase shadow-lg"
            >
              Logout
            </button>
          </div>
        </div>
      </nav>
    </>
  );
}
