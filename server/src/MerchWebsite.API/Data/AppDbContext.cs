using MerchWebsite.API.Entities; // To access the Product entity
// using Microsoft.AspNetCore.Identity; // Remove this line
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Add this for IdentityDbContext
using Microsoft.EntityFrameworkCore;

namespace MerchWebsite.API.Data
{
    // Inherit from IdentityDbContext<User> - let it infer Role and Key type
    public class AppDbContext : IdentityDbContext<User>
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

        // IdentityDbContext handles User, Role, Claim etc. DbSets automatically.
        // We can add more DbSets here for other custom entities later (e.g., Orders)
        // public DbSet<Order> Orders { get; set; }

        // Optionally, override OnModelCreating for more complex configurations
        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder);
        //     // Add custom model configurations here if needed
        // }
    }
}
