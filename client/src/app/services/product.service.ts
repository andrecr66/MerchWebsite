import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http'; // Import HttpClient
import { Observable } from 'rxjs'; // Import Observable for async operations
// We'll define a Product model/interface later, for now use 'any'
import { Product } from '../models/product.model'; // Uncommented and using the model

@Injectable({
    providedIn: 'root' // Makes the service available application-wide
})
export class ProductService {
    // Define the base URL of your backend API
    // TODO: Move this to environment configuration later
    private apiUrl = 'http://localhost:5054/api/products'; // Updated port to 5054

    // Inject HttpClient in the constructor
    constructor(private http: HttpClient) { }

    // Method to get all products
    getProducts(): Observable<Product[]> { // Using Product[] type
        return this.http.get<Product[]>(this.apiUrl);
    }

    // Method to get a single product by ID
    getProduct(id: number): Observable<Product> { // Using Product type
        const url = `${this.apiUrl}/${id}`;
        return this.http.get<Product>(url);
    }

    // Methods for POST, PUT, DELETE will be added later
}
