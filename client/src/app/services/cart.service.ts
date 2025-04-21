// client/src/app/services/cart.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError, catchError, tap } from 'rxjs';
import { environment } from '../../environments/environment'; // Correct path from services
import { Cart } from '../models/cart/cart.model';
import { AddCartItemDto } from '../models/cart/add-cart-item.dto'; // Ensure this exists

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiUrl = `${environment.apiUrl}/cart`;
  private cartSubject = new BehaviorSubject<Cart | null>(null);

  // --- Ensure cart$ observable is defined ---
  cart$: Observable<Cart | null> = this.cartSubject.asObservable();

  constructor(private http: HttpClient) { }

  // --- Ensure loadCart method is defined ---
  loadCart(): Observable<Cart> {
    return this.http.get<Cart>(this.apiUrl).pipe(
      tap(cart => {
        console.log('Cart loaded:', cart);
        this.cartSubject.next(cart);
      }),
      catchError(this.handleError)
    );
  }

  // --- Ensure clearLocalCart method is defined ---
  clearLocalCart(): void {
    this.cartSubject.next(null);
    console.log('Local cart cleared');
  }

  // --- Ensure addItem method is defined ---
  addItem(itemToAdd: AddCartItemDto): Observable<Cart> {
    console.log('CartService: Adding item', itemToAdd);
    return this.http.post<Cart>(`${this.apiUrl}/items`, itemToAdd).pipe(
      tap(updatedCart => {
        console.log('CartService: Item added, updated cart:', updatedCart);
        this.cartSubject.next(updatedCart);
      }),
      catchError(this.handleError)
    );
  }

  // --- Current value getter ---
  get currentCartValue(): Cart | null {
    return this.cartSubject.value;
  }

  // --- Error Handling ---
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred in CartService!';
    // ... (rest of error handling logic from previous step) ...
    console.error('CartService Error:', error);
    console.error('Backend error details:', error.error);
    return throwError(() => new Error('Something went wrong with the cart operation. Please try again.'));
  }

  /**
   * Updates the quantity of a specific item in the cart via the backend API.
   * Updates the local state with the cart returned from the backend.
   */
  updateItemQuantity(itemId: number, quantity: number): Observable<Cart> {
    console.log(`CartService: Updating item ${itemId} to quantity ${quantity}`);
    // Construct the DTO expected by the backend PUT endpoint
    const updateDto: { quantity: number } = { quantity }; // Matches UpdateCartItemQuantityDto on backend

    return this.http.put<Cart>(`${this.apiUrl}/items/${itemId}`, updateDto).pipe(
      tap(updatedCart => {
        console.log(`CartService: Item ${itemId} quantity updated, new cart state:`, updatedCart);
        this.cartSubject.next(updatedCart); // <<<--- IMPORTANT: Update the BehaviorSubject
      }),
      catchError(this.handleError)
    );
  }

  // Add this method inside CartService class (if not already present)

  /**
   * Removes an item from the cart via the backend API.
   * Updates the local state with the cart returned from the backend.
   */
  removeItem(itemId: number): Observable<Cart> {
    console.log(`CartService: Removing item ${itemId}`);
    return this.http.delete<Cart>(`${this.apiUrl}/items/${itemId}`).pipe(
      tap(updatedCart => {
        console.log(`CartService: Item ${itemId} removed, new cart state:`, updatedCart);
        this.cartSubject.next(updatedCart); // <<<--- IMPORTANT: Update the BehaviorSubject
      }),
      catchError(this.handleError)
    );
  }

  // Add checkout method later if needed

  // Add this method inside CartService class


}
