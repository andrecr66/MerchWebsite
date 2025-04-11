// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Models/DTOs/UpdateCartItemQuantityDto.cs
using System.ComponentModel.DataAnnotations;

namespace MerchWebsite.API.Models.DTOs
{
    public class UpdateCartItemQuantityDto
    {
        // New quantity for the item
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
