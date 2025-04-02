// ExpenseTracker.API/Controllers/ExpensesController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ExpenseTracker.API.DTOs;
using ExpenseTracker.Core.Models;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly ExpenseTrackerDbContext _context;
        
        public ExpensesController(ExpenseTrackerDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 0; // Default to 0 if parsing fails

            
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Date = e.Date,
                    Category = e.Category,
                    Description = e.Description,
                    Amount = e.Amount
                })
                .ToListAsync();
                
            return Ok(expenses);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseDto>> GetExpense(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 0; // Default to 0 if parsing fails

            
            var expense = await _context.Expenses
                .Where(e => e.Id == id && e.UserId == userId)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Date = e.Date,
                    Category = e.Category,
                    Description = e.Description,
                    Amount = e.Amount
                })
                .FirstOrDefaultAsync();
                
            if (expense == null)
                return NotFound();
                
            return expense;
        }
        
        [HttpPost]
        public async Task<ActionResult<ExpenseDto>> CreateExpense(CreateExpenseDto createExpenseDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 0; // Default to 0 if parsing fails

            
            var expense = new Expense
            {
                UserId = userId,
                Date = createExpenseDto.Date,
                Category = createExpenseDto.Category,
                Description = createExpenseDto.Description,
                Amount = createExpenseDto.Amount
            };
            
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, new ExpenseDto
            {
                Id = expense.Id,
                Date = expense.Date,
                Category = expense.Category,
                Description = expense.Description,
                Amount = expense.Amount
            });
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(int id, UpdateExpenseDto updateExpenseDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 0; // Default to 0 if parsing fails

            
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
                
            if (expense == null)
                return NotFound();
                
            expense.Date = updateExpenseDto.Date;
            expense.Category = updateExpenseDto.Category;
            expense.Description = updateExpenseDto.Description;
            expense.Amount = updateExpenseDto.Amount;
            
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 0; // Default to 0 if parsing fails

            
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
                
            if (expense == null)
                return NotFound();
                
            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}

namespace ExpenseTracker.API.DTOs
{
    public class ExpenseDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        [Required]
        public required string Category { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public required decimal Amount { get; set; }
    }
    
    public class CreateExpenseDto
    {
        public DateTime Date { get; set; }
        
        [Required]
        public required string Category { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public required decimal Amount { get; set; }
    }
    
    public class UpdateExpenseDto
    {
        public DateTime Date { get; set; }

        [Required]
        public required string Category { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public required decimal Amount { get; set; }
    }
}
