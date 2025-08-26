import React, { useState } from "react";

export default function SearchBar({ onSearch }) {
  const [query, setQuery] = useState("");

  const handleSearch = () => {
    if (onSearch && typeof onSearch === "function") {
      onSearch(query);
    } else {
      console.error("onSearch is not a functionaaaaaaaaaaa", onSearch);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter") {
      handleSearch();
    }
  };

  return (
    <div className="relative flex items-center w-2xl px-6">
      <input
        type="text"
        placeholder="Search questions..."
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        onKeyDown={handleKeyDown}
        className="w-full pl-10 pr-4 py-2 rounded-full shadow-sm border border-cyan-500 focus:outline-none focus:ring-2 focus:ring-cyan-500"
      />

      <svg
        xmlns="http://www.w3.org/2000/svg"
        className="w-5 h-5 text-gray-500 absolute left-9 cursor-pointer hover:text-gray-100"
        fill="currentColor"
        viewBox="0 0 24 24"
        onClick={handleSearch}
      >
        <path
          fillRule="evenodd"
          d="M10 2a8 8 0 015.292 13.707l4.5 4.5a1 1 0 01-1.414 1.414l-4.5-4.5A8 8 0 1110 2zm0 2a6 6 0 100 12 6 6 0 000-12z"
          clipRule="evenodd"
        />
      </svg>
    </div>
  );
}
