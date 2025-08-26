import React, { useState, useRef, useEffect } from "react";
import { Link } from "react-router-dom";
import DropDownNavbar from "../../Components/DropDowns/DropDownNavbar";
import SearchBar from "../SearchBars/SearchBar";
import FormAskQuestion from "../Forms/FormAskQuestion"

const DEFAULT_PROFILE_IMG =
  "https://i.pinimg.com/736x/98/1d/6b/981d6b2e0ccb5e968a0618c8d47671da.jpg";


export default function Navbar({ onSearch }) {
  const [isDropDownOpen, setIsDropDownOpen] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
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
      <SearchBar onSearch={onSearch} />

      {/* Right side - Auth / User */}
      <div className="flex items-center space-x-4">
        <button
          onClick={()=>setIsModalOpen(true)}
          
          className="px-4 py-2 rounded-lg font-medium text-white bg-green-600 hover:bg-green-400 shadow"
        > 
          Ask Question
        </button>
        <FormAskQuestion isOpen={isModalOpen} onClose={() => setIsModalOpen(false)}/>

        <div ref={dropdownRef} className="relative">
          <button
            onClick={toggleDropDown}
            className="flex items-center space-x-2 focus:outline-none"
          >
            {user.ProfileImageUrl ? (
              <img
                src={user.ProfileImageUrl}
                alt="profile"
                className="w-10 h-10 rounded-full "
              />
            ) : (
              <img
                src={DEFAULT_PROFILE_IMG}
                alt="profile"
                className="w-10 h-10 rounded-full "
              />
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
