// ExpenseTracker.Tests/ExpenseControllerTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ExpenseTracker.API.Controllers;
using ExpenseTracker.API.DTOs;
using ExpenseTracker.Core.Models;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseTracker.Tests
{
    public class ExpenseControllerTests
    {
        private ExpenseTrackerDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
                
            var context = new ExpenseTrackerDbContext(options);
            
            // Seed data
            var user = new User
            {
                Id = 1,
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword"
            };
            
            context.Users.Add(user);
            
            context.Expenses.AddRange(
                new Expense
                {
                    Id = 1,
                    UserId = 1,
                    Date = DateTime.UtcNow.AddDays(-1),
                    Category = "Food",
                    Description = "Lunch",
                    Amount = 10.99m
                },
                new Expense
                {
                    Id = 2,
                    UserId = 1,
                    Date = DateTime.UtcNow.AddDays(-2),
                    Category = "Transportation",
                    Description = "Taxi",
                    Amount = 25.50m
                }
            );
            
            context.SaveChanges();
            
            return context;
        }
        
        private ExpensesController GetController(ExpenseTrackerDbContext context)
        {
            var controller = new ExpensesController(context);
            
            // Setup user identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
            
            return controller;
        }
        
        [Fact]
        public async Task GetExpenses_ReturnsAllExpensesForUser()
        {
            // Arrange
            var context = GetDbContext();
            var controller = GetController(context);
            
            // Act
            var result = await controller.GetExpenses();
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var expenses = Assert.IsType<List<ExpenseDto>>(okResult.Value);
            Assert.Equal(2, expenses.Count);
        }
        
        [Fact]
        public async Task GetExpense_ReturnsExpenseWhenExists()
        {
            // Arrange
            var context = GetDbContext();
            var controller = GetController(context);
            
            // Act
            var result = await controller.GetExpense(1);
            
            // Assert
            var expense = Assert.IsType<ExpenseDto>(result.Value);
            Assert.Equal(1, expense.Id);
            Assert.Equal("Food", expense.Category);
        }
        
        [Fact]
        public async Task CreateExpense_AddsExpenseToDatabase()
        {
            // Arrange
            var context = GetDbContext();
            var controller = GetController(context);
            var newExpense = new CreateExpenseDto
            {
                Date = DateTime.UtcNow,
                Category = "Entertainment",
                Description = "Movie",
                Amount = 15.00m
            };
            
            // Act
            var result = await controller.CreateExpense(newExpense);
            
            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedExpense = Assert.IsType<ExpenseDto>(createdResult.Value);
            Assert.Equal("Entertainment", returnedExpense.Category);
            
            // Verify it was added to the database
            var dbExpense = await context.Expenses.FindAsync(returnedExpense.Id);
            Assert.NotNull(dbExpense);
            Assert.Equal("Entertainment", dbExpense.Category);
        }
        
        [Fact]
        public async Task UpdateExpense_ModifiesExistingExpense()
        {
            // Arrange
            var context = GetDbContext();
            var controller = GetController(context);
            var updateExpense = new UpdateExpenseDto
            {
                Date = DateTime.UtcNow,
                Category = "Food",
                Description = "Dinner",
                Amount = 30.00m
            };
            
            // Act
            var result = await controller.UpdateExpense(1, updateExpense);
            
            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Verify it was updated in the database
            var dbExpense = await context.Expenses.FindAsync(1);
            Assert.NotNull(dbExpense); // First verify it's not null
            Assert.Equal("Dinner", dbExpense!.Description); // Use null-forgiving operator
            Assert.Equal(30.00m, dbExpense.Amount);
        }
        
        [Fact]
        public async Task DeleteExpense_RemovesExpenseFromDatabase()
        {
            // Arrange
            var context = GetDbContext();
            var controller = GetController(context);
            
            // Act
            var result = await controller.DeleteExpense(1);
            
            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Verify it was removed from the database
            var dbExpense = await context.Expenses.FindAsync(1);
            Assert.Null(dbExpense);
        }
    }
}
