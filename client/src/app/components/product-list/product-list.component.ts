// client/src/app/components/product-list/product-list.component.ts
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router'; // Keep RouterLink for product detail links
import { FormsModule } from '@angular/forms';
import { NgxSliderModule, Options, LabelType } from '@angular-slider/ngx-slider';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product.model';
import { CartService } from '../../services/cart.service';
import { AddCartItemDto } from '../../models/cart/add-cart-item.dto';

// Interface for Sort Option Type Safety
interface SortOption {
  value: string;
  label: string;
}

@Component({
  selector: 'app-product-list',
  standalone: true,
  // Updated imports
  imports: [CommonModule, RouterLink, FormsModule, NgxSliderModule],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit {

  // State Properties
  products: Product[] = [];
  categories: string[] = [];
  genderOptions: string[] = ['All', 'Men', 'Women', 'Unisex'];
  sortOptions: SortOption[] = [
    { value: 'nameAsc', label: 'Relevance' }, // Map relevance to nameAsc (backend default)
    { value: 'priceAsc', label: 'Price (Low to High)' },
    { value: 'priceDesc', label: 'Price (High to Low)' },
    { value: 'ratingDesc', label: 'Average Rating' }, // Add rating sort
    { value: 'nameAsc', label: 'Name (A-Z)' }, // Keep explicit name sort
    { value: 'nameDesc', label: 'Name (Z-A)' }
  ];

  // Loading/Error State
  isLoading = true; // Initialize as true for initial load
  isLoadingCategories = true;
  error: string | null = null; // Separate error state

  // Applied Filter/Sort State
  selectedCategory: string = 'All';
  selectedGender: string = 'All';
  selectedSortBy: string = 'nameAsc';
  readonly PRICE_MIN_BOUND = 0;
  readonly PRICE_MAX_BOUND = 150; // Adjust as needed
  currentMinPrice: number = this.PRICE_MIN_BOUND;
  currentMaxPrice: number = this.PRICE_MAX_BOUND;

  // Temporary State for Filter Panel
  tempSelectedCategory: string = this.selectedCategory;
  tempSelectedGender: string = this.selectedGender;
  tempMinPrice: number = this.currentMinPrice;
  tempMaxPrice: number = this.currentMaxPrice;

  // Price Slider Configuration
  priceSliderOptions: Options = {
    floor: this.PRICE_MIN_BOUND,
    ceil: this.PRICE_MAX_BOUND,
    step: 1,
    showTicks: false,
    translate: (value: number, label: LabelType): string => {
      switch (label) {
        case LabelType.Low: return "$" + value;
        case LabelType.High: return "$" + value;
        default: return "$" + value;
      }
    }
  };

  // Filter Panel Visibility
  showFilters = false;

  // Injected Services
  private productService = inject(ProductService);
  private cartService = inject(CartService);

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  // --- Data Loading ---
  loadProducts(): void {
    this.isLoading = true; // Set loading true INITIALLY
    this.error = null;     // Clear previous errors INITIALLY

    const categoryToFetch = this.selectedCategory === 'All' ? undefined : this.selectedCategory;
    const genderToFetch = this.selectedGender === 'All' ? undefined : this.selectedGender;
    const sortByToFetch = this.selectedSortBy;
    const minPriceToFetch = (this.currentMinPrice > this.PRICE_MIN_BOUND) ? this.currentMinPrice : undefined;
    const maxPriceToFetch = (this.currentMaxPrice < this.PRICE_MAX_BOUND) ? this.currentMaxPrice : undefined;

    console.log(`ProductListComponent: Loading products. Applied Filters -> Category: ${categoryToFetch ?? 'All'}, SortBy: ${sortByToFetch}, Gender: ${genderToFetch ?? 'All'}, MinPrice: ${minPriceToFetch ?? 'N/A'}, MaxPrice: ${maxPriceToFetch ?? 'N/A'}`);

    this.productService.getProducts(
      categoryToFetch, sortByToFetch, genderToFetch, minPriceToFetch, maxPriceToFetch
    ).subscribe({
      next: (data) => {
        this.products = data;
        this.isLoading = false; // <<< Set loading false on SUCCESS
        this.error = null;     // <<< Ensure error is null on SUCCESS
        console.log(`Products loaded:`, data);
      },
      error: (err) => {
        console.error('Error fetching products:', err);
        this.error = err.message || 'Failed to load products.'; // <<< Set error message on FAILURE
        this.isLoading = false; // <<< Set loading false on FAILURE
        this.products = [];    // <<< Clear products on error
      }
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
    if (!this.showFilters) {
      this.tempSelectedCategory = this.selectedCategory;
      this.tempSelectedGender = this.selectedGender;
      this.tempMinPrice = this.currentMinPrice;
      this.tempMaxPrice = this.currentMaxPrice;
    }
    this.showFilters = !this.showFilters;
  }

  applyAllFilters(): void {
    console.log('Applying all selected filters...');
    this.selectedCategory = this.tempSelectedCategory;
    this.selectedGender = this.tempSelectedGender;
    this.currentMinPrice = this.tempMinPrice;
    this.currentMaxPrice = this.tempMaxPrice;
    this.loadProducts();
    this.showFilters = false;
  }

  cancelFilters(): void {
    // Reset temp slider values back to applied state before closing
    this.tempMinPrice = this.currentMinPrice;
    this.tempMaxPrice = this.currentMaxPrice;
    this.showFilters = false;
  }

  onSortChange(): void {
    console.log(`Sort option changed to: ${this.selectedSortBy}`);
    this.loadProducts();
  }

  // --- Cart ---
  addToCart(product: Product): void {
    console.log(`Adding product ${product.id} (${product.name}) to cart`);
    const itemToAdd: AddCartItemDto = { productId: product.id, quantity: 1 };
    this.cartService.addItem(itemToAdd).subscribe({
      next: (updatedCart) => { console.log('Product added successfully'); },
      error: (err) => { alert(`Failed to add ${product.name} to cart. ${err.message || ''}`); }
    });
  }
}