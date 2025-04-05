using Microsoft.AspNetCore.Identity;

namespace MerchWebsite.API.Entities
{
    // Inherit from IdentityUser to get standard identity properties (Id, UserName, Email, PasswordHash, etc.)
    public class User : IdentityUser
    {
        // Add any custom properties for your user here later, for example:
        // public string? DisplayName { get; set; }
        // public string? Address { get; set; }
    }
}
