// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Models/DTOs/OrderDto.cs
using System; // For DateTimeOffset
using System.Collections.Generic; // For List

namespace MerchWebsite.API.Models.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // Consider if you want to expose this
        public DateTimeOffset OrderDate { get; set; }

        // Shipping Address
        public string ShippingAddress_FullName { get; set; } = string.Empty;
        public string ShippingAddress_AddressLine1 { get; set; } = string.Empty;
        public string? ShippingAddress_AddressLine2 { get; set; }
        public string ShippingAddress_City { get; set; } = string.Empty;
        public string ShippingAddress_PostalCode { get; set; } = string.Empty;
        public string ShippingAddress_Country { get; set; } = string.Empty;

        // Totals
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }

        // Optional: Order Status
        // public string Status { get; set; } = string.Empty; // String representation of status

        // Items
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
}
