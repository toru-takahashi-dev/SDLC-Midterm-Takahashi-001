// src/services/expenseService.js
import axios from 'axios';

const API_URL = '/api/expenses';

// Configure axios to send the auth token with every request
axios.interceptors.request.use(
  config => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  error => Promise.reject(error)
);

export const getExpenses = async () => {
  const response = await axios.get(API_URL);
  return response.data;
};

export const getExpenseById = async (id) => {
  const response = await axios.get(`${API_URL}/${id}`);
  return response.data;
};

export const addExpense = async (expenseData) => {
  const response = await axios.post(API_URL, expenseData);
  return response.data;
};

export const updateExpense = async (id, expenseData) => {
  const response = await axios.put(`${API_URL}/${id}`, expenseData);
  return response.data;
};

export const deleteExpense = async (id) => {
  const response = await axios.delete(`${API_URL}/${id}`);
  return response.data;
};
