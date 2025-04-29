// client/src/app/components/order-history/order-history.component.ts
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common'; // For *ngIf, *ngFor, pipes
import { OrderService } from '../../services/order.service'; // Import OrderService
import { OrderDto } from '../../models/order/order.dto'; // Import OrderDto
import { Observable } from 'rxjs'; // Import Observable if using async pipe directly
import { RouterLink } from '@angular/router'; // Import RouterLink if adding detail links later

@Component({
  selector: 'app-order-history',
  standalone: true,
  // Import necessary modules
  imports: [CommonModule, RouterLink], // Add RouterLink if needed later
  templateUrl: './order-history.component.html',
  styleUrls: ['./order-history.component.css'] // Plural
})
export class OrderHistoryComponent implements OnInit {

  private orderService = inject(OrderService);

  // Property to hold the list of orders
  orders: OrderDto[] = [];
  isLoading = true; // Loading state
  error: string | null = null; // Error state

  // Alternatively, use an observable directly with async pipe:
  // orders$: Observable<OrderDto[]> | undefined;

  ngOnInit(): void {
    this.loadOrderHistory();
    // If using observable approach:
    // this.orders$ = this.orderService.getOrders().pipe(
    //   tap(() => this.isLoading = false), // Turn off loading on first emission
    //   catchError(err => {
    //     this.error = err.message || 'Failed to load order history.';
    //     this.isLoading = false;
    //     return of([]); // Return empty array on error
    //   })
    // );
  }

  loadOrderHistory(): void {
    this.isLoading = true;
    this.error = null;
    // Inside OrderHistoryComponent class, loadOrderHistory method

    this.orderService.getOrders().subscribe({
      // --- ADD TYPE ANNOTATION FOR data ---
      next: (data: OrderDto[]) => { // <<< Added ': OrderDto[]' type
        // --- END TYPE ANNOTATION ---
        this.orders = data;
        this.isLoading = false;
        console.log('Order history loaded:', this.orders);
      },
      error: (err: any) => { // Keep type for err
        // ... error handling ...
      }
    });
  }

  // Optional: Helper to get total items in an order
  getTotalItems(order: OrderDto): number {
    return order.items.reduce((sum, item) => sum + item.quantity, 0);
  }
}