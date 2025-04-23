// server/src/MerchWebsite.API/Entities/Product.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MerchWebsite.API.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)] // Example max length
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } // Keep nullable

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; } // Keep nullable

        // --- ADD Category Property ---
        [Required]
        [MaxLength(50)] // Example max length
        public string Category { get; set; } = "Uncategorized"; // Default value? Or handle null? Required seems better.
        // --- END Category Property ---

        // Add other properties like Sizes, Colors etc. later if needed
    }
}