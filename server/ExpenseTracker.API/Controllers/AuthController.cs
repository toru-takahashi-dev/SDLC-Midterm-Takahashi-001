// ExpenseTracker.API/Controllers/AuthController.cs
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ExpenseTracker.API.DTOs;
using ExpenseTracker.Core.Models;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BC = BCrypt.Net.BCrypt;

namespace ExpenseTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ExpenseTrackerDbContext _context;
        private readonly IConfiguration _configuration;
        
        public AuthController(ExpenseTrackerDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                return BadRequest("Email already exists");
                
            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = BC.HashPassword(registerDto.Password)
            };
            
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Registration successful" });
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);
            
            if (user == null || !BC.Verify(loginDto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password");
                
            var token = GenerateJwtToken(user);
            
            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email
                }
            });
        }
        
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 0; // Default to 0 if parsing fails

            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
                return NotFound();
                
            return Ok(new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email
            });
        }
        
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);
            
            if (user == null)
                return Ok(new { message = "If your email exists in our system, you will receive a password reset link" });
                
            // In a real application, generate a reset token and send an email
            // For this example, we'll just return a success message
            
            return Ok(new { message = "If your email exists in our system, you will receive a password reset link" });
        }
        
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };
            
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT key is missing from configuration.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }
    }
    
    public class RegisterDto
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
    
    public class LoginDto
    {
        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
    
    public class ForgotPasswordDto
    {
        [Required]
        public required string Email { get; set; }
    }
}
