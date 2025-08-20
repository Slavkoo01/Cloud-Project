import { BrowserRouter, Routes, Route, Navigate, Outlet } from "react-router-dom";
import Navbar from "../../Components/Navbars/Navbar"
import Footer from "../../Components/Footers/Footer"
import Card from "../../Components/Cards/Card"
import MainCard from "../../Components/Cards/MainCard"
import Sidebar from "../../Components/Sidebars/Sidebar"

export default function MainPage() {
  return (
    <>
      <div className="min-h-screen flex bg-linear-to-b from-cyan-400 to-blue-500">
        <div className="flex-col w-full ">
          <div className="flex items-center justify-center mt-3 mb-5">
          <Navbar ></Navbar>
          </div>
          <div className="flex items-center justify-center my-5">
          <MainCard> 
           <Sidebar/>
           <Card>
              <div>
                <Outlet></Outlet>
              </div>
           </Card>
          </MainCard>
          </div>
        </div>
      </div>
      <Footer></Footer>

    </>
  );
}
