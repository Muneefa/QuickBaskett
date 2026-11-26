import { API_BASE_URL } from "@/config/env";
import type { ProblemDetails } from "@/types/api";

const TOKEN_KEY = "qb.token";
let token: string | null = null;
try { token = sessionStorage.getItem(TOKEN_KEY); } catch { /* storage unavailable */ }

export const tokenStore = {
  get: () => token,
  set(t: string | null) {
    token = t;
    try { t ? sessionStorage.setItem(TOKEN_KEY, t) : sessionStorage.removeItem(TOKEN_KEY); } catch { /* ignore */ }
  },
};

let onUnauthorized: (() => void) | null = null;
export const setUnauthorizedHandler = (fn: (() => void) | null) => { onUnauthorized = fn; };

export class ApiError extends Error {
  constructor(public status: number, message: string) { super(message); }
}

interface Options { method?: string; body?: unknown; headers?: Record<string, string>; auth?: boolean }

export async function api<T>(path: string, opts: Options = {}): Promise<T> {
  const headers: Record<string, string> = { Accept: "application/json", ...opts.headers };
  if (opts.body !== undefined) headers["Content-Type"] = "application/json";
  if (opts.auth !== false && token) headers.Authorization = `Bearer ${token}`;

  let res: Response;
  try {
    res = await fetch(`${API_BASE_URL}${path}`, {
      method: opts.method ?? "GET", headers, body: opts.body !== undefined ? JSON.stringify(opts.body) : undefined,
    });
  } catch {
    throw new ApiError(0, "Can't reach the server. Check that the API is running.");
  }

  if (res.status === 401 && opts.auth !== false && token) { tokenStore.set(null); onUnauthorized?.(); }
  if (res.status === 204) return undefined as T;

  const text = await res.text();
  let data: unknown = undefined;
  if (text) { try { data = JSON.parse(text); } catch { data = undefined; } }

  if (!res.ok) {
    const p = (data ?? {}) as ProblemDetails & { errors?: Record<string, string[]> };
    const first = p.errors ? Object.values(p.errors).flat()[0] : undefined;
    throw new ApiError(res.status, p.detail || first || p.title || `Request failed (${res.status})`);
  }
  return data as T;
}

export const newIdempotencyKey = (): string =>
  typeof crypto !== "undefined" && "randomUUID" in crypto ? crypto.randomUUID() : `${Date.now()}-${Math.random().toString(16).slice(2)}`;
