export default function Register() {
  return (
    <div>
      <h2 className="text-2xl font-bold text-center mb-4">Register</h2>
      <form className="space-y-4">
        <input
          type="text"
          placeholder="Username"
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-green-400"
        />
        <input
          type="email"
          placeholder="Email"
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-green-400"
        />
        <input
          type="password"
          placeholder="Password"
          className="w-full px-4 py-2 border rounded-lg focus:ring focus:ring-green-400"
        />
        <button
          type="submit"
          className="w-full bg-green-500 text-white py-2 rounded-lg hover:bg-green-600 transition"
        >
          Register
        </button>
      </form>
    </div>
  );
}
