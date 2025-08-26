import React from "react";

export default function CardImage({ isOpen, onClose, image }) {
  if (!isOpen) return null; 

  return (
    <div  onClick={onClose} className="fixed inset-0 shadow-xl flex items-center justify-center z-50 bg-black/50">
      <div className="relative">
        <button
          className="absolute top-2 right-2 rounded-full font-bold w-7 h-7 text-black-600 cursor-pointer hover:text-red-600 hover:font-normal flex items-center justify-center"
          onClick={onClose}
        >
          X
        </button>
        <img
          src={image}
          alt="Question screenshot preview"
          className="shadow-lg rounded-md object-contain mb-3 border"
        />
      </div>
    </div>
  );
}
