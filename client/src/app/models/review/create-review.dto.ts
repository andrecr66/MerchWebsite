export interface CreateReviewDto {
    rating: number; // Must be 1-5
    comment?: string | null;
}