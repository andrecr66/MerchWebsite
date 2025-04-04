import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http'; // Import HttpClient
import { Observable } from 'rxjs'; // Import Observable for async operations
// We'll define a Product model/interface later, for now use 'any'
// import { Product } from '../models/product.model';

@Injectable({
    providedIn: 'root' // Makes the service available application-wide
})
export class ProductService {
    // Define the base URL of your backend API
    // TODO: Move this to environment configuration later
    private apiUrl = 'http://localhost:5000/api/products'; // Replace 5000 with the actual port if different

    // Inject HttpClient in the constructor
    constructor(private http: HttpClient) { }

    // Method to get all products
    getProducts(): Observable<any[]> { // Replace 'any[]' with 'Product[]' later
        return this.http.get<any[]>(this.apiUrl);
    }

    // Method to get a single product by ID
    getProduct(id: number): Observable<any> { // Replace 'any' with 'Product' later
        const url = `${this.apiUrl}/${id}`;
        return this.http.get<any>(url);
    }

    // Methods for POST, PUT, DELETE will be added later
}
