// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Models/DTOs/OrderItemDto.cs
namespace MerchWebsite.API.Models.DTOs
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; // Name when ordered
        public string? ProductImageUrl { get; set; } // ImageUrl when ordered
        public decimal Price { get; set; } // Price per unit when ordered
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }
}
