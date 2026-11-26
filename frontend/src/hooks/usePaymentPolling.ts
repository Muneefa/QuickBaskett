import { useEffect, useRef, useState } from "react";
import { paymentService } from "@/services/paymentService";
import { FINAL_PAYMENT_STATUSES, PAYMENT_POLL_MS } from "@/utils/constants";
import type { Payment } from "@/types/payment";

/**
 * Polls the server for the payment's status. The page only ever shows "paid" when the server says
 * so (set from a verified signature or a verified webhook), never because of something the browser did.
 */
export function usePaymentPolling(paymentId: string | undefined, enabled = true) {
  const [payment, setPayment] = useState<Payment | null>(null);
  const [error, setError] = useState<string | null>(null);
  const timer = useRef<ReturnType<typeof setInterval> | null>(null);

  const refresh = async () => {
    if (!paymentId) return null;
    try {
      const p = await paymentService.get(paymentId);
      setPayment(p); setError(null);
      return p;
    } catch (e) { setError(e instanceof Error ? e.message : "Could not check payment"); return null; }
  };

  useEffect(() => {
    if (!paymentId || !enabled) return;
    let stopped = false;
    const tick = async () => {
      const p = await refresh();
      if (p && (FINAL_PAYMENT_STATUSES as readonly string[]).includes(p.status) && timer.current) {
        clearInterval(timer.current); timer.current = null;
      }
      if (stopped) return;
    };
    void tick();
    timer.current = setInterval(() => void tick(), PAYMENT_POLL_MS);
    return () => { stopped = true; if (timer.current) clearInterval(timer.current); timer.current = null; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [paymentId, enabled]);

  return { payment, error, refresh };
}
