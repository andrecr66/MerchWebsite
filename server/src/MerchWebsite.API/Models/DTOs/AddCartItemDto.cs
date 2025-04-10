// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Models/DTOs/AddCartItemDto.cs
using System.ComponentModel.DataAnnotations; // For validation attributes

namespace MerchWebsite.API.Models.DTOs
{
    public class AddCartItemDto
    {
        [Required]
        public int ProductId { get; set; }

        // Quantity defaults to 1 if not provided, but must be at least 1
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;
    }
}
