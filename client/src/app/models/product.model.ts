// Defines the structure for product data on the frontend
export interface Product {
    id: number;
    name: string;
    description?: string | null; // Optional property, matches C# nullable string
    price: number;
    imageUrl?: string | null; // Optional property
}
