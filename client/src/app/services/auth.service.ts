// client/src/app/services/auth.service.ts
import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http'; // Import HttpErrorResponse
import { Observable, tap, throwError, catchError } from 'rxjs'; // Import throwError, catchError
// --- FIX: Correct the environment import path ---
import { environment } from '../../environments/environment';
// --- End Path Correction ---
import { CartService } from './cart.service';
import { Cart } from '../models/cart/cart.model'; // Import Cart if needed

// Keep your DTO interfaces
interface RegisterDto { email: string; username: string; password?: string; }
interface LoginDto { username: string; password?: string; }
interface AuthResponse { username: string; email: string; token: string; } // Ensure token is here

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private http = inject(HttpClient);
    private platformId = inject(PLATFORM_ID);
    private cartService = inject(CartService);

    private apiUrl = `${environment.apiUrl}/auth`; // Use environment variable
    private tokenKey = 'authToken';

    register(registerData: RegisterDto): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.apiUrl}/register`, registerData).pipe(
            tap(response => {
                console.log('Registration successful:', response);
            }),
            catchError(this.handleError) // <<< Add catchError
        );
    }

    login(loginData: LoginDto): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.apiUrl}/login`, loginData).pipe(
            tap((response: AuthResponse) => { // <<< Add explicit type here
                this.storeToken(response.token); // Now token property should be found
                console.log('Login successful, token stored.');
                if (isPlatformBrowser(this.platformId)) {
                    // Assuming loadCart exists on CartService
                    this.cartService.loadCart().subscribe({
                        next: (cart: Cart) => console.log('AuthService: Cart loaded successfully after login.'), // Type added
                        error: (err: any) => console.error('AuthService: Failed to load cart after login:', err) // Type added
                    });
                }
            }),
            catchError(this.handleError) // <<< Add catchError
        );
    }

    private storeToken(token: string): void { /* ... */ }

    logout(): void {
        if (isPlatformBrowser(this.platformId)) {
            const token = this.getToken();
            localStorage.removeItem(this.tokenKey);
            console.log('AuthService: Token removed.');
            if (token) {
                // Assuming clearLocalCart exists on CartService
                this.cartService.clearLocalCart();
                console.log('AuthService: Local cart cleared on logout.');
            }
        }
    }

    // --- FIX: Ensure methods have return paths (Your code looked OK, maybe side effect of other errors) ---
    isLoggedIn(): boolean {
        return isPlatformBrowser(this.platformId) && !!localStorage.getItem(this.tokenKey);
    }

    getToken(): string | null {
        return isPlatformBrowser(this.platformId) ? localStorage.getItem(this.tokenKey) : null;
    }
    // --- End Fix ---

    // --- ADD Basic HandleError if missing ---
    private handleError(error: HttpErrorResponse): Observable<never> {
        console.error('AuthService encountered an error:', error);
        const message = error.error?.title || error.error?.message || error.message || 'Authentication operation failed';
        return throwError(() => new Error(message));
    }
    // --- End HandleError ---
}