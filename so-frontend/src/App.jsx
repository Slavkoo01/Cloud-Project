import { BrowserRouter } from 'react-router-dom'
import AuthLayout from "./Layout/Auth/Auth"
import MainPageLayout from "./Layout/MainPage/MainPage"
import './App.css'

function App() {
  const user = JSON.parse(localStorage.getItem("user"));

  return (
    <>
      <div className='bg-gray-500'>
        <BrowserRouter>
          {user ? <MainPageLayout /> : <AuthLayout />}
        </BrowserRouter>
      </div>
    </>
  )
}

export default App
