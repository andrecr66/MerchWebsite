// client/src/app/models/product.model.ts
export interface Product {
    id: number;
    name: string;
    description?: string | null;
    price: number;
    imageUrl?: string | null;
    category: string;
    gender?: string | null; // <<< ADD Optional gender property
}