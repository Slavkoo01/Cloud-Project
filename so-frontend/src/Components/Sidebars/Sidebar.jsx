import React from "react";

export default function Sidebar() {
  return (
    <aside className="h-screen w-60 bg-linear-to-b from-cyan-500 to-blue-600 rounded-2xl text-white flex flex-col p-4">
      <h2 className="text-2xl font-bold mb-6">Dashboard</h2>
      <nav className="flex flex-col gap-4">
        <a href="/" className="flex items-center gap-2 hover:bg-cyan-400 p-2 rounded-lg">
          🏠 Home
        </a>
        <a href="/profile" className="flex items-center gap-2 hover:bg-cyan-400 p-2 rounded-lg">
          👤 Profile
        </a>
      </nav>
    </aside>
  );
}
