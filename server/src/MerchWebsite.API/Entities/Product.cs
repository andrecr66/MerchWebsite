using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MerchWebsite.API.Entities
{
    public class Product
    {
        [Key] // Specifies the primary key
        public int Id { get; set; }

        [Required] // Makes the property required in the database
        [MaxLength(100)] // Sets a maximum length
        public required string Name { get; set; } // 'required' keyword for non-nullable reference types

        [MaxLength(500)]
        public string? Description { get; set; } // Nullable string for optional description

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Specifies the database column type for precision
        public decimal Price { get; set; }

        [MaxLength(2048)] // Max length for a URL
        public string? ImageUrl { get; set; } // Optional image URL

        // Potential future properties:
        // public string Sku { get; set; }
        // public int StockQuantity { get; set; }
        // public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        // public int CategoryId { get; set; } // Foreign key
        // public Category Category { get; set; } // Navigation property
    }
}
