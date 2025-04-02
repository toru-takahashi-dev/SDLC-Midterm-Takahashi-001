// src/components/ExpenseSummary.jsx
import React from 'react';

const ExpenseSummary = ({ summary, timeFrame }) => {
  return (
    <div className="expense-summary">
      <div className="summary-card total">
        <h3>Total Expenses</h3>
        <p className="amount">${summary.total.toFixed(2)}</p>
        <p className="period">{timeFrame === 'day' ? 'Today' : timeFrame === 'month' ? 'This Month' : 'This Year'}</p>
      </div>
      
      <div className="summary-card average">
        <h3>Average Per {timeFrame === 'day' ? 'Hour' : timeFrame === 'month' ? 'Day' : 'Month'}</h3>
        <p className="amount">${summary.average.toFixed(2)}</p>
      </div>
      
      <div className="summary-card highest">
        <h3>Highest Expense</h3>
        <p className="amount">${summary.highest.amount.toFixed(2)}</p>
        <p className="category">{summary.highest.category}</p>
      </div>
      
      <div className="summary-card top-category">
        <h3>Top Category</h3>
        <p className="category">{summary.topCategory.name}</p>
        <p className="amount">${summary.topCategory.total.toFixed(2)}</p>
      </div>
    </div>
  );
};

export default ExpenseSummary;
