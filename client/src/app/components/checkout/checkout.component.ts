// client/src/app/components/checkout/checkout.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { Observable } from 'rxjs';
import { Cart } from '../../models/cart/cart.model';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
// --- Ensure OrderDto is imported ---
import { OrderDto } from '../../models/order/order.dto'; // <<< VERIFY/ADD THIS IMPORT
// --- End Import ---

@Component({
    selector: 'app-checkout',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './checkout.component.html',
    styleUrls: ['./checkout.component.css'] // Ensure checkout.component.css exists or remove this line
})
export class CheckoutComponent implements OnInit {
    cartService = inject(CartService);
    orderService = inject(OrderService);
    router = inject(Router);
    fb = inject(FormBuilder);

    cart$: Observable<Cart | null> = this.cartService.cart$;
    checkoutForm: FormGroup;
    isSubmitting = false;
    errorMessage: string | null = null;

    constructor() {
        // Initialize the form group
        this.checkoutForm = this.fb.group({
            fullName: ['', Validators.required],
            addressLine1: ['', Validators.required],
            addressLine2: [''],
            city: ['', Validators.required],
            // --- MODIFY THIS LINE ---
            postalCode: ['', Validators.required],
            country: ['', Validators.required]
        });
    }

    ngOnInit(): void {
        const currentCart = this.cartService.currentCartValue;
        if (!currentCart || !currentCart.items || currentCart.items.length === 0) {
            console.log('Checkout loaded with empty cart, redirecting to products.');
            this.router.navigate(['/products']);
        }
    }

    placeOrder(): void {
        this.errorMessage = null;
        this.checkoutForm.markAllAsTouched();

        if (this.checkoutForm.invalid) {
            console.log('Checkout form is invalid');
            this.errorMessage = 'Please fill in all required shipping details correctly.';
            return;
        }

        this.isSubmitting = true;

        const orderDetails = {
            shippingAddress_FullName: this.checkoutForm.value.fullName,
            shippingAddress_AddressLine1: this.checkoutForm.value.addressLine1,
            shippingAddress_AddressLine2: this.checkoutForm.value.addressLine2 || null,
            shippingAddress_City: this.checkoutForm.value.city,
            shippingAddress_PostalCode: this.checkoutForm.value.postalCode,
            shippingAddress_Country: this.checkoutForm.value.country,
        };

        console.log('Placing order with details:', orderDetails);

        // Type annotations for subscribe callbacks
        this.orderService.placeOrder(orderDetails).subscribe({
            next: (createdOrder: OrderDto) => { // Type annotation here
                this.isSubmitting = false;
                console.log('Order placed successfully:', createdOrder);
                alert('Order placed successfully!');
                this.cartService.clearLocalCart();
                this.router.navigate(['/']);
            },
            error: (err: any) => { // Type annotation here (any or HttpErrorResponse)
                this.isSubmitting = false;
                console.error('Error placing order:', err);
                this.errorMessage = `Failed to place order: ${err?.message || 'Unknown error'}`;
            }
        });
    }

    // Helper getters
    get fullName() { return this.checkoutForm.get('fullName'); }
    get addressLine1() { return this.checkoutForm.get('addressLine1'); }
    get city() { return this.checkoutForm.get('city'); }
    get postalCode() { return this.checkoutForm.get('postalCode'); }
    get country() { return this.checkoutForm.get('country'); }
}