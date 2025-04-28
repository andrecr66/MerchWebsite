// client/src/app/services/product.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http'; // Ensure HttpParams is imported
import { Observable, catchError, throwError, of } from 'rxjs';
import { Product } from '../models/product.model'; // Ensure Product model includes 'gender?'
import { environment } from '../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class ProductService {
    private http = inject(HttpClient);
    // Ensure apiUrl points to the base /products endpoint
    private apiUrl = `${environment.apiUrl}/products`;

    // --- METHOD WITH ALL FILTERS/SORTING ---
    getProducts(
        category?: string,
        sortBy?: string,
        gender?: string,      // <<< Check this exists
        minPrice?: number | null,    // <<< Check this exists
        maxPrice?: number | null     // <<< Check this exists
    ): Observable<Product[]> {
        let params = new HttpParams(); // Start with empty params

        // Append category if provided and valid
        if (category && category !== 'All') {
            params = params.append('category', category);
        }

        // Append sortBy if provided
        if (sortBy) {
            params = params.append('sortBy', sortBy);
        }

        // --- ADD Gender Param Append ---
        if (gender && gender !== 'All') { // Use 'All' or similar convention for no filter
            params = params.append('gender', gender);
        }
        // --- End Gender Param ---

        // --- ADD Price Params Append ---
        // Check specifically for null or undefined before converting to string
        if (minPrice !== null && minPrice !== undefined) {
            params = params.append('minPrice', minPrice.toString());
        }
        if (maxPrice !== null && maxPrice !== undefined) {
            params = params.append('maxPrice', maxPrice.toString());
        }
        // --- End Price Params ---


        // Update logging to include all parameters
        console.log(`ProductService: Fetching products. Category: ${category ?? 'All'}, SortBy: ${sortBy ?? 'Default'}, Gender: ${gender ?? 'All'}, MinPrice: ${minPrice ?? 'N/A'}, MaxPrice: ${maxPrice ?? 'N/A'}. Params:`, params.toString());

        // Make the GET request with the potentially modified params object
        return this.http.get<Product[]>(this.apiUrl, { params }).pipe(
            catchError(this.handleError) // Handle potential errors
        );
    }
    // --- END getProducts METHOD ---


    // Get single product by ID (remains the same)
    getProductById(id: number): Observable<Product> {
        const url = `${this.apiUrl}/${id}`;
        console.log(`ProductService: Fetching product with ID ${id} from ${url}`);
        return this.http.get<Product>(url).pipe(
            catchError(this.handleError)
        );
    }

    // Get Categories (remains hardcoded for now)
    getCategories(): Observable<string[]> {
        console.log('ProductService: Returning hardcoded categories');
        const hardcodedCategories = ['All', 'T-Shirts', 'Hoodies', 'Accessories']; // Match seeding
        return of(hardcodedCategories);
    }


    // Error Handler (remains the same)
    private handleError(error: HttpErrorResponse): Observable<never> {
        let errorMessage = 'An unknown error occurred fetching product data!';

        // --- SSR Safety Check ---
        // Check if ErrorEvent is defined in the current environment before using instanceof
        const isBrowser = typeof window !== 'undefined' && typeof ErrorEvent !== 'undefined';
        if (isBrowser && error.error instanceof ErrorEvent) {
            // --- End SSR Safety Check ---
            // Client-side or network error occurred. Handle it accordingly.
            errorMessage = `Network Error: ${error.message}`;
        } else {
            // Backend returned an unsuccessful response code.
            // The response body may contain clues as to what went wrong.
            errorMessage = `Server returned code ${error.status}, error message is: ${error.message}`;
            if (error.status === 404) {
                errorMessage = `Product data not found.`;
            }
        }
        console.error('ProductService Error:', error);
        // Log the specific backend error if available
        if (!isBrowser || !(error.error instanceof ErrorEvent)) {
            console.error('Backend error details:', error.error);
        }
        return throwError(() => new Error(errorMessage)); // Return user-friendly message
    }
}