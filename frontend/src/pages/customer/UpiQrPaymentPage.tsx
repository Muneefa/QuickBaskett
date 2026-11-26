import { useEffect } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { UpiQrCard } from "@/components/payment/UpiQrCard";
import { PaymentStatusBanner } from "@/components/payment/PaymentStatusBanner";
import { Button } from "@/components/ui/Button";
import { useInitiatedPayment } from "@/hooks/useInitiatedPayment";
import { usePaymentPolling } from "@/hooks/usePaymentPolling";
import { useCountdown } from "@/hooks/useCountdown";
import { paymentService } from "@/services/paymentService";
import { useToast } from "@/contexts/ToastContext";
import { IS_DEV } from "@/config/env";
import { paths } from "@/routes/routePaths";

export function UpiQrPaymentPage() {
  const { orderId } = useParams();
  const nav = useNavigate();
  const toast = useToast();
  const { init, retry, busy, error } = useInitiatedPayment(orderId, "UpiQr");
  const { payment, refresh } = usePaymentPolling(init?.paymentId, !!init);
  const left = useCountdown(init?.upiQr?.expiresAtUtc);
  const status = payment?.status ?? init?.status ?? "Pending";
  const paid = status === "Captured";
  const dead = ["Failed", "Expired", "Cancelled"].includes(status) || left === 0;

  // Only the server's confirmed status moves the customer forward.
  useEffect(() => { if (paid && orderId) nav(paths.orderDone(orderId), { replace: true }); }, [paid, orderId, nav]);

  if (!init?.upiQr) return (
    <div className="mx-auto max-w-md p-6 text-center">
      <p className="mb-3">We couldn't find a QR for this order.</p>
      <Button onClick={() => void retry("UpiQr")} disabled={busy}>Generate a new QR</Button>
      {error && <p className="mt-2 text-sm text-bad" role="alert">{error}</p>}
    </div>
  );

  const simulate = async (success: boolean) => {
    try { await paymentService.devSimulate(init.paymentId, success); await refresh(); }
    catch (e) { toast.push(e instanceof Error ? e.message : "Simulation failed", "err"); }
  };

  return (
    <div className="mx-auto max-w-md space-y-4">
      <h1 className="text-2xl font-extrabold">Scan to pay</h1>
      <UpiQrCard qr={init.upiQr} expired={dead} />
      <PaymentStatusBanner status={dead && status === "Pending" ? "Expired" : status} reason={payment?.failureReason} />
      {dead && <Button className="w-full" onClick={() => void retry("UpiQr")} disabled={busy}>{busy ? "Creating…" : "Get a new QR"}</Button>}
      {error && <p className="text-sm text-bad" role="alert">{error}</p>}
      <p className="text-center text-xs text-muted">We confirm payment with your bank. A screenshot or a message on this page is never treated as proof.</p>
      {IS_DEV && init.provider === "Mock" && !dead && (
        <div className="rounded-xl border border-dashed border-line p-3 text-center">
          <p className="mb-2 text-xs font-bold text-muted">DEV ONLY · mock gateway</p>
          <div className="flex gap-2"><Button variant="ghost" className="flex-1" onClick={() => void simulate(true)}>Simulate paid</Button><Button variant="ghost" className="flex-1" onClick={() => void simulate(false)}>Simulate failed</Button></div>
        </div>
      )}
      <Link to={paths.pay(init.orderId)} className="block text-center text-sm font-bold text-brand">Pay another way</Link>
    </div>
  );
}
