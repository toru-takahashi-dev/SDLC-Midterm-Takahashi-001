// ExpenseTracker.API/Controllers/AuthController.cs
using System.Security.Claims;
using ExpenseTracker.Core.Models;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ExpenseTrackerDbContext _context;
        
        public AuthController(ExpenseTrackerDbContext context)
        {
            _context = context;
        }
        
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
            {
                // If user doesn't exist in local database, create a new user
                user = new User
                {
                    Email = email,
                    Name = User.FindFirst(ClaimTypes.Name)?.Value ?? email
                };
                
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
                
            return Ok(new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email
            });
        }
    }
}
