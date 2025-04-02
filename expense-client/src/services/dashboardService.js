// src/services/dashboardService.js
import axios from './axiosConfig';

const API_URL = '/api/dashboard';

export const getDashboardData = async (timeFrame = 'month') => {
  try {
    const response = await axios.get(`${API_URL}?timeFrame=${timeFrame}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching dashboard data for timeframe ${timeFrame}:`, error);
    throw error;
  }
};