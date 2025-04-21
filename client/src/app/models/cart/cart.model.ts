// client/src/app/models/cart/cart.model.ts
import { CartItem } from './cart-item.model';

export interface Cart {
  id: number; // Cart ID
  userId: string;
  items: CartItem[];
  grandTotal: number; // This was calculated in the backend DTO
}
