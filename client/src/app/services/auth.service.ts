import { Injectable, inject, PLATFORM_ID } from '@angular/core'; // Import PLATFORM_ID
import { isPlatformBrowser } from '@angular/common'; // Import isPlatformBrowser
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs'; // Import tap operator

// Define interfaces for the DTOs (matching backend)
interface RegisterDto {
    email: string;
    username: string;
    password?: string; // Password might be optional depending on flow
}

interface LoginDto {
    username: string;
    password?: string;
}

// Interface for the expected login/register response (now includes token)
interface AuthResponse {
    username: string; // Added username
    email: string; // Added email
    token: string; // Added token
}


@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private http = inject(HttpClient);
    private platformId = inject(PLATFORM_ID); // Inject PLATFORM_ID
    // TODO: Move to environment config
    private apiUrl = 'http://localhost:5054/api/auth'; // Use correct port

    private tokenKey = 'authToken'; // Key for storing token in local storage

    register(registerData: RegisterDto): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.apiUrl}/register`, registerData).pipe(
            tap(response => {
                // Optionally log in user immediately by storing token
                // this.storeToken(response.token); 
            })
        );
    }

    login(loginData: LoginDto): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.apiUrl}/login`, loginData).pipe(
            tap(response => {
                // Store the token upon successful login
                this.storeToken(response.token);
            })
        );
    }

    // Store the token in local storage only if in browser
    private storeToken(token: string): void {
        if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem(this.tokenKey, token);
        }
    }

    // Logout method: remove token only if in browser
    logout(): void {
        if (isPlatformBrowser(this.platformId)) {
            localStorage.removeItem(this.tokenKey);
            // Potentially navigate to login page or refresh state
        }
    }

    // Check if user is logged in (only possible in browser)
    isLoggedIn(): boolean {
        if (isPlatformBrowser(this.platformId)) {
            return !!localStorage.getItem(this.tokenKey);
        }
        return false; // Cannot be logged in on server
    }

    // Get the stored token (only possible in browser)
    getToken(): string | null {
        if (isPlatformBrowser(this.platformId)) {
            return localStorage.getItem(this.tokenKey);
        }
        return null; // No token on server
    }
}
