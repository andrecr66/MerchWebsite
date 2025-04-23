// client/src/app/components/product-list/product-list.component.ts
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product.model';
import { CartService } from '../../services/cart.service'; // <<<--- Import CartService
import { AddCartItemDto } from '../../models/cart/add-cart-item.dto'; // <<<--- Import DTO

import { RouterLink } from '@angular/router'; // <<< ADD RouterLink Import

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterLink], // <<< ADD RouterLink to imports
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  isLoading = true;
  error: string | null = null;

  private productService = inject(ProductService);
  private cartService = inject(CartService); // <<<--- Inject CartService

  ngOnInit(): void {
    this.productService.getProducts().subscribe({
      next: (data) => {
        this.products = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching products:', err);
        this.error = 'Failed to load products. Please try again later.';
        this.isLoading = false;
      }
    });
  }

  // --- Method to add item to cart ---
  addToCart(product: Product): void {
    console.log(`Adding product ${product.id} (${product.name}) to cart`);
    // Default quantity to 1 for now when adding from list
    const itemToAdd: AddCartItemDto = {
      productId: product.id,
      quantity: 1
    };

    this.cartService.addItem(itemToAdd).subscribe({
      // Optional: Add user feedback on success/error
      next: (updatedCart) => {
        console.log('Product added successfully, new cart:', updatedCart);
        // Maybe show a temporary confirmation message
      },
      error: (err) => {
        console.error(`Error adding product ${product.id} to cart:`, err);
        // Show user-friendly error message
        alert(`Failed to add ${product.name} to cart. ${err.message || ''}`);
      }
    });
  }
  // --- End add to cart method ---
}