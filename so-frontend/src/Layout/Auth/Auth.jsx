import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Login from "../../Views/Auth/Login";
import Register from "../../Views/Auth/Register";
import NavbarAuth from "../../Components/Navbar/NavbarAuth";
import Footer from "../../Components/Footer/Footer";

export default function Auth() {
  return (
    <div className="w-full">
     <NavbarAuth></NavbarAuth>
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      <div className="w-full max-w-md bg-white shadow-lg rounded-2xl p-6">
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            
            <Route path="*" element={<Navigate to="/login" replace />} />
          </Routes>
      </div>
    </div>
    <Footer></Footer>
    </div>
  );
}
