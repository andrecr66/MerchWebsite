// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Entities/OrderItem.cs
using System.ComponentModel.DataAnnotations.Schema; // For [Column]

namespace MerchWebsite.API.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        // Foreign Key to the Order header
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        // Foreign Key to the Product
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!; // Navigation property optional if storing product details here

        // Store product details AT THE TIME OF ORDER to prevent issues if product is deleted or details change
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Price per unit when ordered
        public int Quantity { get; set; }
        public string ProductName { get; set; } = string.Empty; // Name when ordered
        public string? ProductImageUrl { get; set; } // ImageUrl when ordered

        // Calculated total for this line item
        [NotMapped] // Typically calculated, not stored, unless needed for historical reporting
        public decimal TotalPrice => Price * Quantity;
    }
}
