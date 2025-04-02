// src/components/ExpenseList.jsx
import React, { useState, useEffect } from 'react';
import { getExpenses, deleteExpense } from '../services/expenseService';
import ExpenseForm from './ExpenseForm';

const ExpenseList = () => {
  const [expenses, setExpenses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [editingExpense, setEditingExpense] = useState(null);

  useEffect(() => {
    fetchExpenses();
  }, []);

  const fetchExpenses = async () => {
    try {
      setLoading(true);
      const data = await getExpenses();
      setExpenses(data);
      setError('');
    } catch (err) {
      setError('Failed to fetch expenses');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    try {
      await deleteExpense(id);
      setExpenses(expenses.filter(expense => expense.id !== id));
    } catch (err) {
      setError('Failed to delete expense');
    }
  };

  const handleEdit = (expense) => {
    setEditingExpense(expense);
  };

  const handleExpenseUpdated = () => {
    setEditingExpense(null);
    fetchExpenses();
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div className="error-message">{error}</div>;

  return (
    <div className="expense-list-container">
      <h2>Your Expenses</h2>
      {editingExpense ? (
        <ExpenseForm 
          expense={editingExpense} 
          onSuccess={handleExpenseUpdated} 
          onCancel={() => setEditingExpense(null)} 
        />
      ) : (
        <table className="expense-table">
          <thead>
            <tr>
              <th>Date</th>
              <th>Category</th>
              <th>Description</th>
              <th>Amount</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {expenses.map(expense => (
              <tr key={expense.id}>
                <td>{new Date(expense.date).toLocaleDateString()}</td>
                <td>{expense.category}</td>
                <td>{expense.description}</td>
                <td>${expense.amount.toFixed(2)}</td>
                <td>
                  <button 
                    className="btn-edit" 
                    onClick={() => handleEdit(expense)}
                  >
                    Edit
                  </button>
                  <button 
                    className="btn-delete" 
                    onClick={() => handleDelete(expense.id)}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default ExpenseList;
