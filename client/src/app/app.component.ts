// client/src/app/app.component.ts
import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
// --- Make sure CommonModule is imported ---
import { CommonModule } from '@angular/common';
// --- End Import ---
import { AuthService } from './services/auth.service';
import { CartService } from './services/cart.service';
import { Observable, map } from 'rxjs';
import { Cart } from './models/cart/cart.model'; // Import Cart if needed for typing

@Component({
  selector: 'app-root',
  standalone: true,
  // --- Ensure CommonModule is in imports ---
  imports: [CommonModule, RouterOutlet, RouterLink],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'MerchWebsiteClient';

  // Make services public for template access
  public authService = inject(AuthService);
  public cartService = inject(CartService);

  // --- Ensure these properties are declared correctly ---
  // isLoggedIn$ is removed as we'll call the method directly in the template
  cartItemCount$: Observable<number>;
  // --- End Property Declarations ---

  constructor() {
    // isLoggedIn$ initialization removed

    // Ensure map and reduce logic is correct
    this.cartItemCount$ = this.cartService.cart$.pipe(
      map(cart => {
        if (!cart || !cart.items) {
          return 0;
        }
        return cart.items.reduce((count, item) => count + item.quantity, 0);
      })
    );
  }

  ngOnInit(): void {
    console.log('AppComponent initialized.');
  }

  // --- Ensure logout method is declared correctly ---
  logout(): void {
    this.authService.logout();
  }
  // --- End logout method ---
}
