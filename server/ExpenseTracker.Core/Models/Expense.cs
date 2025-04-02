// ExpenseTracker.Core/Models/Expense.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Core.Models
{
    public class Expense
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
        
        [StringLength(255)]
        public string Description { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual User User { get; set; }
    }
}
