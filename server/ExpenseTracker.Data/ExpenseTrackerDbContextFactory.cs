using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ExpenseTracker.Data
{
    public class ExpenseTrackerDbContextFactory : IDesignTimeDbContextFactory<ExpenseTrackerDbContext>
    {
        public ExpenseTrackerDbContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings.json
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
            var configuration = builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Get the connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Create DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<ExpenseTrackerDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ExpenseTrackerDbContext(optionsBuilder.Options);
        }
    }
}
