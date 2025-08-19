
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import App from './App';
import AuthLayout from './Layout/Auth/Auth';

export default function Root() {
  const user = JSON.parse(localStorage.getItem('user')); 

  return (
    <BrowserRouter>
      <Routes>
        {user ? (
          <Route path="/*" element={<App />} />
        ) : (
            
          <Route path="/*" element={<AuthLayout />} />  
        )}
      </Routes>
    </BrowserRouter>
  );
}


