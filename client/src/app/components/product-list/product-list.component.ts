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
  genderOptions: string[] = ['All', 'Men', 'Women', 'Unisex']; // Example options
  selectedGender: string = 'All'; // Default selection
  minPrice: number | null = null; // Input for min price
  maxPrice: number | null = null;



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

  // --- Modify loadProducts to include new filters ---
  loadProducts(): void {
    this.isLoading = true;
    this.error = null;
    // Read current filter/sort state from component properties
    const categoryToFetch = this.selectedCategory === 'All' ? undefined : this.selectedCategory;
    const sortByToFetch = this.selectedSortBy;
    const genderToFetch = this.selectedGender === 'All' ? undefined : this.selectedGender; // <<< Read gender
    const minPriceToFetch = this.minPrice ?? undefined; // <<< Read min price (pass undefined if null)
    const maxPriceToFetch = this.maxPrice ?? undefined; // <<< Read max price (pass undefined if null)

    console.log(`ProductListComponent: Loading products. Category: ${this.selectedCategory}, SortBy: ${sortByToFetch}, Gender: ${this.selectedGender}, MinPrice: ${minPriceToFetch ?? 'N/A'}, MaxPrice: ${maxPriceToFetch ?? 'N/A'}`);

    this.productService.getProducts(
      categoryToFetch,
      sortByToFetch,
      genderToFetch,
      minPriceToFetch,
      maxPriceToFetch
    ).subscribe({
      // ... next/error handlers ...
      next: (data) => { this.products = data; this.isLoading = false; /*...*/ },
      error: (err) => { this.error = err.message || 'Failed...'; this.isLoading = false; /*...*/ }
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

  filterByGender(gender: string): void {
    if (this.selectedGender !== gender) {
      this.selectedGender = gender;
      this.loadProducts();
    }
  }

  // Inside ProductListComponent class

  applyPriceFilter(): void {
    // --- ADD Validation ---
    this.error = null; // Clear previous errors specific to filtering
    let min = this.minPrice;
    let max = this.maxPrice;

    // Treat empty input as null (no filter)
    if (min === null || min === undefined || String(min).trim() === '') min = null;
    if (max === null || max === undefined || String(max).trim() === '') max = null;

    // Ensure values are numbers if not null
    min = (min !== null) ? Number(min) : null;
    max = (max !== null) ? Number(max) : null;

    // Check for non-numeric input after conversion (isNaN) or negative values
    if ((min !== null && (isNaN(min) || min < 0)) || (max !== null && (isNaN(max) || max < 0))) {
      console.error("Price filter validation: Invalid number or negative value.");
      this.error = "Please enter valid positive numbers for price range.";
      // Optionally reset values:
      // this.minPrice = null;
      // this.maxPrice = null;
      return; // Stop processing
    }


    // Check if min price is greater than max price (only if both are provided)
    if (min !== null && max !== null && min > max) {
      console.error(`Price filter validation: Min price (${min}) cannot be greater than Max price (${max}).`);
      this.error = "Minimum price cannot be greater than maximum price.";
      return; // Stop processing
    }
    // --- END Validation ---

    // Assign potentially cleaned values back (optional, ngModel might handle it)
    this.minPrice = min;
    this.maxPrice = max;

    console.log(`Applying price filter: Min=${this.minPrice ?? 'N/A'}, Max=${this.maxPrice ?? 'N/A'}`);
    this.loadProducts(); // Reload with current price values (which might be null now)
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