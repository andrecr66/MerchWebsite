// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Entities/Cart.cs
using System.ComponentModel.DataAnnotations;

namespace MerchWebsite.API.Entities
{
    public class Cart
    {
        public int Id { get; set; }

        // Link to the user who owns this cart
        // Using string for UserId consistent with ASP.NET Core Identity default
        [Required]
        public string UserId { get; set; } = string.Empty;
        // Optional: Navigation property back to the User if needed, but often just UserId is sufficient
        // public User User { get; set; }

        // Collection of items in this cart
        // Initialize to prevent null reference exceptions
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
