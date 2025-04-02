// src/components/ExpenseForm.jsx
import React, { useState, useEffect } from 'react';
import { addExpense, updateExpense } from '../services/expenseService';
import './ExpenseForm.css';

const ExpenseForm = ({ expense = null, onSuccess, onCancel }) => {
  const [formData, setFormData] = useState({
    date: new Date().toISOString().substr(0, 10),
    category: '',
    description: '',
    amount: ''
  });
  const [error, setError] = useState('');

  useEffect(() => {
    if (expense) {
      setFormData({
        date: new Date(expense.date).toISOString().substr(0, 10),
        category: expense.category,
        description: expense.description,
        amount: expense.amount.toString()
      });
    }
  }, [expense]);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const validateForm = () => {
    if (!formData.date || !formData.category || !formData.amount) {
      setError('Please fill in all required fields');
      return false;
    }
    if (isNaN(parseFloat(formData.amount)) || parseFloat(formData.amount) <= 0) {
      setError('Amount must be a positive number');
      return false;
    }
    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;

    try {
      const expenseData = {
        ...formData,
        amount: parseFloat(formData.amount)
      };

      if (expense) {
        await updateExpense(expense.id, expenseData);
      } else {
        await addExpense(expenseData);
      }
      
      onSuccess();
    } catch (err) {
      setError('Failed to save expense');
    }
  };

  return (
    <div className="expense-form-container">
      <h3>{expense ? 'Edit Expense' : 'Add New Expense'}</h3>
      {error && <div className="error-message">{error}</div>}
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="date">Date*</label>
          <input
            type="date"
            id="date"
            name="date"
            value={formData.date}
            onChange={handleChange}
            required
          />
        </div>
        <div className="form-group">
          <label htmlFor="category">Category*</label>
          <select
            id="category"
            name="category"
            value={formData.category}
            onChange={handleChange}
            required
          >
            <option value="">Select a category</option>
            <option value="Food">Food</option>
            <option value="Transportation">Transportation</option>
            <option value="Housing">Housing</option>
            <option value="Utilities">Utilities</option>
            <option value="Entertainment">Entertainment</option>
            <option value="Healthcare">Healthcare</option>
            <option value="Other">Other</option>
          </select>
        </div>
        <div className="form-group">
          <label htmlFor="description">Description</label>
          <input
            type="text"
            id="description"
            name="description"
            value={formData.description}
            onChange={handleChange}
          />
        </div>
        <div className="form-group">
          <label htmlFor="amount">Amount*</label>
          <input
            type="number"
            id="amount"
            name="amount"
            value={formData.amount}
            onChange={handleChange}
            step="0.01"
            min="0.01"
            required
          />
        </div>
        <div className="form-actions">
          <button type="submit" className="btn-primary">
            {expense ? 'Update Expense' : 'Add Expense'}
          </button>
          {onCancel && (
            <button type="button" className="btn-secondary" onClick={onCancel}>
              Cancel
            </button>
          )}
        </div>
      </form>
    </div>
  );
};

export default ExpenseForm;
