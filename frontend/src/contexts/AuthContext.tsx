import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { authService } from "@/services/authService";
import { setUnauthorizedHandler, tokenStore } from "@/services/apiClient";
import type { RegisterRequest, Role, User } from "@/types/auth";

interface AuthApi {
  user: User | null;
  loading: boolean;
  /** Signs in and rejects (with a clear message) if the account's role is not the one this login screen is for. */
  login: (email: string, password: string, expected?: Role) => Promise<User>;
  register: (b: RegisterRequest) => Promise<User>;
  logout: () => void;
}
const Ctx = createContext<AuthApi | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState<boolean>(!!tokenStore.get());

  const logout = useCallback(() => { tokenStore.set(null); setUser(null); }, []);

  useEffect(() => {
    setUnauthorizedHandler(() => setUser(null));
    if (!tokenStore.get()) return;
    authService.me().then(setUser).catch(() => tokenStore.set(null)).finally(() => setLoading(false));
    return () => setUnauthorizedHandler(null);
  }, []);

  const login = useCallback(async (email: string, password: string, expected?: Role) => {
    const res = await authService.login({ email, password });
    if (expected && res.user.role !== expected) {
      // Do not keep a session for the wrong area.
      throw new Error(`This sign-in is for ${label(expected)} accounts only.`);
    }
    tokenStore.set(res.accessToken);
    setUser(res.user);
    return res.user;
  }, []);

  const register = useCallback(async (b: RegisterRequest) => {
    const res = await authService.register(b);
    tokenStore.set(res.accessToken);
    setUser(res.user);
    return res.user;
  }, []);

  const value = useMemo(() => ({ user, loading, login, register, logout }), [user, loading, login, register, logout]);
  return <Ctx.Provider value={value}>{children}</Ctx.Provider>;
}

const label = (r: Role) => (r === "DeliveryPartner" ? "delivery partner" : r.toLowerCase());

export function useAuthContext(): AuthApi {
  const c = useContext(Ctx);
  if (!c) throw new Error("useAuth outside AuthProvider");
  return c;
}
