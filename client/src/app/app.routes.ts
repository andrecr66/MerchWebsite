import { Routes } from '@angular/router';
import { ProductListComponent } from './components/product-list/product-list.component'; // Import the component

export const routes: Routes = [
    { path: 'products', component: ProductListComponent }, // Route for product list
    { path: '', redirectTo: '/products', pathMatch: 'full' }, // Default route redirects to /products
    // We can add more routes later, e.g., for product details:
    // { path: 'products/:id', component: ProductDetailComponent }, 
    // { path: '**', component: NotFoundComponent } // Wildcard route for 404
];
