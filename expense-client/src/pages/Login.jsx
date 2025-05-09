// src/pages/Login.jsx
import React, { useState, useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';
import { loginUser } from '../services/authService';
import './Login.css';

import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";
import Button from "react-bootstrap/Button"

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const { setUser } = useContext(AuthContext);
  const navigate = useNavigate();
  

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await loginUser(email, password);
      localStorage.setItem('token', response.token);
      setUser(response.user);
      navigate('/dashboard');
    } catch (err) {
      setError('Invalid email or password');
    }
  };

  const { instance } = useMsal();

  const handleLogin = () => {
       
    instance.loginRedirect(loginRequest).catch(e => {
        console.log(e);
    });
      
  }

  const handleLogout = (logoutType) => {
    
        instance.logoutRedirect({
            postLogoutRedirectUri: "/",
        });
    
  }

  return (
    <div>
    <Button variant="danger" onClick={() => handleLogin()}>Sign in</Button>
    <Button variant="danger" onClick={() => handleLogout()}>Sign out</Button>
    </div>
  );
};

export default Login;
