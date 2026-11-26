import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { PaymentStatusBanner } from "@/components/payment/PaymentStatusBanner";
import { RazorpayCheckoutButton } from "@/components/payment/RazorpayCheckoutButton";
import { PaymentMethodSelector } from "@/components/checkout/PaymentMethodSelector";
import { Button } from "@/components/ui/Button";
import { Spinner } from "@/components/ui/Spinner";
import { useInitiatedPayment } from "@/hooks/useInitiatedPayment";
import { usePaymentPolling } from "@/hooks/usePaymentPolling";
import { useOrder } from "@/hooks/useOrder";
import { paymentService } from "@/services/paymentService";
import { useToast } from "@/contexts/ToastContext";
import { IS_DEV } from "@/config/env";
import { formatCurrency } from "@/utils/formatCurrency";
import { paths } from "@/routes/routePaths";
import type { PaymentMethod } from "@/types/payment";

export function PaymentPage() {
  const { orderId } = useParams();
  const nav = useNavigate();
  const toast = useToast();
  const { order } = useOrder(orderId);
  const [method, setMethod] = useState<PaymentMethod>("Upi");
  const { init, retry, busy, error } = useInitiatedPayment(orderId, method);
  const { payment, refresh } = usePaymentPolling(init?.paymentId, !!init);
  const [msg, setMsg] = useState<string | null>(null);
  const status = payment?.status ?? init?.status;

  useEffect(() => { if (status === "Captured" && orderId) nav(paths.orderDone(orderId), { replace: true }); }, [status, orderId, nav]);
  useEffect(() => { if (init?.method === "UpiQr" && orderId) nav(paths.payQr(orderId), { replace: true }); }, [init, orderId, nav]);

  if (!order) return <Spinner />;
  const retryable = !init || ["Failed", "Expired", "Cancelled"].includes(status ?? "");

  const simulate = async (success: boolean) => {
    if (!init) return;
    try {
      const r = await paymentService.devSimulate(init.paymentId, success);
      if (success) await paymentService.verify({ paymentId: init.paymentId, gatewayOrderId: r.gatewayOrderId, gatewayPaymentId: r.gatewayPaymentId, signature: r.checkoutSignature });
      await refresh();
    } catch (e) { toast.push(e instanceof Error ? e.message : "Simulation failed", "err"); }
  };

  return (
    <div className="mx-auto max-w-md space-y-4">
      <h1 className="text-2xl font-extrabold">Complete your payment</h1>
      <p className="text-sm text-muted">Order {order.orderNumber} · <b className="text-ink">{formatCurrency(order.totalAmount)}</b></p>
      {status && <PaymentStatusBanner status={status} reason={payment?.failureReason} />}
      {msg && <p className="rounded-xl bg-bad-bg p-3 text-sm font-semibold text-bad" role="alert">{msg}</p>}
      {error && <p className="text-sm text-bad" role="alert">{error}</p>}

      {retryable ? (
        <>
          <PaymentMethodSelector value={method} onChange={setMethod} />
          <Button className="w-full" disabled={busy} onClick={() => { setMsg(null); void retry(method).then(() => { if (method === "CashOnDelivery" && orderId) nav(paths.orderDone(orderId)); }); }}>
            {busy ? "Starting…" : `Pay ${formatCurrency(order.totalAmount)}`}
          </Button>
        </>
      ) : init && init.status !== "Captured" && (
        <>
          {init.provider === "Razorpay"
            ? <RazorpayCheckoutButton init={init} onDone={() => void refresh()} onError={setMsg} />
            : IS_DEV && init.provider === "Mock" && (
              <div className="rounded-xl border border-dashed border-line p-3 text-center">
                <p className="mb-2 text-xs font-bold text-muted">DEV ONLY · mock gateway</p>
                <div className="flex gap-2"><Button variant="ghost" className="flex-1" onClick={() => void simulate(true)}>Simulate paid</Button><Button variant="ghost" className="flex-1" onClick={() => void simulate(false)}>Simulate failed</Button></div>
              </div>)}
          <p className="text-center text-xs text-muted">Card and bank details are entered in Razorpay's secure window. We never see or store them.</p>
        </>
      )}
      <Link to={paths.order(order.id)} className="block text-center text-sm font-bold text-brand">View this order</Link>
    </div>
  );
}
