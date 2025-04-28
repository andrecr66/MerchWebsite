export interface Product {
    id: number;
    name: string;
    description?: string | null;
    price: number;
    imageUrl?: string | null;
    category: string;
    gender?: string | null;
    averageRating?: number | null; // <<< ADD Optional rating
}