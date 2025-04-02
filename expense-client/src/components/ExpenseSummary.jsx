// src/components/ExpenseSummary.jsx
import React from 'react';

const ExpenseSummary = ({ summary, timeFrame }) => {
  // Guard clause - if summary is null/undefined, show a loading state
  if (!summary) {
    return <div className="expense-summary loading">Loading summary data...</div>;
  }

  // Helper function to safely format amounts
  const formatAmount = (value) => {
    return value != null ? `$${Number(value).toFixed(2)}` : 'N/A';
  };

  // Determine time period text
  const timePeriodText = 
    timeFrame === 'day' ? '[DateTime]' : 
    timeFrame === 'month' ? 'This Month' : 
    'This Year';

  // Determine average period text
  const averagePeriodText = 
    timeFrame === 'day' ? 'Hour' : 
    timeFrame === 'month' ? 'Day' : 
    'Month';

  return (
    <div className="expense-summary">
      <div className="summary-card total">
        <h3>Total Expenses</h3>
        <p className="amount">{summary.total != null ? formatAmount(summary.total) : 'No data'}</p>
        <p className="period">{timePeriodText}</p>
      </div>
      
      <div className="summary-card average">
        <h3>Average Per {averagePeriodText}</h3>
        <p className="amount">{summary.average != null ? formatAmount(summary.average) : 'No data'}</p>
      </div>
      
      {/* The error is likely happening in this section */}
      <div className="summary-card highest">
        <h3>Highest Expense</h3>
        <p className="amount">
          {summary.highest && typeof summary.highest === 'object' && summary.highest.amount != null 
            ? formatAmount(summary.highest.amount) 
            : 'No data'}
        </p>
        <p className="category">
          {summary.highest && typeof summary.highest === 'object' && summary.highest.category 
            ? summary.highest.category 
            : 'None'}
        </p>
      </div>
      
      <div className="summary-card top-category">
        <h3>Top Category</h3>
        <p className="category">
          {summary.topCategory && typeof summary.topCategory === 'object' && summary.topCategory.name 
            ? summary.topCategory.name 
            : 'None'}
        </p>
        <p className="amount">
          {summary.topCategory && typeof summary.topCategory === 'object' && summary.topCategory.total != null 
            ? formatAmount(summary.topCategory.total) 
            : 'No data'}
        </p>
      </div>
    </div>
  );
};

export default ExpenseSummary;
