// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Models/DTOs/CartDto.cs
namespace MerchWebsite.API.Models.DTOs
{
    public class CartDto
    {
        public int Id { get; set; } // Cart ID
        public string UserId { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal GrandTotal => Items.Sum(item => item.TotalPrice); // Calculated total
    }
}
