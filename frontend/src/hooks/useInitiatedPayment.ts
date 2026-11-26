import { useEffect, useState } from "react";
import { paymentService } from "@/services/paymentService";
import { loadInitiated, saveInitiated } from "@/utils/paymentSession";
import type { InitiatePaymentResponse, PaymentMethod } from "@/types/payment";

/** Returns the saved initiate response for an order, and a function to start a fresh payment (retry). */
export function useInitiatedPayment(orderId: string | undefined, method: PaymentMethod) {
  const [init, setInit] = useState<InitiatePaymentResponse | null>(() => (orderId ? loadInitiated(orderId) : null));
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => { if (orderId) setInit(loadInitiated(orderId)); }, [orderId]);

  const retry = async (m: PaymentMethod = method) => {
    if (!orderId) return;
    setBusy(true); setError(null);
    try { const r = await paymentService.initiate(orderId, m, true); saveInitiated(r); setInit(r); }
    catch (e) { setError(e instanceof Error ? e.message : "Could not start a new payment"); }
    finally { setBusy(false); }
  };
  return { init, retry, busy, error };
}
