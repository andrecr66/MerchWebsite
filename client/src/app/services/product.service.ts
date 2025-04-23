// client/src/app/services/product.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http'; // Import HttpErrorResponse
import { Observable, catchError, throwError } from 'rxjs'; // Import catchError, throwError
import { Product } from '../models/product.model';
import { environment } from '../../environments/environment'; // Ensure correct path

@Injectable({
    providedIn: 'root'
})
export class ProductService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/products`; // Base URL for products API

    // Existing method to get all products
    getProducts(): Observable<Product[]> {
        return this.http.get<Product[]>(this.apiUrl).pipe(
            catchError(this.handleError) // Add error handling
        );
    }

    // --- ADD METHOD to get single product by ID ---
    getProductById(id: number): Observable<Product> {
        const url = `${this.apiUrl}/${id}`; // Construct URL like /api/products/5
        console.log(`ProductService: Fetching product with ID ${id} from ${url}`);
        return this.http.get<Product>(url).pipe(
            catchError(this.handleError)
        );
    }
    // --- END ADD METHOD ---

    // --- ADD Basic Error Handler ---
    private handleError(error: HttpErrorResponse): Observable<never> {
        let errorMessage = 'An unknown error occurred fetching product data!';
        if (error.error instanceof ErrorEvent) {
            errorMessage = `Network Error: ${error.message}`;
        } else {
            // Backend returned an unsuccessful response code.
            errorMessage = `Server returned code ${error.status}, error message is: ${error.message}`;
            if (error.status === 404) {
                errorMessage = `Product not found.`; // Specific message for 404
            }
        }
        console.error('ProductService Error:', error);
        return throwError(() => new Error(errorMessage));
    }
    // --- END Error Handler ---
}