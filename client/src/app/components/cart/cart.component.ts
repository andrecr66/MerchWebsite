// client/src/app/components/cart/cart.component.ts
import { Component, OnInit, inject } from '@angular/core'; // Use inject or constructor, be consistent
import { CommonModule } from '@angular/common';
import { CartService } from '../../services/cart.service';
import { Cart } from '../../models/cart/cart.model';
import { Observable } from 'rxjs';
import { RouterLink } from '@angular/router'; // Import RouterLink

@Component({
    selector: 'app-cart',
    standalone: true,
    // Make sure RouterLink is imported if used in template
    imports: [CommonModule, RouterLink],
    templateUrl: './cart.component.html',
    styleUrls: ['./cart.component.css'] // Plural
})
export class CartComponent implements OnInit {
    cart$: Observable<Cart | null>; // Removed | undefined, initialized in constructor/ngOnInit

    // Using constructor injection as shown in your code:
    constructor(private cartService: CartService) {
        // Initialize cart$ directly from the service
        this.cart$ = this.cartService.cart$;
    }

    ngOnInit(): void {
        // No need to re-assign here if done in constructor
        // Trigger loading cart data (AuthService might already do this)
        // Consider if this load is necessary or might cause race conditions with AuthService load
        // For now, we keep it to ensure data is loaded if navigated directly
        this.cartService.loadCart().subscribe({
            error: (err) => console.error('Error loading cart in CartComponent ngOnInit:', err)
        });
    }

    // --- New method to handle the change event ---
    onQuantityChange(event: Event, itemId: number): void {
        const inputElement = event.target as HTMLInputElement;
        const quantity = parseInt(inputElement.value, 10); // Parse value to integer

        if (!isNaN(quantity) && quantity >= 0) { // Allow 0 for potential removal via update
            this.updateCartItemQuantity(itemId, quantity);
        } else {
            console.warn('Invalid quantity entered:', inputElement.value);
            // TODO: Optionally reset input value to previous known value
        }
    }

    // In cart.component.ts

    // Method that actually calls the service
    private updateCartItemQuantity(itemId: number, quantity: number): void {
        console.log(`CartComponent: Updating item ${itemId} quantity to ${quantity}`);
        if (quantity <= 0) {
            this.removeItem(itemId);
        } else {
            // --- UNCOMMENT THE SERVICE CALL ---
            this.cartService.updateItemQuantity(itemId, quantity).subscribe({
                next: () => console.log(`Item ${itemId} quantity updated successfully in backend`),
                error: (err) => console.error(`Error updating quantity for item ${itemId}:`, err)
            });
            // --- End Uncomment ---
        }
    }

    removeItem(itemId: number): void {
        console.log(`CartComponent: Removing item ${itemId}`);
        // --- UNCOMMENT THE SERVICE CALL ---
        this.cartService.removeItem(itemId).subscribe({
            next: () => console.log(`Item ${itemId} removed successfully from backend`),
            error: (err) => console.error(`Error removing item ${itemId}:`, err)
        });
        // --- End Uncomment ---
    }

    checkout(): void {
        console.log('Proceed to checkout');
        // Inject Router and navigate if needed
    }
}