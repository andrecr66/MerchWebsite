// server/src/MerchWebsite.API/Models/DTOs/ReviewDto.cs
using System;

namespace MerchWebsite.API.Models.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; } // Review ID
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        // Include user info - prevent sending full user ID
        public string UserName { get; set; } = "Anonymous"; // Default if user somehow not found
    }
}