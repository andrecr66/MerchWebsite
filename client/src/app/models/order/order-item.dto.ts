// client/src/app/models/order/order-item.dto.ts
// Matches backend OrderItemDto
export interface OrderItemDto {
  productId: number;
  productName: string;
  productImageUrl: string | null; // Allow null
  price: number;
  quantity: number;
  totalPrice: number; // Calculated property
}
