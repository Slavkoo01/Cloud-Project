import React, { useState } from "react";
import Navbar from "../../Components/Navbars/NavbarProfile";
import Footer from "../../Components/Footers/FooterProfile";
import FormEditProfile from "../../Components/Forms/FormEditProfile";
export default function Profile() {
  const user = JSON.parse(localStorage.getItem("user"));
  const [isModalOpen, setModalOpen] = useState(false);
  return (
    <>
      <Navbar />
      <main className="profile-page">
        {/* Banner */}
        <section className="relative block h-[500px]">
          <div
            className="absolute top-0 w-full h-full bg-center bg-cover"
            style={{
              backgroundImage: `url('https://images.unsplash.com/photo-1499336315816-097655dcfbda?ixlib=rb-1.2.1&auto=format&fit=crop&w=2710&q=80')`,
            }}
          >
            <span className="w-full h-full absolute opacity-50 bg-black"></span>
          </div>
          <div
            className="top-auto bottom-0 left-0 right-0 w-full absolute pointer-events-none overflow-hidden h-[70px]"
            style={{ transform: "translateZ(0)" }}
          >
            <svg
              className="absolute bottom-0 overflow-hidden"
              xmlns="http://www.w3.org/2000/svg"
              preserveAspectRatio="none"
              viewBox="0 0 2560 100"
            >
              <polygon
                className="fill-gray-100"
                points="2560 0 2560 100 0 100"
              />
            </svg>
          </div>
        </section>

        {/* Profile content */}
        <section className="relative py-16 bg-gray-100">
          <div className="container mx-auto px-4">
            <div className="relative flex flex-col bg-white w-full mb-6 shadow-xl rounded-lg -mt-64">
              <div className="px-6">
                {/* Top section */}
                <div className="flex flex-wrap justify-center">
                  <div className="w-full lg:w-3/12 px-4 flex justify-center">
                    <div className="relative">
                      <img
                        alt="profile"
                        src={
                          user?.ProfileImageUrl ||
                          "https://i.pinimg.com/736x/98/1d/6b/981d6b2e0ccb5e968a0618c8d47671da.jpg"
                        }
                        className="shadow-xl rounded-full h-40 w-40 object-cover border-4 border-white -mt-20"
                      />
                    </div>
                  </div>
                  <div className="w-full lg:w-4/12 px-4 lg:text-right lg:self-center absolute ml-6">
                    <div className="py-6 px-3 mt-32 sm:mt-0">
                      <button
                        className="bg-blue-500 cursor-pointer hover:bg-blue-600 text-white font-bold px-4 py-2 rounded-md shadow-md transition-all"
                        type="button"
                        onClick={() => setModalOpen(true)}
                      >
                        Edit Profile
                      </button>
                       <FormEditProfile isOpen={isModalOpen} onClose={() => setModalOpen(false)} />
                    </div>
                  </div>                 
                </div>

                {/* User info */}
                <div className="text-center mt-12">
                  <h3 className="text-4xl font-semibold text-gray-800 mb-2">
                    {user?.Name} {user?.LastName}
                  </h3>
                  <div className="text-sm text-gray-500 font-bold uppercase">
                    <i className="fas fa-map-marker-alt mr-2"></i>
                    {user?.City}, {user?.Country}
                  </div>
                  <div className="mt-6 text-gray-600">
                    <i className="fas fa-envelope mr-2"></i>
                    {user?.Email}
                  </div>
                  <div className="text-gray-600 mt-2">
                    <i className="fas fa-home mr-2"></i>
                    {user?.Address}
                  </div>
                </div>

                {/* About section */}
                <div className="mt-10 py-10 border-t border-gray-200 text-center">
                  
                </div>
              </div>
            </div>
          </div>
        </section>
      </main>
      <Footer />
    </>
  );
}
