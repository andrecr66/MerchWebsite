// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Data/AppDbContext.cs
using MerchWebsite.API.Entities; // Make sure this is present
using Microsoft.AspNetCore.Identity; // Need this for IdentityRole seeding
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MerchWebsite.API.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        // --- ADD DbSets for Cart and CartItem ---
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        // --- END ADDED DbSets ---

        // --- MODIFY: Uncomment and enhance OnModelCreating ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // MUST call base method for Identity

            // Optional: Add Role seeding if you did this in Phase 3
            // If you didn't seed roles before, you can omit this part

            // --- MODIFIED: Use STATIC Guids for Role Seeding ---
            modelBuilder.Entity<IdentityRole>().HasData(
                 new IdentityRole
                 {
                     // Use a fixed, static GUID string for the ID
                     Id = "c7b013f0-5201-4317-abd8-c211f91b7331", // Example Static GUID for Member
                     Name = "Member",
                     NormalizedName = "MEMBER",
                     // Use a fixed, static string for the ConcurrencyStamp
                     ConcurrencyStamp = "18b4a3d1-f1e4-4f2d-b0f5-c8e0b7a1e5b8" // Example Static Stamp
                 },
                 new IdentityRole
                 {
                     // Use a different fixed, static GUID string for the ID
                     Id = "c7b013f0-5201-4317-abd8-c211f91b7330", // Example Static GUID for Admin
                     Name = "Admin",
                     NormalizedName = "ADMIN",
                     // Use a different fixed, static string for the ConcurrencyStamp
                     ConcurrencyStamp = "a2b9c4e2-g2f5-5a3e-c1a6-d9f1c8b2f6c9" // Example Static Stamp
                 }
            );
            // --- END MODIFIED Role Seeding ---

            // --- ADD configurations for Cart ---
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .IsUnique(); // Ensure only one Cart per User

            // Optional: Explicitly define relationships if needed, but conventions
            // should work here based on the Entity definitions.
            // modelBuilder.Entity<CartItem>()
            //     .HasOne(ci => ci.Cart)
            //     .WithMany(c => c.Items)
            //     .HasForeignKey(ci => ci.CartId);
            //
            // modelBuilder.Entity<CartItem>()
            //     .HasOne(ci => ci.Product)
            //     .WithMany()
            //     .HasForeignKey(ci => ci.ProductId);
            // --- END ADDED configurations ---
        }
        // --- END MODIFIED OnModelCreating ---
    }
}