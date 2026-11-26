import { api } from "./apiClient";
import type { AuthResponse, LoginRequest, RegisterRequest, User } from "@/types/auth";

export const authService = {
  login: (b: LoginRequest) => api<AuthResponse>("/api/auth/login", { method: "POST", body: b, auth: false }),
  register: (b: RegisterRequest) => api<AuthResponse>("/api/auth/register", { method: "POST", body: b, auth: false }),
  me: () => api<User>("/api/auth/me"),
};
