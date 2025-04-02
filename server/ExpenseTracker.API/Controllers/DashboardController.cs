// ExpenseTracker.API/Controllers/DashboardController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ExpenseTrackerDbContext _context;
        
        public DashboardController(ExpenseTrackerDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetDashboardData([FromQuery] string timeFrame = "month")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 0; // Default to 0 if parsing fails

            
            DateTime startDate;
            DateTime endDate = DateTime.UtcNow;
            
            switch (timeFrame.ToLower())
            {
                case "day":
                    startDate = DateTime.UtcNow.Date;
                    break;
                case "month":
                    startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                    break;
                case "year":
                    startDate = new DateTime(DateTime.UtcNow.Year, 1, 1);
                    break;
                default:
                    startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                    break;
            }
            
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();
                
            var recentExpenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Take(5)
                .Select(e => new
                {
                    e.Id,
                    e.Date,
                    e.Category,
                    e.Description,
                    e.Amount
                })
                .ToListAsync();
                
            // Calculate summary data
            var total = expenses.Sum(e => e.Amount);
            
            var average = 0m;
            if (expenses.Any())
            {
                switch (timeFrame.ToLower())
                {
                    case "day":
                        // Average per hour (assuming 24 hours in a day)
                        average = total / 24;
                        break;
                    case "month":
                        // Average per day
                        var daysInMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
                        average = total / daysInMonth;
                        break;
                    case "year":
                        // Average per month
                        average = total / 12;
                        break;
                }
            }
            
            var highestExpense = expenses.OrderByDescending(e => e.Amount).FirstOrDefault();
            
            var topCategory = expenses
                .GroupBy(e => e.Category)
                .Select(g => new { Name = g.Key, Total = g.Sum(e => e.Amount) })
                .OrderByDescending(g => g.Total)
                .FirstOrDefault();
                
            // Prepare time series data
            var timeSeriesData = new Dictionary<string, decimal>();
            
            switch (timeFrame.ToLower())
            {
                case "day":
                    // Group by hour
                    for (int hour = 0; hour < 24; hour++)
                    {
                        var hourStart = startDate.AddHours(hour);
                        var hourEnd = hourStart.AddHours(1);
                        var hourTotal = expenses
                            .Where(e => e.Date >= hourStart && e.Date < hourEnd)
                            .Sum(e => e.Amount);
                            
                        timeSeriesData.Add($"{hour}:00", hourTotal);
                    }
                    break;
                case "month":
                    // Group by day
                    var daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        var dayDate = new DateTime(startDate.Year, startDate.Month, day);
                        var dayTotal = expenses
                            .Where(e => e.Date.Date == dayDate.Date)
                            .Sum(e => e.Amount);
                            
                        timeSeriesData.Add(dayDate.ToString("MM/dd"), dayTotal);
                    }
                    break;
                case "year":
                    // Group by month
                    for (int month = 1; month <= 12; month++)
                    {
                        var monthStart = new DateTime(startDate.Year, month, 1);
                        var monthEnd = monthStart.AddMonths(1);
                        var monthTotal = expenses
                            .Where(e => e.Date >= monthStart && e.Date < monthEnd)
                            .Sum(e => e.Amount);
                            
                        timeSeriesData.Add(monthStart.ToString("MMM"), monthTotal);
                    }
                    break;
            }
            
            // Prepare category data
            var categoryData = expenses
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
                .OrderByDescending(g => g.Total)
                .ToDictionary(g => g.Category, g => g.Total);
                
            return Ok(new
            {
                summary = new
                {
                    total,
                    average,
                    highest = highestExpense != null
                        ? new { amount = highestExpense.Amount, category = highestExpense.Category }
                        : null,
                    topCategory = topCategory != null
                        ? new { name = topCategory.Name, total = topCategory.Total }
                        : null
                },
                chartData = new
                {
                    timeSeries = new
                    {
                        labels = timeSeriesData.Keys.ToList(),
                        values = timeSeriesData.Values.ToList()
                    },
                    byCategory = new
                    {
                        labels = categoryData.Keys.ToList(),
                        values = categoryData.Values.ToList()
                    }
                },
                recentExpenses
            });
        }
    }
}
