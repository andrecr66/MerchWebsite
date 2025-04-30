// client/src/app/components/login/login.component.ts
import { Component, inject, OnInit } from '@angular/core'; // Added OnInit
import { CommonModule } from '@angular/common'; // For *ngIf
import { Router, RouterLink } from '@angular/router'; // Import Router and RouterLink
// --- Import Reactive Forms ---
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
// --- End Import ---
import { AuthService } from '../../services/auth.service';
import { LoginDto } from '../../models/login.dto'; // Assuming this exists

@Component({
  selector: 'app-login',
  standalone: true,
  // --- Add ReactiveFormsModule and RouterLink ---
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'] // Use plural
})
export class LoginComponent implements OnInit { // Implement OnInit

  // --- Injected Services ---
  private authService = inject(AuthService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  // --- End Injected Services ---

  // --- Component Properties ---
  loginForm: FormGroup; // The reactive form group
  isSubmitting = false; // Flag for loading state
  errorMessage: string | null = null; // To display login errors
  // --- End Component Properties ---

  constructor() {
    // --- Initialize the form ---
    this.loginForm = this.fb.group({
      // Form controls match LoginDto and template formControlName attributes
      username: ['', [Validators.required]], // Username is required
      password: ['', [Validators.required]]  // Password is required
    });
    // --- End Form Initialization ---
  }

  ngOnInit(): void {
    // Optional: Redirect if already logged in?
    if (this.authService.isLoggedIn()) {
      console.log('User already logged in, redirecting from login page...');
      this.router.navigate(['/products']); // Or user dashboard
    }
  }

  // --- Form Submission Handler ---
  onSubmit(): void {
    this.errorMessage = null; // Clear previous errors
    this.loginForm.markAllAsTouched(); // Mark fields for validation display

    if (this.loginForm.invalid) {
      console.log('Login form invalid.');
      return; // Stop if form has validation errors
    }

    this.isSubmitting = true; // Set loading state

    // Prepare data matching LoginDto
    const loginData: LoginDto = {
      username: this.loginForm.value.username,
      password: this.loginForm.value.password
    };

    console.log('Attempting login with:', loginData.username);

    this.authService.login(loginData).subscribe({
      // Inside onSubmit -> subscribe
      next: (user) => { // 'user' here is of type AuthResponse
        this.isSubmitting = false;
        console.log('Login successful for:', user.username); // <<< FIX: Use lowercase 'username'
        this.router.navigate(['/products']);
      },
      error: (err) => {
        this.isSubmitting = false;
        console.error('Login failed:', err);
        // Display the error message from the service/backend
        this.errorMessage = err.message || 'Login failed. Please check username/password.';
      }
    });
  }
  // --- End Submission Handler ---


  // --- Helper Getters for Template Validation ---
  get username() { return this.loginForm.get('username'); }
  get password() { return this.loginForm.get('password'); }
  // --- End Helper Getters ---
}