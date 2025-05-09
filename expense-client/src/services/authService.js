// src/services/authService.js
import axios from './axiosConfig';
import { loginRequest } from '../authConfig';

const API_URL = '/api/auth';

export const loginUser = async (msalInstance) => {
  try {
    // Initiate login redirect
    await msalInstance.loginRedirect(loginRequest);
  } catch (error) {
    console.error('Azure AD Login failed:', error);
    throw error;
  }
};

export const logoutUser = async (msalInstance) => {
  try {
    await msalInstance.logoutRedirect({
      postLogoutRedirectUri: "/"
    });
  } catch (error) {
    console.error('Logout failed:', error);
    throw error;
  }
};

export const getCurrentUser = async (msalInstance) => {
  try {
    const accounts = msalInstance.getAllAccounts();
    
    if (accounts.length > 0) {
      const account = accounts[0];
      
      // Acquire token silently
      const tokenResponse = await msalInstance.acquireTokenSilent({
        ...loginRequest,
        account: account
      });

      // Fetch user details using the acquired token
      const response = await axios.get(`${API_URL}/me`, {
        headers: {
          'Authorization': `Bearer ${tokenResponse.accessToken}`
        }
      });

      return {
        ...response.data,
        token: tokenResponse.accessToken
      };
    }
    return null;
  } catch (error) {
    console.error('Error fetching current user:', error);
    throw error;
  }
};

// Removed old registration and password reset methods
