// client/src/app/services/product.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError, of } from 'rxjs';
import { Product } from '../models/product.model';
import { environment } from '../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class ProductService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/products`;

    // --- Modify getProducts signature and params ---
    getProducts(category?: string, sortBy?: string): Observable<Product[]> { // <<< Add sortBy parameter
        let params = new HttpParams();

        // Append category if provided and not 'All'
        if (category && category !== 'All') {
            params = params.append('category', category);
        }

        // Append sortBy if provided
        if (sortBy) { // <<< Add this block
            params = params.append('sortBy', sortBy);
        }

        console.log(`ProductService: Fetching products. Category: ${category ?? 'All'}, SortBy: ${sortBy ?? 'Default'}. Params:`, params.toString());
        return this.http.get<Product[]>(this.apiUrl, { params }).pipe(
            catchError(this.handleError)
        );
    }
    // --- End Modify getProducts ---

    getProductById(id: number): Observable<Product> {
        const url = `${this.apiUrl}/${id}`;
        console.log(`ProductService: Fetching product with ID ${id} from ${url}`);
        return this.http.get<Product>(url).pipe(
            catchError(this.handleError)
        );
    }

    // --- ADD getCategories ---
    // For now, return a hardcoded list. Replace with API call later.
    getCategories(): Observable<string[]> {
        console.log('ProductService: Returning hardcoded categories');
        const hardcodedCategories = ['All', 'T-Shirts', 'Hoodies', 'Accessories']; // Match seeding
        return of(hardcodedCategories); // 'of' creates an Observable from the array
        /* // Later, replace with API call:
        return this.http.get<string[]>(`${environment.apiUrl}/categories`).pipe( // Assuming /api/categories endpoint
           catchError(this.handleError)
        );
        */
    }
    // --- END ADD getCategories ---


    private handleError(error: HttpErrorResponse): Observable<never> {
        // ... (keep existing error handler) ...
        let errorMessage = 'An unknown error occurred fetching product data!';
        if (error.error instanceof ErrorEvent) { /* ... */ } else { /* ... */ }
        console.error('ProductService Error:', error);
        return throwError(() => new Error(errorMessage));
    }
}