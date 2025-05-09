import React, { useEffect, useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMsal } from "@azure/msal-react";
import { AuthContext } from '../context/AuthContext';
import { loginRequest } from "../authConfig";

import Button from 'react-bootstrap/Button';

const Login = () => {
  const { instance } = useMsal();
  const navigate = useNavigate();
  const { user, setUser } = useContext(AuthContext);

  useEffect(() => {
    // Handle redirect after login
    instance.handleRedirectPromise()
      .then((response) => {
        if (response) {
          const account = response.account;
          setUser(account);
          navigate('/dashboard');
        }
      })
      .catch((error) => {
        console.error('Login redirect error:', error);
      });
  }, [instance, navigate, setUser]);

  // If already authenticated, redirect to dashboard
  useEffect(() => {
    if (user) {
      navigate('/dashboard');
    }
  }, [user, navigate]);

  const handleLogin = () => {
    instance.loginRedirect(loginRequest).catch(e => {
      console.error('Login initiation failed', e);
    });
  };

  return (
    <Button variant="danger" onClick={handleLogin}>Sign in</Button>
  );
};

export default Login;