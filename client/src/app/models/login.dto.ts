// client/src/app/models/login.dto.ts
export interface LoginDto {
    username: string;
    password?: string; // Keep password optional if backend handles it differently sometimes
}