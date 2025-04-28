// server/src/MerchWebsite.API/Entities/Product.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MerchWebsite.API.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = "Uncategorized";

        // --- ADD Gender Property ---
        [MaxLength(20)] // Example: "Men", "Women", "Unisex", "Kids"
        public string? Gender { get; set; } // Nullable string for gender
        // --- END Gender Property ---

        public double? AverageRating { get; set; } // Nullable double for rating
    }
}