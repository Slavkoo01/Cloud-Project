export default function Login() {
  return (
    <div>
      <h2 className="text-2xl font-bold text-center mb-4">Login</h2>
      <form className="space-y-4">
        <input
          type="email"
          placeholder="Email"
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-blue-400"
        />
        <input
          type="password"
          placeholder="Password"
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-blue-400"
        />
        <button
          type="submit"
          className="w-full bg-blue-500 text-white py-2 rounded-lg hover:bg-blue-600 transition"
        >
          Login
        </button>
      </form>
    </div>
  );
}
