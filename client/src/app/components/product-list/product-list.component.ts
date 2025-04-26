// client/src/app/components/product-list/product-list.component.ts
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms'; // Needed for ngModel
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product.model';
import { CartService } from '../../services/cart.service';
import { AddCartItemDto } from '../../models/cart/add-cart-item.dto';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule], // Ensure FormsModule
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit {

  // State Properties
  products: Product[] = [];
  categories: string[] = [];
  genderOptions: string[] = ['All', 'Men', 'Women', 'Unisex'];
  priceRangeOptions = [
    { value: 'all', label: 'All Prices' },
    { value: '0-25', label: '$0 - $25' },
    { value: '25-50', label: '$25 - $50' },
    { value: '50-100', label: '$50 - $100' },
    { value: '100+', label: '$100+' }
  ];
  sortOptions = [
    { value: 'nameAsc', label: 'Name (A-Z)' },
    { value: 'nameDesc', label: 'Name (Z-A)' },
    { value: 'priceAsc', label: 'Price (Low to High)' },
    { value: 'priceDesc', label: 'Price (High to Low)' }
  ];

  // Loading/Error State
  isLoading = true;
  isLoadingCategories = true;
  error: string | null = null;

  // --- Applied Filter/Sort State ---
  // These values are used when calling loadProducts
  selectedCategory: string = 'All';
  selectedGender: string = 'All';
  selectedPriceRange: string = 'all';
  selectedSortBy: string = 'nameAsc';
  // --- End Applied State ---

  // --- Temporary State for Filter Panel ---
  // These are bound to the dropdowns inside the panel
  tempSelectedCategory: string = this.selectedCategory;
  tempSelectedGender: string = this.selectedGender;
  tempSelectedPriceRange: string = this.selectedPriceRange;
  // --- End Temporary State ---

  // --- Filter Panel Visibility ---
  showFilters = false; // Initially hidden
  // --- End Visibility ---

  // Injected Services
  private productService = inject(ProductService);
  private cartService = inject(CartService);

  ngOnInit(): void {
    this.loadProducts(); // Initial load uses default applied filters
    this.loadCategories();
  }

  // --- Data Loading ---
  // This method ALWAYS uses the 'selected...' properties (applied filters)
  loadProducts(): void {
    this.isLoading = true;
    this.error = null;

    const categoryToFetch = this.selectedCategory === 'All' ? undefined : this.selectedCategory;
    const genderToFetch = this.selectedGender === 'All' ? undefined : this.selectedGender;
    const sortByToFetch = this.selectedSortBy;

    // Parse Price Range from the *applied* selection
    let minPriceToFetch: number | undefined = undefined;
    let maxPriceToFetch: number | undefined = undefined;
    if (this.selectedPriceRange && this.selectedPriceRange !== 'all') {
      if (this.selectedPriceRange.includes('+')) {
        minPriceToFetch = parseInt(this.selectedPriceRange.replace('+', ''), 10);
      } else {
        const parts = this.selectedPriceRange.split('-');
        if (parts.length === 2) {
          minPriceToFetch = parseInt(parts[0], 10);
          maxPriceToFetch = parseInt(parts[1], 10);
        }
      }
      if (isNaN(minPriceToFetch ?? NaN)) minPriceToFetch = undefined;
      if (isNaN(maxPriceToFetch ?? NaN)) maxPriceToFetch = undefined;
    }

    console.log(`ProductListComponent: Loading products. Applied Filters -> Category: ${categoryToFetch ?? 'All'}, SortBy: ${sortByToFetch}, Gender: ${genderToFetch ?? 'All'}, MinPrice: ${minPriceToFetch ?? 'N/A'}, MaxPrice: ${maxPriceToFetch ?? 'N/A'}`);

    this.productService.getProducts(
      categoryToFetch, sortByToFetch, genderToFetch, minPriceToFetch, maxPriceToFetch
    ).subscribe({
      next: (data) => { this.products = data; this.isLoading = false; },
      error: (err) => { this.error = err.message || 'Failed...'; this.isLoading = false; }
    });
  }

  loadCategories(): void {
    this.isLoadingCategories = true;
    this.productService.getCategories().subscribe({
      next: (cats) => { this.categories = cats; this.isLoadingCategories = false; },
      error: (err) => { console.error('Error fetching categories:', err); this.isLoadingCategories = false; }
    });
  }

  // --- Event Handlers ---

  toggleFilters(): void {
    // When opening, reset temporary filters to match currently applied ones
    if (!this.showFilters) {
      this.tempSelectedCategory = this.selectedCategory;
      this.tempSelectedGender = this.selectedGender;
      this.tempSelectedPriceRange = this.selectedPriceRange;
    }
    this.showFilters = !this.showFilters; // Toggle visibility
  }

  applyFiltersFromPanel(): void { // <<< Ensure this exact name is used
    console.log('Applying filters from panel...');
    // Copy temporary selections to applied selections
    this.selectedCategory = this.tempSelectedCategory;
    this.selectedGender = this.tempSelectedGender;
    this.selectedPriceRange = this.tempSelectedPriceRange;

    this.loadProducts(); // Reload products with the newly applied filters
    this.showFilters = false; // Hide the panel
  }

  // Optional: Cancel/Close button handler for the panel
  cancelFilters(): void {
    this.showFilters = false; // Just hide the panel, don't apply temp changes
  }

  // Sort changes instantly reload products
  onSortChange(): void {
    console.log(`Sort option changed to: ${this.selectedSortBy}`);
    this.loadProducts();
  }

  // --- Cart ---
  addToCart(product: Product): void {
    console.log(`Adding product ${product.id} (${product.name}) to cart`);
    const itemToAdd: AddCartItemDto = { productId: product.id, quantity: 1 };
    this.cartService.addItem(itemToAdd).subscribe({ /* ... */ });
  }
}