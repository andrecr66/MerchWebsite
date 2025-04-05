import { Injectable, inject } from '@angular/core';
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

// Interface for the expected login response (adjust if backend returns more/different info)
interface AuthResponse {
    message: string;
    // token?: string; // Add later when backend returns JWT
}


@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private http = inject(HttpClient);
    // TODO: Move to environment config
    private apiUrl = 'http://localhost:5054/api/auth'; // Use correct port

    // We'll add token storage logic later
    // private tokenKey = 'authToken';

    register(registerData: RegisterDto): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.apiUrl}/register`, registerData);
        // No token handling yet
    }

    login(loginData: LoginDto): Observable<AuthResponse> {
        return this.http.post<AuthResponse>(`${this.apiUrl}/login`, loginData).pipe(
            tap(response => {
                // TODO: Store the token upon successful login
                // if (response.token) {
                //   localStorage.setItem(this.tokenKey, response.token);
                // }
            })
        );
    }

    // TODO: Add logout method
    // logout(): void {
    //   localStorage.removeItem(this.tokenKey);
    //   // Potentially navigate to login page
    // }

    // TODO: Add method to check if user is logged in (e.g., by checking for token)
    // isLoggedIn(): boolean {
    //   return !!localStorage.getItem(this.tokenKey);
    // }

    // TODO: Add method to get the stored token
    // getToken(): string | null {
    //   return localStorage.getItem(this.tokenKey);
    // }
}
