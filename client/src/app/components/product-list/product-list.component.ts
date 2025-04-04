import { Component, OnInit, inject } from '@angular/core'; // Import OnInit
import { CommonModule } from '@angular/common'; // Import CommonModule
import { ProductService } from '../../services/product.service'; // Import ProductService
import { Product } from '../../models/product.model'; // Import Product model

@Component({
  selector: 'app-product-list',
  standalone: true, // Indicates this is a standalone component
  imports: [CommonModule], // Import CommonModule here for standalone components
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.css'
})
export class ProductListComponent implements OnInit { // Implement OnInit

  products: Product[] = []; // Property to hold the product list
  isLoading = true; // Flag for loading state
  error: string | null = null; // Property to hold potential errors

  // Inject ProductService using the inject function (modern way)
  private productService = inject(ProductService);

  ngOnInit(): void {
    // Fetch products when the component initializes
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
}
