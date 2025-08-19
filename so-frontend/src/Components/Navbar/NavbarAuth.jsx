import React from "react";

export default function Navbar() {
  return (
    <nav className="w-full bg-white shadow-md px-6 py-3 flex items-center justify-between">
      <div className="text-xl font-bold text-indigo-600">MyApp</div>
      <div className="hidden md:flex gap-6">
        <a href="/" className="text-gray-700 hover:text-indigo-600">Home</a>
        <a href="/about" className="text-gray-700 hover:text-indigo-600">About</a>
        <a href="/contact" className="text-gray-700 hover:text-indigo-600">Contact</a>
      </div>
      <button className="md:hidden p-2 rounded-lg hover:bg-gray-100">
        {/* You can put an icon here */}
        ☰
      </button>
    </nav>
  );
}
