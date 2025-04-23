// client/src/app/models/order/order.dto.ts
// Matches backend OrderDto
import { OrderItemDto } from './order-item.dto'; // Import the item DTO

export interface OrderDto {
  id: number;
  userId: string;
  orderDate: string; // Or Date if you parse it
  shippingAddress_FullName: string;
  shippingAddress_AddressLine1: string;
  shippingAddress_AddressLine2: string | null;
  shippingAddress_City: string;
  shippingAddress_PostalCode: string;
  shippingAddress_Country: string;
  subtotal: number;
  shippingFee: number;
  grandTotal: number;
  // status: string; // Optional status
  items: OrderItemDto[];
}
