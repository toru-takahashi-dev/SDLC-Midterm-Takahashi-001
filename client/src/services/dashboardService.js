// src/services/dashboardService.js
import axios from 'axios';

const API_URL = '/api/dashboard';

export const getDashboardData = async (timeFrame = 'month') => {
  const response = await axios.get(`${API_URL}?timeFrame=${timeFrame}`, {
    headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
  });
  return response.data;
};
