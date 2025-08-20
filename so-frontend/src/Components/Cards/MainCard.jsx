export default function MainCard({ children }) {
  return (
    <div className="w-[95%] rounded-4xl min-h-screen bg-gradient-to-b from-cyan-400 to-blue-500
      shadow-2xl px-6 py-5 flex gap-6">
      {children}
    </div>
  );
}
