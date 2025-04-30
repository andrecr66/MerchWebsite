// client/src/app/components/order-history/order-history.component.ts
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderService } from '../../services/order.service';
import { AuthService } from '../../services/auth.service'; // <<< Import AuthService
import { OrderDto } from '../../models/order/order.dto';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-history.component.html',
  styleUrls: ['./order-history.component.css']
})
export class OrderHistoryComponent implements OnInit {

  private orderService = inject(OrderService);
  private authService = inject(AuthService); // <<< Inject AuthService

  orders: OrderDto[] = [];
  isLoading = false; // Start as false, set true only if loading
  error: string | null = null;

  ngOnInit(): void {
    // --- Check Login Status ---
    if (this.authService.isLoggedIn()) { // <<< ADD PARENTHESES ()
      console.log('User is logged in, loading order history.');
      this.loadOrderHistory();
    } else {
      console.log('User is not logged in, skipping order history load.');
      // Optional: Set an error message or redirect to login
      this.error = "You must be logged in to view order history.";
      // Or redirect:
      // const router = inject(Router); // Inject Router if needed
      // router.navigate(['/login']);
    }
    // --- End Check ---
  }

  loadOrderHistory(): void {
    this.isLoading = true; // Set loading true when starting fetch
    this.error = null;
    this.orderService.getOrders().subscribe({
      next: (data: OrderDto[]) => { // Added type
        this.orders = data;
        this.isLoading = false;
        console.log('Order history loaded:', this.orders);
      },
      error: (err: any) => { // Added type
        console.error('Error fetching order history:', err);
        this.error = err?.message || 'Failed to load order history.';
        this.isLoading = false;
      }
    });
  }

  getTotalItems(order: OrderDto): number {
    return order.items.reduce((sum, item) => sum + item.quantity, 0);
  }
}