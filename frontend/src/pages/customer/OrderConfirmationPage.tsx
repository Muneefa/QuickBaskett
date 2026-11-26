import { Link, useParams } from "react-router-dom";
import { PaymentStatusBanner } from "@/components/payment/PaymentStatusBanner";
import { Spinner, ErrorBox } from "@/components/ui/Spinner";
import { useOrder } from "@/hooks/useOrder";
import { formatCurrency } from "@/utils/formatCurrency";
import { paths } from "@/routes/routePaths";
import type { PaymentStatus } from "@/types/payment";

export function OrderConfirmationPage() {
  const { id } = useParams();
  const { order, error, loading } = useOrder(id, 3000);
  if (loading) return <Spinner />;
  if (error || !order) return <ErrorBox message={error ?? "Order not found"} />;
  const paid = order.latestPaymentStatus === "Captured";
  return (
    <div className="mx-auto max-w-md space-y-4 text-center">
      <p className="text-6xl">{paid || order.status === "Confirmed" ? "✅" : "🕒"}</p>
      <h1 className="text-2xl font-extrabold">{order.status === "Confirmed" ? "Order confirmed" : "Order placed"}</h1>
      <p className="text-muted">{order.orderNumber} · {formatCurrency(order.totalAmount)}</p>
      {order.latestPaymentStatus && <PaymentStatusBanner status={order.latestPaymentStatus as PaymentStatus} />}
      {order.status === "PendingPayment" && !order.latestPaymentStatus && <p className="text-sm text-muted">Waiting for payment confirmation from the server…</p>}
      <div className="flex gap-2"><Link className="flex-1 rounded-xl bg-brand py-3 text-sm font-bold text-brand-ink" to={paths.order(order.id)}>Track order</Link>
        <Link className="flex-1 rounded-xl bg-surface2 py-3 text-sm font-bold" to={paths.home}>Keep shopping</Link></div>
    </div>
  );
}
