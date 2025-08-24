import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Auth from "./Layout/Auth/Auth";
import MainPage from "./Layout/MainPage/MainPage";
import Login from "./Views/Auth/Login";
import Register from "./Views/Auth/Register";
import Profile from "./Layout/Profile/Profile";
import FeedQuestions from "./Views/Feed/Questions"
function App() {
  const user = JSON.parse(localStorage.getItem("user"));

  return (
    <BrowserRouter>
      <Routes>
        {!user ? (
          // Guest routes
          <>
            <Route path="/" element={<Auth />}>
              <Route path="login" element={<Login />} />
              <Route path="register" element={<Register />} />
              <Route path="*" element={<Navigate to="/login" replace />} />
              <Route path="/" element={<Navigate to="/login" replace />} />
            </Route>
          </>
        ) : (
          // Authenticated routes
          <>
            <Route path="/" element={<MainPage />}>
              <Route path="/feed" element={<FeedQuestions/>} />
              <Route path="*" element={<Navigate to="/feed" replace />} />
              <Route path="/" element={<Navigate to="/feed" replace />} />
              
            </Route>
            <Route path="/profile" element={<Profile />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </>
        )}
      </Routes>
    </BrowserRouter>
  );
}

export default App;
