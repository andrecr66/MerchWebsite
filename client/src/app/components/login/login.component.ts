import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms'; // Import Reactive Forms modules
import { Router } from '@angular/router'; // Import Router
import { AuthService } from '../../services/auth.service'; // Import AuthService

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule], // Add ReactiveFormsModule
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginForm: FormGroup;
  isLoading = false;
  error: string | null = null;

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  constructor() {
    // Initialize the form group
    this.loginForm = this.fb.group({
      username: ['', [Validators.required]], // Username control with required validator
      password: ['', [Validators.required]]  // Password control with required validator
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched(); // Mark fields as touched to show validation errors
      return; // Stop if form is invalid
    }

    this.isLoading = true;
    this.error = null;

    const loginData = this.loginForm.value;

    this.authService.login(loginData).subscribe({
      next: (response) => {
        console.log('Login successful:', response);
        this.isLoading = false;
        // TODO: Handle successful login (e.g., store token, navigate to protected area)
        this.router.navigate(['/products']); // Navigate back to products for now
      },
      error: (err) => {
        console.error('Login failed:', err);
        this.error = err.error?.message || 'Login failed. Please check your credentials.'; // Display error message from backend if available
        this.isLoading = false;
      }
    });
  }
}
