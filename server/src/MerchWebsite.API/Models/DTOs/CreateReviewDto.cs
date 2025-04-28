// server/src/MerchWebsite.API/Models/DTOs/CreateReviewDto.cs
using System.ComponentModel.DataAnnotations;

namespace MerchWebsite.API.Models.DTOs
{
    public class CreateReviewDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }
    }
}