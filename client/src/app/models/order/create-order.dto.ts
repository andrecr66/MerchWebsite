// client/src/app/models/order/create-order.dto.ts
// Matches backend CreateOrderDto
export interface CreateOrderDto {
  shippingAddress_FullName: string;
  shippingAddress_AddressLine1: string;
  shippingAddress_AddressLine2: string | null; // Match null possibility
  shippingAddress_City: string;
  shippingAddress_PostalCode: string;
  shippingAddress_Country: string;
  // Add SaveAddress flag later if needed
}
