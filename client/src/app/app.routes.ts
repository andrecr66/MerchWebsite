// client/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { ProductListComponent } from './components/product-list/product-list.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { CartComponent } from './components/cart/cart.component';
import { CheckoutComponent } from './components/checkout/checkout.component';
// --- IMPORT ProductDetailComponent (will create next) ---
import { ProductDetailComponent } from './components/product-detail/product-detail.component';
import { OrderHistoryComponent } from './components/order-history/order-history.component'; // <<< ADD IMPORT

export const routes: Routes = [
    { path: 'products', component: ProductListComponent },
    // --- ADD Route for Product Detail ---
    // ':id' creates a route parameter named 'id'
    { path: 'products/:id', component: ProductDetailComponent },
    // --- END Product Detail Route ---
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'cart', component: CartComponent },
    { path: 'checkout', component: CheckoutComponent },
    { path: 'orders', component: OrderHistoryComponent }, // <<< ADD ROUTE
    // Default route
    { path: '', redirectTo: '/products', pathMatch: 'full' },
    // { path: '**', component: NotFoundComponent } // Wildcard for 404
];