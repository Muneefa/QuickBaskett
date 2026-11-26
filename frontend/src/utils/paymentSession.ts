import type { InitiatePaymentResponse } from "@/types/payment";

/** Keeps the initiate response for the current tab so a refresh on the payment screen doesn't lose the QR or gateway order. Holds no secrets. */
const key = (orderId: string) => `qb.pay.${orderId}`;
export function saveInitiated(r: InitiatePaymentResponse) { try { sessionStorage.setItem(key(r.orderId), JSON.stringify(r)); } catch { /* ignore */ } }
export function loadInitiated(orderId: string): InitiatePaymentResponse | null {
  try { const s = sessionStorage.getItem(key(orderId)); return s ? (JSON.parse(s) as InitiatePaymentResponse) : null; } catch { return null; }
}
export function clearInitiated(orderId: string) { try { sessionStorage.removeItem(key(orderId)); } catch { /* ignore */ } }
