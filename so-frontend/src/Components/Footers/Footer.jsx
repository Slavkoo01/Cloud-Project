export default function Footer() {
  return (
    <footer className=" shadow-lg relative w-full mt-auto">
      {/* Triangle topper */}
      <div
        aria-hidden="true"
        className="absolute -top-5 left-1/2 -translate-x-1/2 
                   w-0 h-0 
                   border-l-[25px] border-l-transparent 
                   border-r-[25px] border-r-transparent 
                   border-b-[25px] border-b-blue-600 
                   "
      />
      <div className="w-full bg-blue-600 py-4 text-center text-gray-100 shadow-lg drop-shadow-md">
        © {new Date().getFullYear()} StackOverflow Project.
      </div>
    </footer>
  );
}
