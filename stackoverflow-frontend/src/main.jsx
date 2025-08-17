import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import './index.css';
import Login from './Login';
import Register from './Register';
import App from './App';
import 'bootstrap/dist/css/bootstrap.min.css'; // CSS
import 'bootstrap/dist/js/bootstrap.bundle.min.js'; // JS bundle
import $ from 'jquery'; // jQuery ako ti je potreban


createRoot(document.getElementById('root')).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Login />} />        
        <Route path="/register" element={<Register />} />
      </Routes>
    </BrowserRouter>
  </StrictMode>
);
