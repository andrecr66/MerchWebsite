using System.ComponentModel.DataAnnotations;

namespace MerchWebsite.API.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [RegularExpression("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{4,8}$", ErrorMessage = "Password must be complex")] // Example complexity rule
        public required string Password { get; set; }

        [Required]
        public required string Username { get; set; }

        // Add other registration fields if needed, e.g., DisplayName
    }
}
