// src/pages/Dashboard.jsx
import React, { useState, useEffect } from 'react';
import { getDashboardData } from '../services/dashboardService';
import ExpenseChart from '../components/ExpenseChart';
import ExpenseSummary from '../components/ExpenseSummary';
import ExpenseForm from '../components/ExpenseForm';
import ExpenseList from '../components/ExpenseList';

const Dashboard = () => {
  const [dashboardData, setDashboardData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showAddExpenseForm, setShowAddExpenseForm] = useState(false);
  const [timeFrame, setTimeFrame] = useState('month'); // 'day', 'month', 'year'

  useEffect(() => {
    fetchDashboardData();
  }, [timeFrame]);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      const data = await getDashboardData(timeFrame);
      setDashboardData(data);
      setError('');
    } catch (err) {
      setError('Failed to fetch dashboard data');
    } finally {
      setLoading(false);
    }
  };

  const handleExpenseAdded = () => {
    setShowAddExpenseForm(false);
    fetchDashboardData();
  };

  if (loading) return <div>Loading dashboard...</div>;
  if (error) return <div className="error-message">{error}</div>;

  return (
    <div className="dashboard-container">
      <h1>Expense Dashboard</h1>
      
      <div className="dashboard-actions">
        <button 
          className="btn-primary" 
          onClick={() => setShowAddExpenseForm(!showAddExpenseForm)}
        >
          {showAddExpenseForm ? 'Cancel' : 'Add New Expense'}
        </button>
        <div className="time-frame-selector">
          <button 
            className={`btn-filter ${timeFrame === 'day' ? 'active' : ''}`}
            onClick={() => setTimeFrame('day')}
          >
            Daily
          </button>
          <button 
            className={`btn-filter ${timeFrame === 'month' ? 'active' : ''}`}
            onClick={() => setTimeFrame('month')}
          >
            Monthly
          </button>
          <button 
            className={`btn-filter ${timeFrame === 'year' ? 'active' : ''}`}
            onClick={() => setTimeFrame('year')}
          >
            Yearly
          </button>
        </div>
      </div>

      {showAddExpenseForm && (
        <div className="add-expense-section">
          <ExpenseForm onSuccess={handleExpenseAdded} onCancel={() => setShowAddExpenseForm(false)} />
        </div>
      )}

      {dashboardData && (
        <>
          <div className="dashboard-summary">
            <ExpenseSummary summary={dashboardData.summary} timeFrame={timeFrame} />
          </div>
          
          <div className="dashboard-charts">
            <ExpenseChart 
              data={dashboardData.chartData} 
              timeFrame={timeFrame} 
            />
          </div>
          
          <div className="recent-expenses">
            <h2>Recent Expenses</h2>
            <ExpenseList expenses={dashboardData.recentExpenses} onDataChange={fetchDashboardData} />
          </div>
        </>
      )}
    </div>
  );
};

export default Dashboard;
