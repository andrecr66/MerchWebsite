// client/src/app/models/review/review.model.ts
export interface Review { // <<< ENSURE 'export' IS PRESENT
    id: number;
    rating: number;
    comment?: string | null;
    reviewDate: string;
    userId: string;
    userName?: string;
}