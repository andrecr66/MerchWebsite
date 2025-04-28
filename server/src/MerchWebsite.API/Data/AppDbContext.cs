// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Data/AppDbContext.cs
using MerchWebsite.API.Entities;
using Microsoft.AspNetCore.Identity;
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
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // --- ADD DbSets for Order and OrderItem ---
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        // --- END ADDED DbSets ---
        // Inside AppDbContext class
        // ... other DbSets ...
        public DbSet<Review> Reviews { get; set; } // <<< ADD THIS DbSet
                                                   // ... OnModelCreating ...

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // MUST call base method for Identity

            // --- Keep existing configurations ---
            modelBuilder.Entity<IdentityRole>().HasData(
                 new IdentityRole
                 {
                     Id = "c7b013f0-5201-4317-abd8-c211f91b7331", // Static GUID for Member
                     Name = "Member",
                     NormalizedName = "MEMBER",
                     ConcurrencyStamp = "18b4a3d1-f1e4-4f2d-b0f5-c8e0b7a1e5b8" // Static Stamp
                 },
                 new IdentityRole
                 {
                     Id = "c7b013f0-5201-4317-abd8-c211f91b7330", // Static GUID for Admin
                     Name = "Admin",
                     NormalizedName = "ADMIN",
                     ConcurrencyStamp = "a2b9c4e2-g2f5-5a3e-c1a6-d9f1c8b2f6c9" // Static Stamp
                 }
            );

            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .IsUnique();
            // --- End existing configurations ---


            // --- ADD configurations for Order and OrderItem ---

            // Configure decimal precision for monetary values in Order
            modelBuilder.Entity<Order>()
                .Property(o => o.Subtotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingFee)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
               .Property(o => o.GrandTotal)
               .HasColumnType("decimal(18,2)");

            // Configure decimal precision for monetary values in OrderItem
            modelBuilder.Entity<OrderItem>()
               .Property(oi => oi.Price)
               .HasColumnType("decimal(18,2)");

            // Relationships Order <-> OrderItem and OrderItem <-> Product
            // should be handled by convention, but could be defined explicitly if needed.
            /* Example:
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            // Product relationship might not need navigation back from Product
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);
            */
            // --- END ADDED configurations ---
        }
    }
}