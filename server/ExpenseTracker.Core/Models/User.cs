// ExpenseTracker.Core/Models/User.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public required string Email { get; set; }
        
        [Required]
        public required string PasswordHash { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<Expense> Expenses { get; set; }
    }
}
