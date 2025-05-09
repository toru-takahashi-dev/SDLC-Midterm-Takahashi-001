// src/services/axiosConfig.js
import axios from 'axios';

// Get the API URL from environment variables
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5016';

console.log(API_BASE_URL);

const instance = axios.create({
  baseURL: API_BASE_URL,
});

// Add the auth token interceptor
instance.interceptors.request.use(
  config => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  error => Promise.reject(error)
);

export default instance;
