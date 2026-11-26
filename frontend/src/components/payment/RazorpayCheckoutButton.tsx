import { useState } from "react";
import { Button } from "@/components/ui/Button";
import { loadRazorpay, newRazorpay } from "@/utils/razorpay";
import { paymentService } from "@/services/paymentService";
import { useAuth } from "@/hooks/useAuth";
import { formatCurrency } from "@/utils/formatCurrency";
import type { InitiatePaymentResponse, PaymentMethod } from "@/types/payment";

const only = (m: PaymentMethod): Record<string, boolean> | undefined =>
  m === "Upi" ? { upi: true, card: false, netbanking: false, wallet: false } :
  m === "Card" ? { card: true, upi: false, netbanking: false, wallet: false } :
  m === "NetBanking" ? { netbanking: true, upi: false, card: false, wallet: false } : undefined;

/** Opens Razorpay's own window. Our code only receives the signed result and forwards it to the server, which does the checking. */
export function RazorpayCheckoutButton({ init, onDone, onError }: { init: InitiatePaymentResponse; onDone: () => void; onError: (m: string) => void }) {
  const { user } = useAuth();
  const [busy, setBusy] = useState(false);
  const open = async () => {
    if (!init.publicKeyId || !init.gatewayOrderId) { onError("Payment details are missing. Please go back and try again."); return; }
    setBusy(true);
    try {
      await loadRazorpay();
      const rzp = newRazorpay({
        key: init.publicKeyId, order_id: init.gatewayOrderId, amount: init.amountInPaise, currency: init.currency, name: "QuickBasket",
        prefill: { name: user?.fullName, email: user?.email, contact: user?.phoneNumber ?? undefined }, method: only(init.method),
        theme: { color: "#15803f" },
        handler: r => {
          paymentService.verify({ paymentId: init.paymentId, gatewayOrderId: r.razorpay_order_id, gatewayPaymentId: r.razorpay_payment_id, signature: r.razorpay_signature })
            .then(onDone).catch(e => onError(e instanceof Error ? e.message : "Could not verify the payment"));
        },
        modal: { ondismiss: () => setBusy(false) },
      });
      rzp.on("payment.failed", r => onError(r.error?.description ?? "The payment failed"));
      rzp.open();
    } catch (e) { onError(e instanceof Error ? e.message : "Could not open checkout"); setBusy(false); }
  };
  return <Button className="w-full" onClick={() => void open()} disabled={busy}>{busy ? "Opening secure checkout…" : `Pay ${formatCurrency(init.amount)}`}</Button>;
}
