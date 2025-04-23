// client/src/app/components/product-list/product-list.component.ts
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product.model'; // Ensure this model includes 'category'
import { CartService } from '../../services/cart.service';
import { AddCartItemDto } from '../../models/cart/add-cart-item.dto';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit {

  products: Product[] = [];
  isLoading = true; // Keep this for product loading
  error: string | null = null;

  // --- VERIFY/ADD Category properties ---
  categories: string[] = [];
  selectedCategory: string = 'All'; // Default selection
  isLoadingCategories = true; // Separate loading flag for categories
  // --- End Category properties ---

  private productService = inject(ProductService);
  private cartService = inject(CartService);

  ngOnInit(): void {
    this.loadProducts(); // Load initial (all) products
    this.loadCategories(); // Load categories
  }

  // --- VERIFY/ADD Method to load products ---
  loadProducts(category?: string): void {
    this.isLoading = true;
    this.error = null;
    // If a category is passed, update selection, otherwise keep current/default
    this.selectedCategory = category !== undefined ? category : this.selectedCategory;
    console.log(`ProductListComponent: Loading products for category: ${this.selectedCategory}`);

    this.productService.getProducts(this.selectedCategory === 'All' ? undefined : this.selectedCategory).subscribe({
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
  // --- End load products method ---


  // --- VERIFY/ADD Method to load categories ---
  loadCategories(): void {
    this.isLoadingCategories = true;
    this.productService.getCategories().subscribe({
      next: (cats) => {
        this.categories = cats;
        this.isLoadingCategories = false;
        console.log('Categories loaded:', cats);
      },
      error: (err) => {
        console.error('Error fetching categories:', err);
        this.isLoadingCategories = false;
      }
    });
  }
  // --- End load categories method ---


  // --- VERIFY/ADD Method to handle category filter ---
  filterByCategory(category: string): void {
    if (this.selectedCategory === category) {
      return; // Don't reload if same category clicked
    }
    // Update selectedCategory THEN call loadProducts WITHOUT category param
    // loadProducts will use the updated this.selectedCategory
    this.selectedCategory = category;
    this.loadProducts(); // Reload products using the new selection
  }
  // --- End category filter method ---

  addToCart(product: Product): void {
    console.log(`Adding product ${product.id} (${product.name}) to cart`);
    const itemToAdd: AddCartItemDto = { productId: product.id, quantity: 1 };
    this.cartService.addItem(itemToAdd).subscribe({
      next: (updatedCart) => { console.log('Product added successfully'); },
      error: (err) => { alert(`Failed to add ${product.name} to cart. ${err.message || ''}`); }
    });
  }
}