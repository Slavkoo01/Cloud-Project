import { useState } from 'react';

function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const response = await fetch("http://localhost:5000/api/auth/login", {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });

      if (response.ok) {
        const data = await response.json();
        console.log('Login successful:', data);
        localStorage.setItem('token', data.token);
      } else {
        console.error('Login failed:', response.statusText);
      }

    } catch (error) {
      console.error('Error during login:', error);
    }
  };

 return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-purple-600 via-pink-500 to-red-500">
      <div className="w-full max-w-md p-8 bg-white/10 backdrop-blur-lg rounded-xl shadow-lg border border-white/20">
        <h2 className="text-3xl font-bold text-white text-center mb-6">Welcome Back</h2>
        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label className="block text-sm font-medium text-white mb-1">Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-4 py-2 rounded-lg border border-white/30 bg-white/20 text-white placeholder-white/50 focus:outline-none focus:ring-2 focus:ring-pink-400 focus:border-transparent"
              placeholder="you@example.com"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-white mb-1">Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full px-4 py-2 rounded-lg border border-white/30 bg-white/20 text-white placeholder-white/50 focus:outline-none focus:ring-2 focus:ring-pink-400 focus:border-transparent"
              placeholder="********"
            />
          </div>

          <button
            type="submit"
            className="w-full py-2 rounded-lg bg-pink-500 hover:bg-pink-600 text-white font-semibold transition-colors duration-300"
          >
            Login
          </button>
        </form>

        <p className="mt-6 text-center text-white/70 text-sm">
          Don’t have an account? <a href="/register" className="text-white font-medium underline">Sign up</a>
        </p>
      </div>
    </div>
  )
}
export default Login;
