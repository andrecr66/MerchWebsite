// client/src/app/services/order.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, catchError } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateOrderDto } from '../models/order/create-order.dto';
import { OrderDto } from '../models/order/order.dto'; // Ensure this is imported

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/orders`;

  placeOrder(orderDetails: CreateOrderDto): Observable<OrderDto> {
    console.log('OrderService: Placing order', orderDetails);
    return this.http.post<OrderDto>(this.apiUrl, orderDetails).pipe(
      catchError(this.handleError)
    );
  }

  // --- ENSURE THIS METHOD EXISTS ---
  getOrders(): Observable<OrderDto[]> {
    console.log('OrderService: Fetching order history');
    return this.http.get<OrderDto[]>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }
  // --- END getOrders METHOD ---

  private handleError(error: HttpErrorResponse): Observable<never> {
    console.error('OrderService Error:', error);
    const message = error.error?.title || error.message || 'An error occurred with the order operation.';
    return throwError(() => new Error(message));
  }
}