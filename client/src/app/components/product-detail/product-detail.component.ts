// client/src/app/components/product-detail/product-detail.component.ts
import { Component, OnInit, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common'; // For *ngIf, async pipe, currency pipe etc.
import { ActivatedRoute, Router } from '@angular/router'; // Import ActivatedRoute and Router
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service'; // Import CartService
import { Product } from '../../models/product.model';
import { AddCartItemDto } from '../../models/cart/add-cart-item.dto'; // Import DTO
import { Observable, Subscription, switchMap, tap, catchError, of } from 'rxjs'; // Import necessary RxJS operators

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule], // Import CommonModule
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css'] // Plural
})
export class ProductDetailComponent implements OnInit, OnDestroy {
  // Inject services and router utilities
  private route = inject(ActivatedRoute);
  private router = inject(Router); // Inject Router for navigation on error
  private productService = inject(ProductService);
  private cartService = inject(CartService); // Inject CartService

  product: Product | null = null; // Property to hold the fetched product
  isLoading = true; // Loading flag
  error: string | null = null; // Error flag/message

  // Optional: Subscription management
  private productSubscription: Subscription | null = null;
  private addToCartSubscription: Subscription | null = null;

  ngOnInit(): void {
    console.log('ProductDetailComponent initialized');
    // Use RxJS to get the 'id' parameter from the route, then fetch the product
    this.productSubscription = this.route.paramMap.pipe(
      tap(() => { // Reset state before fetching
        this.isLoading = true;
        this.error = null;
        this.product = null;
        console.log('Route params changed, fetching product...');
      }),
      switchMap(params => {
        const idParam = params.get('id'); // Get the 'id' parameter as string
        if (!idParam) {
          console.error('Product ID parameter missing from route');
          // Throw an error that catchError can handle
          throw new Error('Product ID is missing.');
        }
        const productId = Number(idParam); // Convert string ID to number
        if (isNaN(productId)) {
          console.error('Invalid Product ID parameter:', idParam);
          throw new Error('Invalid Product ID format.');
        }
        // Call the service to get the product
        return this.productService.getProductById(productId).pipe(
          catchError(err => {
            console.error('Error fetching product details:', err);
            this.error = err.message || 'Failed to load product details.';
            // Navigate away if product not found (optional)
            // if (err.message.includes('Not found')) { this.router.navigate(['/products']); }
            this.isLoading = false;
            return of(null); // Return null observable to stop the main pipe on error
          })
        );
      })
    ).subscribe(productData => {
      this.isLoading = false;
      if (productData) { // Check if productData is not null (error handled in catchError)
        this.product = productData;
        console.log('Product loaded:', this.product);
      } else {
        // Error was handled in catchError, maybe navigate or just show error message
        if (!this.error) { // If catchError didn't set a specific message
          this.error = 'Product could not be loaded.';
        }
      }
    });
  }

  ngOnDestroy(): void {
    // Unsubscribe to prevent memory leaks when component is destroyed
    this.productSubscription?.unsubscribe();
    this.addToCartSubscription?.unsubscribe();
    console.log('ProductDetailComponent destroyed, subscriptions cleaned up.');
  }

  // Method to add the current product to the cart
  addToCart(): void {
    if (!this.product) {
      console.error('Cannot add to cart, product data is not loaded.');
      return;
    }
    console.log(`Adding product ${this.product.id} (${this.product.name}) to cart from detail page`);
    const itemToAdd: AddCartItemDto = {
      productId: this.product.id,
      quantity: 1 // Default to 1 from detail page, could add quantity selector later
    };

    // Unsubscribe from previous add attempt if any
    this.addToCartSubscription?.unsubscribe();

    this.addToCartSubscription = this.cartService.addItem(itemToAdd).subscribe({
      next: (updatedCart) => {
        console.log('Product added successfully from detail page, new cart:', updatedCart);
        alert(`${this.product?.name} added to cart!`); // Simple feedback
      },
      error: (err) => {
        console.error(`Error adding product ${this.product?.id} to cart:`, err);
        alert(`Failed to add ${this.product?.name} to cart. ${err.message || ''}`);
      }
    });
  }
}