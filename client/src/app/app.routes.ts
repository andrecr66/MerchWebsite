import { Routes } from '@angular/router';
import { ProductListComponent } from './components/product-list/product-list.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { CartComponent } from './components/cart/cart.component'; // Import Cart component

export const routes: Routes = [
    { path: 'products', component: ProductListComponent },
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'cart', component: CartComponent }, // Add cart route
    { path: '', redirectTo: '/products', pathMatch: 'full' },
    // We can add more routes later, e.g., for product details:
    // { path: 'products/:id', component: ProductDetailComponent }, 
    // { path: '**', component: NotFoundComponent } // Wildcard route for 404
];
