// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Models/DTOs/CartItemDto.cs
namespace MerchWebsite.API.Models.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; } // CartItem ID
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; // Include details needed for display
        public string? ProductImageUrl { get; set; } // Optional image URL
        public decimal Price { get; set; } // Price at the time? Or current price? Let's use current for now.
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity; // Calculated property
    }
}
