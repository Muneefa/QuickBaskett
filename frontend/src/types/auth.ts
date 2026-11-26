export type Role = "Customer" | "Admin" | "DeliveryPartner";
export interface User { id: string; email: string; fullName: string; phoneNumber?: string | null; role: Role; isAvailable: boolean }
export interface AuthResponse { accessToken: string; expiresAtUtc: string; user: User }
export interface RegisterRequest { email: string; password: string; fullName: string; phoneNumber?: string }
export interface LoginRequest { email: string; password: string }
