// client/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { ProductListComponent } from './components/product-list/product-list.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { CartComponent } from './components/cart/cart.component';
// --- ADD IMPORT FOR CHECKOUT ---
import { CheckoutComponent } from './components/checkout/checkout.component';
// --- END IMPORT ---

export const routes: Routes = [
    // --- Default route should usually be last or handled carefully ---
    // { path: '', redirectTo: '/products', pathMatch: 'full' },
    { path: 'products', component: ProductListComponent },
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'cart', component: CartComponent },
    // --- ADD THE CHECKOUT ROUTE ---
    { path: 'checkout', component: CheckoutComponent },
    // --- END CHECKOUT ROUTE ---
    // Default route moved to end or replace with wildcard
    { path: '', redirectTo: '/products', pathMatch: 'full' },
    // { path: '**', component: NotFoundComponent } // Wildcard for 404
];