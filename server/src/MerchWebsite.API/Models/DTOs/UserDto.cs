namespace MerchWebsite.API.Models.DTOs
{
    // DTO to return basic user info and token
    public class UserDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Token { get; set; }
        // Add other user info if needed (e.g., DisplayName)
    }
}
