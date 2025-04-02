// src/services/authService.js
import axios from 'axios';

const API_URL = '/api/auth';

export const registerUser = async (userData) => {
  const response = await axios.post(`${API_URL}/register`, userData);
  return response.data;
};

export const loginUser = async (email, password) => {
  const response = await axios.post(`${API_URL}/login`, { email, password });
  return response.data;
};

export const getCurrentUser = async () => {
  const token = localStorage.getItem('token');
  if (!token) return null;
  
  const response = await axios.get(`${API_URL}/me`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  return response.data;
};

export const requestPasswordReset = async (email) => {
  const response = await axios.post(`${API_URL}/forgot-password`, { email });
  return response.data;
};

export const resetPassword = async (token, password) => {
  const response = await axios.post(`${API_URL}/reset-password`, { token, password });
  return response.data;
};
