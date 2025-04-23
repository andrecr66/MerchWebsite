// client/src/app/services/order.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, catchError } from 'rxjs';
import { environment } from '../../environments/environment';
// --- Ensure these paths are correct relative to 'services' ---
import { CreateOrderDto } from '../models/order/create-order.dto';
import { OrderDto } from '../models/order/order.dto';
// --- End path check ---

@Injectable({
  providedIn: 'root'
})
export class OrderService { // <<< Ensure 'export' is present
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/orders`; // API endpoint for orders

  // Method to place an order
  placeOrder(orderDetails: CreateOrderDto): Observable<OrderDto> {
    console.log('OrderService: Placing order', orderDetails);
    return this.http.post<OrderDto>(this.apiUrl, orderDetails).pipe(
      catchError(this.handleError)
    );
  }

  // Basic error handler
  private handleError(error: HttpErrorResponse): Observable<never> {
    console.error('OrderService Error:', error);
    const message = error.error?.title || error.message || 'Could not place order.';
    return throwError(() => new Error(message));
  }

  // Add methods for fetching order history later if needed
  // getOrders(): Observable<OrderDto[]> { ... }
  // getOrder(id: number): Observable<OrderDto> { ... }
}
