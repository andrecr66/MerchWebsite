using MerchWebsite.API.Entities; // To access the Product entity
using Microsoft.EntityFrameworkCore;

namespace MerchWebsite.API.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor that accepts DbContextOptions, allowing configuration (like connection string)
        // to be passed in from Program.cs (dependency injection)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet represents the collection of all entities in the context,
        // or that can be queried from the database, of a given type.
        // EF Core will create a table named "Products" based on this DbSet
        // and the Product entity configuration.
        public DbSet<Product> Products { get; set; }

        // We can add more DbSets here for other entities later (e.g., Users, Orders)
        // public DbSet<User> Users { get; set; }
        // public DbSet<Order> Orders { get; set; }

        // Optionally, override OnModelCreating for more complex configurations
        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder);
        //     // Add custom model configurations here if needed
        // }
    }
}
