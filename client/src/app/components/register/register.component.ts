import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms'; // Import Reactive Forms modules
import { Router } from '@angular/router'; // Import Router
import { AuthService } from '../../services/auth.service'; // Import AuthService

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule], // Add ReactiveFormsModule
    templateUrl: './register.component.html',
    styleUrl: './register.component.css'
})
export class RegisterComponent {
    registerForm: FormGroup;
    isLoading = false;
    error: string | null = null;
    successMessage: string | null = null;

    private fb = inject(FormBuilder);
    private authService = inject(AuthService);
    private router = inject(Router);

    constructor() {
        // Initialize the form group
        this.registerForm = this.fb.group({
            username: ['', [Validators.required]],
            email: ['', [Validators.required, Validators.email]], // Add email validator
            // Add password complexity validator matching backend DTO
            password: ['', [Validators.required, Validators.pattern('(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{4,8}$')]]
        });
    }

    onSubmit(): void {
        if (this.registerForm.invalid) {
            this.registerForm.markAllAsTouched(); // Mark fields as touched to show validation errors
            return; // Stop if form is invalid
        }

        this.isLoading = true;
        this.error = null;
        this.successMessage = null; // Clear previous success message

        const registerData = this.registerForm.value;

        this.authService.register(registerData).subscribe({
            next: (response) => {
                console.log('Registration successful:', response);
                this.isLoading = false;
                this.successMessage = 'Registration successful! You can now log in.';
                this.registerForm.reset(); // Clear the form
                // Optionally navigate to login after a delay or directly
                // setTimeout(() => this.router.navigate(['/login']), 2000); 
            },
            error: (err) => {
                console.error('Registration failed:', err);
                // Attempt to parse backend validation errors if available
                if (err.error?.errors) {
                    this.error = Object.values(err.error.errors).flat().join(' ');
                } else {
                    this.error = err.error?.message || 'Registration failed. Please try again.';
                }
                this.isLoading = false;
            }
        });
    }
}
