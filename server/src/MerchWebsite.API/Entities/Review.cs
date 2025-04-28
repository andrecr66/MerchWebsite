// server/src/MerchWebsite.API/Entities/Review.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MerchWebsite.API.Entities
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5)] // Rating must be between 1 and 5
        public int Rating { get; set; }

        [MaxLength(1000)] // Max length for comment
        public string? Comment { get; set; } // Comment is optional

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        // Foreign Key to Product
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Foreign Key to User (ASP.NET Core Identity User)
        [Required]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!; // Navigation to User
    }
}