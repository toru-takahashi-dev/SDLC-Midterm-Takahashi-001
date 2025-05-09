// src/context/AuthContext.jsx
import React, { createContext, useState, useEffect } from 'react';
import { getCurrentUser } from '../services/authService';
import { useMsal } from "@azure/msal-react";

export const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const { instance } = useMsal();

  useEffect(() => {
    const loadUser = async () => {
      try {
        // Check if user is authenticated with MSAL
        const accounts = instance.getAllAccounts();
        if (accounts.length > 0) {
          const currentAccount = accounts[0];
          setUser(currentAccount);
          
          // Optional: Validate token or fetch additional user details
          // const userData = await getCurrentUser();
          // setUser(userData);
        }
      } catch (error) {
        console.error('Authentication check failed', error);
        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    loadUser();
  }, [instance]);

  // In AuthContext.jsx
  const logout = () => {
    // Clear local storage
    localStorage.removeItem('token');
    // Reset user state
    setUser(null);
  };


  return (
    <AuthContext.Provider value={{ 
      user, 
      setUser, 
      loading, 
      logout,
      isAuthenticated: !!user 
    }}>
      {children}
    </AuthContext.Provider>
  );
};

