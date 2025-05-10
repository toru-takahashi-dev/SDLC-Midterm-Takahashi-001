using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Core.Models; // Assuming your models are in this namespace

namespace ExpenseTracker.Data
{
    public class ExpenseTrackerDbContext : DbContext
    {
        public ExpenseTrackerDbContext(DbContextOptions<ExpenseTrackerDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Add any specific configuration for your entities
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Expense>()
                .HasKey(e => e.Id);
        }
    }
}
