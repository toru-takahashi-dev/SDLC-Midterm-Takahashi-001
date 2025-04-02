// src/components/ExpenseChart.jsx
import React from 'react';
import { Line, Bar, Pie } from 'react-chartjs-2';
import './ExpenseChart.css';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';

// Register ChartJS components
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend
);

const ExpenseChart = ({ data, timeFrame }) => {
  const renderTimeSeriesChart = () => {
    const chartData = {
      labels: data.timeSeries.labels,
      datasets: [
        {
          label: 'Expenses',
          data: data.timeSeries.values,
          fill: false,
          backgroundColor: 'rgba(75, 192, 192, 0.2)',
          borderColor: 'rgba(75, 192, 192, 1)',
          tension: 0.1
        }
      ]
    };

    const options = {
      responsive: true,
      plugins: {
        legend: {
          position: 'top',
        },
        title: {
          display: true,
          text: `Expenses Over ${timeFrame === 'day' ? 'Day' : timeFrame === 'month' ? 'Month' : 'Year'}`
        }
      }
    };

    return <Line data={chartData} options={options} />;
  };

  const renderCategoryChart = () => {
    const chartData = {
      labels: data.byCategory.labels,
      datasets: [
        {
          label: 'Expenses by Category',
          data: data.byCategory.values,
          backgroundColor: [
            'rgba(255, 99, 132, 0.6)',
            'rgba(54, 162, 235, 0.6)',
            'rgba(255, 206, 86, 0.6)',
            'rgba(75, 192, 192, 0.6)',
            'rgba(153, 102, 255, 0.6)',
            'rgba(255, 159, 64, 0.6)',
            'rgba(199, 199, 199, 0.6)'
          ],
          borderColor: [
            'rgba(255, 99, 132, 1)',
            'rgba(54, 162, 235, 1)',
            'rgba(255, 206, 86, 1)',
            'rgba(75, 192, 192, 1)',
            'rgba(153, 102, 255, 1)',
            'rgba(255, 159, 64, 1)',
            'rgba(199, 199, 199, 1)'
          ],
          borderWidth: 1
        }
      ]
    };

    return (
      <div className="category-chart">
        <h3>Expenses by Category</h3>
        <Pie data={chartData} />
      </div>
    );
  };

  return (
    <div className="charts-container">
      <div className="time-series-chart">
        {renderTimeSeriesChart()}
      </div>
      <div className="category-chart">
        {renderCategoryChart()}
      </div>
    </div>
  );
};

export default ExpenseChart;
