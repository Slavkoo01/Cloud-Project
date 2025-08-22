import { Outlet } from "react-router-dom";
import NavbarAuth from "../../Components/Navbars/NavbarAuth";
import Footer from "../../Components/Footers/Footer";

export default function Auth() {
  return (
    <>
      <NavbarAuth />
      <div className="min-h-screen flex items-center justify-center bg-linear-to-b from-cyan-400 to-blue-500">
        <div className="w-full max-w-md bg-white shadow-lg rounded-2xl p-6">
          <Outlet />
        </div>
      </div>
      <Footer />
    </>
  );
}
