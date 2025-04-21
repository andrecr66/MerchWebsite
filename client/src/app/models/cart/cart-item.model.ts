// client/src/app/models/cart/cart-item.model.ts
export interface CartItem {
  id: number; // CartItem ID
  productId: number;
  productName: string;
  productImageUrl?: string; // Optional
  price: number;
  quantity: number;
  totalPrice: number; // This was calculated in the backend DTO
}
