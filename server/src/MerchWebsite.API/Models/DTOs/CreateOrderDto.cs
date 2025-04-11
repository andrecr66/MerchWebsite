// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Models/DTOs/CreateOrderDto.cs
using System.ComponentModel.DataAnnotations;

namespace MerchWebsite.API.Models.DTOs
{
    // DTO for the request body when creating an order
    public class CreateOrderDto
    {
        // We might not need anything specific here if we fetch cart from DB
        // but we DO need shipping address.

        // Could add a flag: public bool SaveAddress { get; set; }

        [Required]
        public string ShippingAddress_FullName { get; set; } = string.Empty;
        [Required]
        public string ShippingAddress_AddressLine1 { get; set; } = string.Empty;
        public string? ShippingAddress_AddressLine2 { get; set; }
        [Required]
        public string ShippingAddress_City { get; set; } = string.Empty;
        [Required]
        public string ShippingAddress_PostalCode { get; set; } = string.Empty;
        [Required]
        public string ShippingAddress_Country { get; set; } = string.Empty;
    }
}
