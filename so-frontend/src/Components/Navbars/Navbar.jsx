import React, { useState, useRef, useEffect } from "react";
import { Link } from "react-router-dom";
import DropDownNavbar from "../../Components/DropDowns/DropDownNavbar";
import SearchBar from "../SearchBars/SearchBar";

export default function Navbar() {
  const [isDropDownOpen, setIsDropDownOpen] = useState(false);

  const storedUser = localStorage.getItem("user");
  const user = storedUser ? JSON.parse(storedUser) : null;


  const dropdownRef = useRef(null);

  const toggleDropDown = () => {
    setIsDropDownOpen((prev) => !prev);
  };

  // Close dropdown on outside click
  useEffect(() => {
    function handleClickOutside(event) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target)
      ) {
        setIsDropDownOpen(false);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  return (
    <nav className="w-95/100 rounded-full bg-cyan-400 shadow-xl px-6 py-5 flex items-center justify-between">
      {/* Left side - Logo */}
      <div className="flex items-center space-x-2">
        <Link to="/" className="flex items-center space-x-2">
          <span className="text-xl font-bold text-blue-600">StackOverflow</span>
        </Link>
      </div>

      {/* Middle - Search */}
      <SearchBar />

      {/* Right side - Auth / User */}
      <div className="flex items-center space-x-4">
        <Link
          to="/ask"
          className="px-4 py-2 rounded-lg font-medium text-white bg-green-600 hover:bg-green-400 shadow"
        >
          Ask Question
        </Link>

        <div ref={dropdownRef} className="relative">
          <button
            onClick={toggleDropDown}
            className="flex items-center space-x-2 focus:outline-none"
          >
            {user.ProfileImageUrl ? (
              <img
                src={user.ProfileImageUrl}
                alt="profile"
                className="w-10 h-10 rounded-full border border-gray-300"
              />
            ) : (
              <div className="w-10 h-10 rounded-full bg-blue-600 text-white flex items-center justify-center font-bold">
                {user.Name[0]}
                {user.LastName[0]}
              </div>
            )}
            <span className="font-medium text-gray-800">{user.Name}</span>
          </button>

          {isDropDownOpen && (
            <div className="absolute right-0 mt-2">
              <DropDownNavbar />
            </div>
          )}
        </div>
      </div>
    </nav>
  );
}
