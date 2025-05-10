using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseTracker.Core.Models;
using ExpenseTracker.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Core.Enums;
using ExpenseTracker.API.DTOs;


namespace ExpenseTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerExpensesController : ControllerBase
    {
        private readonly ExpenseTrackerDbContext _context;

        public ManagerExpensesController(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        // GET: api/ManagerExpenses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetAllExpenses(
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] string sortBy = "date",
            [FromQuery] bool descending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Expenses.AsQueryable();

            // Date range filter
            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(e => e.Date <= endDate.Value);

            // Amount range filter
            if (minAmount.HasValue)
                query = query.Where(e => e.Amount >= minAmount.Value);
            
            if (maxAmount.HasValue)
                query = query.Where(e => e.Amount <= maxAmount.Value);

            // Sorting
            query = sortBy.ToLower() switch
            {
                "amount" => descending 
                    ? query.OrderByDescending(e => e.Amount)
                    : query.OrderBy(e => e.Amount),
                "date" => descending 
                    ? query.OrderByDescending(e => e.Date)
                    : query.OrderBy(e => e.Date),
                _ => descending 
                    ? query.OrderByDescending(e => e.Date)
                    : query.OrderBy(e => e.Date)
            };

            // Pagination
            var totalCount = await query.CountAsync();
            var expenses = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(e => e.User) // Include user details if needed
                .ToListAsync();

            

            return Ok(new 
            {
                TotalExpenses = totalCount,
                Page = page,
                PageSize = pageSize,
                Expenses = expenses.Select(x => new {
                    Id = x.Id,
                    Date = x.Date,
                    Category = x.Category,
                    Description = x.Description,
                    Amount = x.Amount,
                    User = new UserResponse{
                        Id = x.User.Id,
                        Email = x.User.Email,
                        Name = x.User.Name
                    }
                }).ToList()
                
            });
        }

        // GET: api/ManagerExpenses/summary
        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetExpensesSummary(
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.Expenses.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(e => e.Date <= endDate.Value);

            var summary = new
            {
                TotalExpenses = await query.SumAsync(e => e.Amount),
                AverageExpense = await query.AverageAsync(e => e.Amount),
                ExpenseCount = await query.CountAsync(),
                TopExpenseCategories = await query
                    .GroupBy(e => e.Category)
                    .Select(g => new 
                    { 
                        Category = g.Key, 
                        TotalAmount = g.Sum(e => e.Amount) 
                    })
                    .OrderByDescending(x => x.TotalAmount)
                    .Take(5)
                    .ToListAsync()
            };

            return Ok(summary);
        }

        // GET: api/ManagerExpenses/export
        [HttpGet("export")]
        public async Task<FileResult> ExportExpenses(
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.Expenses.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(e => e.Date <= endDate.Value);

            var expenses = await query
                .Include(e => e.User)
                .ToListAsync();

            // Convert to CSV 
            var csvContent = GenerateCSV(expenses);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);

            return File(bytes, "text/csv", $"Expenses_Export_{DateTime.Now:yyyyMMdd}.csv");
        }

        private string GenerateCSV(List<Expense> expenses)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Date,User,Category,Amount,Description");
            
            foreach (var expense in expenses)
            {
                csv.AppendLine($"{expense.Date:yyyy-MM-dd},{expense.User.Name},{expense.Category},{expense.Amount},{expense.Description}");
            }

            return csv.ToString();
        }

        // New method: Get expenses by specific user
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Expense>>> GetUserExpenses(
            int userId,
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound($"User with ID {userId} not found");

            var query = _context.Expenses
                .Where(e => e.UserId == userId)
                .AsQueryable();

            // Date range filter
            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(e => e.Date <= endDate.Value);

            var totalCount = await query.CountAsync();
            var expenses = await query
                .OrderByDescending(e => e.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new 
            {
                User = new { user.Id, user.Name, user.Email },
                TotalExpenses = totalCount,
                Page = page,
                PageSize = pageSize,
                Expenses = expenses
            });
        }

        // New method: Bulk expense approval
        [HttpPost("approve")]
        public async Task<ActionResult> ApproveExpenses([FromBody] List<int> expenseIds)
        {
            if (expenseIds == null || !expenseIds.Any())
                return BadRequest("No expense IDs provided");

            var expensesToApprove = await _context.Expenses
                .Where(e => expenseIds.Contains(e.Id))
                .ToListAsync();

            if (expensesToApprove.Count != expenseIds.Count)
                return BadRequest("Some expense IDs are invalid");

            foreach (var expense in expensesToApprove)
            {
                // Assuming you have an ApprovalStatus enum or property
                expense.ApprovalStatus = ApprovalStatus.Approved;
                expense.ApprovedBy = User.Identity.Name; // Current manager's username
                expense.ApprovedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                Message = $"Successfully approved {expensesToApprove.Count} expenses" 
            });
        }

        // New method: Bulk expense rejection
        [HttpPost("reject")]
        public async Task<ActionResult> RejectExpenses(
            [FromBody] ExpenseRejectionRequest rejectionRequest)
        {
            if (rejectionRequest?.ExpenseIds == null || !rejectionRequest.ExpenseIds.Any())
                return BadRequest("No expense IDs provided");

            var expensesToReject = await _context.Expenses
                .Where(e => rejectionRequest.ExpenseIds.Contains(e.Id))
                .ToListAsync();

            if (expensesToReject.Count != rejectionRequest.ExpenseIds.Count)
                return BadRequest("Some expense IDs are invalid");

            foreach (var expense in expensesToReject)
            {
                expense.ApprovalStatus = ApprovalStatus.Rejected;
                expense.ApprovedBy = User.Identity.Name;
                expense.ApprovedAt = DateTime.UtcNow;
                expense.RejectionReason = rejectionRequest.RejectionReason;
            }

            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                Message = $"Successfully rejected {expensesToReject.Count} expenses" 
            });
        }

        // New method: Get expenses pending approval
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<Expense>>> GetPendingExpenses(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Expenses
                .Where(e => e.ApprovalStatus == ApprovalStatus.Pending)
                .Include(e => e.User)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var pendingExpenses = await query
                .OrderBy(e => e.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new 
            {
                TotalPendingExpenses = totalCount,
                Page = page,
                PageSize = pageSize,
                Expenses = pendingExpenses
            });
        }
    }

    // Supporting classes for request models
    public class ExpenseRejectionRequest
    {
        public List<int> ExpenseIds { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class UserResponse{

        public int Id { get; set; }
        
        public required string Name { get; set; }
        public required string Email { get; set; }



    }

    
}
