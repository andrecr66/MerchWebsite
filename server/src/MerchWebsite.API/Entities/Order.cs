// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Entities/Order.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // For [Column]

namespace MerchWebsite.API.Entities
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Link to the user placing the order
        public User User { get; set; } = null!; // Navigation property back to User

        public DateTime OrderDate { get; set; } = DateTime.UtcNow; // Record when the order was placed

        // Shipping Address - Store directly or link to an Address entity?
        // For simplicity now, let's store directly. Could be refactored later.
        [Required]
        public string ShippingAddress_FullName { get; set; } = string.Empty;
        [Required]
        public string ShippingAddress_AddressLine1 { get; set; } = string.Empty;
        public string? ShippingAddress_AddressLine2 { get; set; }
        [Required]
        public string ShippingAddress_City { get; set; } = string.Empty;
        [Required]
        public string ShippingAddress_PostalCode { get; set; } = string.Empty;
        [Required]
        public string ShippingAddress_Country { get; set; } = string.Empty;

        // Consider adding OrderStatus (e.g., Pending, Processing, Shipped, Delivered, Cancelled)
        // public OrderStatus Status { get; set; } = OrderStatus.Pending; // Requires an OrderStatus enum

        // Store calculated total at time of order? Recommended to avoid issues if product prices change later.
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; } // Sum of OrderItem prices * quantities
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; } // Example: could be calculated or fixed
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal { get; set; } // Subtotal + ShippingFee

        // Collection of items in this order
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        // Payment Intent ID (if using Stripe, etc.) could be added later
        // public string? PaymentIntentId { get; set; }
    }
}
// Optional: Define OrderStatus enum if using Status property
// public enum OrderStatus { Pending, PaymentReceived, PaymentFailed, Processing, Shipped, Delivered, Cancelled }
