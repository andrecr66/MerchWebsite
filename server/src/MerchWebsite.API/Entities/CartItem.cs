// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Entities/CartItem.cs
using System.ComponentModel.DataAnnotations.Schema; // Needed for [ForeignKey]

namespace MerchWebsite.API.Entities
{
    public class CartItem
    {
        public int Id { get; set; }

        // Foreign Key to the Cart
        public int CartId { get; set; }
        // Navigation property back to the Cart
        public Cart Cart { get; set; } = null!; // null! indicates it's required & expected to be loaded

        // Foreign Key to the Product
        public int ProductId { get; set; }
        // Navigation property to the Product
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        // Optional: Store calculated price at time of adding,
        // or rely on joining with Product table for current price.
        // For simplicity now, we'll rely on joining.
    }
}
