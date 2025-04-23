// client/src/app/components/product-list/product-list.component.ts
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms'; // <<< Import FormsModule for ngModel
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product.model';
import { CartService } from '../../services/cart.service';
import { AddCartItemDto } from '../../models/cart/add-cart-item.dto';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule], // <<< Add FormsModule here
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css'] // Plural
})
export class ProductListComponent implements OnInit {

  // State Properties
  products: Product[] = [];
  categories: string[] = [];
  isLoading = true; // For products
  isLoadingCategories = true; // For categories
  error: string | null = null; // For product loading errors

  // Filter/Sort State
  selectedCategory: string = 'All'; // Default category filter
  selectedSortBy: string = 'nameAsc'; // Default sort - matches backend default

  // Sort Options for Dropdown
  sortOptions = [
    { value: 'nameAsc', label: 'Name (A-Z)' },
    { value: 'nameDesc', label: 'Name (Z-A)' },
    { value: 'priceAsc', label: 'Price (Low to High)' },
    { value: 'priceDesc', label: 'Price (High to Low)' }
  ];

  // Injected Services
  private productService = inject(ProductService);
  private cartService = inject(CartService);

  ngOnInit(): void {
    // Load initial data when component loads
    this.loadProducts(); // Uses default category ('All') and sort ('nameAsc')
    this.loadCategories();
  }

  // --- Data Loading Methods ---

  loadProducts(): void {
    this.isLoading = true;
    this.error = null;
    const categoryToFetch = this.selectedCategory === 'All' ? undefined : this.selectedCategory;
    const sortByToFetch = this.selectedSortBy; // Get current selection

    // --- ADD LOG ---
    console.log(`[loadProducts] Called. Category: ${categoryToFetch ?? 'All'}, SortBy: ${sortByToFetch}`);
    // --- END LOG ---

    this.productService.getProducts(categoryToFetch, sortByToFetch).subscribe({
      next: (data) => {
        this.products = data;
        this.isLoading = false;
        console.log(`Products loaded:`, data);
      },
      error: (err) => {
        console.error('Error fetching products:', err);
        this.error = err.message || 'Failed to load products.';
        this.isLoading = false;
      }
    });
  }

  loadCategories(): void {
    this.isLoadingCategories = true;
    this.productService.getCategories().subscribe({
      next: (cats) => {
        this.categories = cats; // Assumes getCategories returns ['All', ...]
        this.isLoadingCategories = false;
        console.log('Categories loaded:', cats);
      },
      error: (err) => {
        console.error('Error fetching categories:', err);
        // Handle error - maybe show a message that filtering is unavailable
        this.isLoadingCategories = false;
      }
    });
  }

  // --- Event Handlers ---

  filterByCategory(category: string): void {
    // Update selection only if it changed, then reload products
    if (this.selectedCategory !== category) {
      this.selectedCategory = category;
      this.loadProducts();
    }
  }

  onSortChange(): void {
    // The value in selectedSortBy should already be updated by [(ngModel)]
    console.log(`[onSortChange] Sort option selected (ngModel value): ${this.selectedSortBy}`);
    this.loadProducts(); // Explicitly reload products
  }
  // Note: Removed the onSortChange($event) method as ngModel handles the update.
  // If you prefer using (change) instead of [(ngModel)], reinstate the onSortChange($event) method.


  addToCart(product: Product): void {
    console.log(`Adding product ${product.id} (${product.name}) to cart`);
    const itemToAdd: AddCartItemDto = { productId: product.id, quantity: 1 };
    this.cartService.addItem(itemToAdd).subscribe({
      next: (updatedCart) => {
        console.log('Product added successfully');
        // Optionally provide better user feedback (e.g., toast message)
      },
      error: (err) => {
        console.error(`Error adding ${product.name} to cart:`, err);
        alert(`Failed to add ${product.name} to cart. ${err.message || ''}`);
      }
    });
  }
}