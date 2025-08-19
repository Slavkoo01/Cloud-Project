import React from "react";

export default function Footer() {
  return (
    <footer className="w-full bg-gray-100 py-4 text-center text-gray-600 mt-auto">
      © {new Date().getFullYear()} MyApp. All rights reserved.
    </footer>
  );
}
