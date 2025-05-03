// client/src/app/components/product-detail/product-detail.component.ts
import { Component, OnInit, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router'; // Added RouterLink here
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable, Subscription, switchMap, tap, catchError, of } from 'rxjs';

import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service'; // Ensure AuthService is imported

import { Product } from '../../models/product.model';
import { AddCartItemDto } from '../../models/cart/add-cart-item.dto';
import { Review } from '../../models/review/review.model';
import { CreateReviewDto } from '../../models/review/create-review.dto';
import { OrderDto } from '../../models/order/order.dto'; // Ensure OrderDto is imported if used (needed for submitReview->reloadProductData->next type)
import { FormsModule } from '@angular/forms'; // <<< Ensure FormsModule is imported for ngModel

@Component({
  selector: 'app-product-detail',
  standalone: true,
  // Updated imports
  imports: [CommonModule, RouterLink, ReactiveFormsModule, FormsModule],
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css']
})
export class ProductDetailComponent implements OnInit, OnDestroy {
  // Injected services and utilities
  private route = inject(ActivatedRoute);
  public router = inject(Router); // Public for template access if needed (e.g., returnUrl)
  private productService = inject(ProductService);
  private cartService = inject(CartService);
  public authService = inject(AuthService); // Public for template *ngIf checks
  private fb = inject(FormBuilder);

  // Component State
  product: Product | null = null;
  isLoading = true; // Start in loading state
  error: string | null = null;

  selectedQuantity: number = 1;

  // State for Reviews
  reviews: Review[] = [];
  isLoadingReviews = false;
  reviewsError: string | null = null;
  showReviewForm = false;
  reviewForm: FormGroup;
  isSubmittingReview = false;
  reviewSubmitError: string | null = null;

  // Subscriptions
  private productSubscription: Subscription | null = null;
  private addToCartSubscription: Subscription | null = null;
  private reviewSubscription: Subscription | null = null;
  private reviewSubmitSubscription: Subscription | null = null;

  constructor() {
    // Initialize Review Form
    this.reviewForm = this.fb.group({
      rating: [null, [Validators.required, Validators.min(1), Validators.max(5)]],
      comment: ['', Validators.maxLength(1000)]
    });
  }


  ngOnInit(): void {
    console.log('ProductDetailComponent initialized');
    this.productSubscription = this.route.paramMap.pipe(
      tap(() => {
        // Reset state before fetching new product
        this.isLoading = true;
        this.error = null;
        this.product = null;
        this.reviews = []; // Clear previous reviews
        this.isLoadingReviews = false; // Reset review loading
        this.reviewsError = null;
        this.showReviewForm = false; // Hide review form
        console.log('Route params changed, fetching product...');
      }),
      switchMap(params => {
        const idParam = params.get('id');
        if (!idParam) {
          throw new Error('Product ID is missing.'); // Let catchError handle this
        }
        const productId = Number(idParam);
        if (isNaN(productId)) {
          throw new Error('Invalid Product ID format.'); // Let catchError handle this
        }
        // Fetch product
        return this.productService.getProductById(productId).pipe(
          // Inside ngOnInit -> switchMap -> getProductById().pipe(...)
          tap(productData => {
            if (productData) {
              console.log('Product data loaded, now fetching reviews.');
              this.loadReviews(productData.id); // <<< UNCOMMENT
            }
          }),
          catchError(err => { // Catch errors from getProductById ONLY
            console.error('Error fetching product details:', err);
            // Set the main error property for the component
            this.error = err.message || 'Failed to load product details.';
            // Ensure loading stops even if product fetch fails
            this.isLoading = false;
            return of(null); // Return null to the main subscribe block
          })
        );
      })
    ).subscribe(productData => {
      // --- UPDATED SUBSCRIBE BLOCK ---
      // This runs after product fetch attempt (success or caught error)
      this.isLoading = false; // <<< Stop loading indicator HERE unconditionally

      if (productData) { // Product data was successfully fetched
        this.product = productData;
        this.error = null; // Clear any previous general error
        console.log('Product displayed:', this.product);
        // Optional: Now trigger review loading if needed, AFTER product display logic is set
        // this.loadReviews(this.product.id);
      } else {
        // Product data is null, meaning catchError handled the product fetch error
        // The 'this.error' property should already be set inside catchError
        // If somehow error isn't set, provide a generic one
        if (!this.error) {
          this.error = 'Product could not be loaded.';
        }
        console.log('Product data is null after subscribe, error message:', this.error);
      }
      // --- END UPDATED SUBSCRIBE BLOCK ---
    });
  }

  // --- Method to Load Reviews ---
  loadReviews(productId: number): void {
    this.isLoadingReviews = true;
    this.reviewsError = null;
    this.reviewSubscription?.unsubscribe(); // Unsubscribe previous if any
    this.reviewSubscription = this.productService.getReviewsForProduct(productId).subscribe({
      next: (data) => {
        this.reviews = data;
        this.isLoadingReviews = false;
        console.log('Reviews loaded:', data);
      },
      error: (err) => {
        console.error('Error fetching reviews:', err);
        this.reviewsError = err.message || 'Failed to load reviews.';
        this.isLoadingReviews = false;
      }
    });
  }
  // --- End Load Reviews ---


  // --- Method to Submit Review ---
  submitReview(): void {
    this.reviewSubmitError = null; // Clear previous submit errors
    this.reviewForm.markAllAsTouched(); // Show validation errors

    if (this.reviewForm.invalid || !this.product) {
      this.reviewSubmitError = 'Please select a rating (1-5). Comment is optional.';
      return;
    }

    this.isSubmittingReview = true;

    const reviewData: CreateReviewDto = {
      rating: this.reviewForm.value.rating,
      comment: this.reviewForm.value.comment || null // Send null if empty
    };

    console.log(`Submitting review for product ${this.product.id}`, reviewData);
    this.reviewSubmitSubscription?.unsubscribe();
    this.reviewSubmitSubscription = this.productService.submitReview(this.product.id, reviewData)
      .subscribe({
        next: () => {
          console.log('Review submitted successfully');
          this.isSubmittingReview = false;
          this.showReviewForm = false; // Hide form on success
          this.reviewForm.reset(); // Reset form fields
          alert('Thank you for your review!');
          // Reload product data to show updated average rating/count
          // Or potentially just reload reviews if backend returns updated product
          if (this.product) {
            this.reloadProductData(this.product.id);
            this.loadReviews(this.product.id); // <<< UNCOMMENT
          }
        },
        error: (err) => {
          console.error('Error submitting review:', err);
          this.reviewSubmitError = `Failed to submit review: ${err.message || 'Please try again.'}`;
          this.isSubmittingReview = false;
        }
      });
  }
  // --- End Submit Review ---

  // --- Helper to reload product data ---
  reloadProductData(productId: number): void {
    this.isLoading = true; // Show loading indicator for product section
    this.productService.getProductById(productId).subscribe({
      next: (p) => { this.product = p; this.isLoading = false; },
      error: (e) => { this.error = e.message; this.isLoading = false; }
    });
  }
  // --- End Helper ---

  ngOnDestroy(): void {
    this.productSubscription?.unsubscribe();
    this.addToCartSubscription?.unsubscribe();
    this.reviewSubscription?.unsubscribe(); // <<< Unsubscribe reviews
    this.reviewSubmitSubscription?.unsubscribe(); // <<< Unsubscribe submit
  }

  // Method to add the current product to the cart
  addToCart(): void {
    if (!this.product) {
      console.error('Cannot add to cart, product data is not loaded.');
      return;
    }
    // Basic validation for quantity
    if (this.selectedQuantity < 1 || !Number.isInteger(this.selectedQuantity)) {
      alert('Please enter a valid quantity (at least 1).');
      this.selectedQuantity = 1; // Reset to default
      return;
    }

    console.log(`Adding product ${this.product.id} (${this.product.name}) - Quantity: ${this.selectedQuantity} to cart`);

    const itemToAdd: AddCartItemDto = {
      productId: this.product.id,
      quantity: this.selectedQuantity // <<< Use the selected quantity
    };

    this.addToCartSubscription?.unsubscribe();
    this.addToCartSubscription = this.cartService.addItem(itemToAdd).subscribe({
      next: (updatedCart) => {
        console.log('Product added successfully from detail page, new cart:', updatedCart);
        alert(`${this.selectedQuantity} x ${this.product?.name} added to cart!`); // Updated alert
        // Optional: Reset quantity after adding?
        // this.selectedQuantity = 1;
      },
      error: (err) => {
        console.error(`Error adding product ${this.product?.id} to cart:`, err);
        alert(`Failed to add ${this.product?.name} to cart. ${err.message || ''}`);
      }
    });
  }
  // --- END Modify addToCart Method ---
  get rating() { return this.reviewForm.get('rating'); }
  get comment() { return this.reviewForm.get('comment'); }

  scrollToReviews(event?: MouseEvent): void { // Make event optional
    event?.preventDefault(); // Prevent default if called from an anchor
    const element = document.getElementById('reviews');
    if (element) {
      // Use smooth scrolling
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
      // Optionally focus the first form element if form is shown
      if (this.showReviewForm) {
        setTimeout(() => { // Timeout helps ensure element is visible
          document.querySelector<HTMLElement>('.review-form #rating')?.focus();
        }, 50); // Short delay
      }
    } else {
      console.error("Element with id 'reviews' not found for scrolling.");
    }
  }


}