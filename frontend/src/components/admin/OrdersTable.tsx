import { Badge } from "@/components/ui/Badge";
import { formatCurrency } from "@/utils/formatCurrency";
import { formatDateTime } from "@/utils/formatDate";
import type { Order, OrderStatus } from "@/types/order";
import type { User } from "@/types/auth";

const NEXT: Partial<Record<OrderStatus, OrderStatus[]>> = { Confirmed: ["Packed", "Cancelled"], Packed: ["OutForDelivery", "Cancelled"], PendingPayment: ["Cancelled"] };

export function OrdersTable({ orders, partners, onStatus, onAssign }: {
  orders: Order[]; partners: User[]; onStatus: (o: Order, s: OrderStatus) => void; onAssign: (o: Order, partnerId: string) => void;
}) {
  if (!orders.length) return <p className="p-6 text-center text-muted">No orders.</p>;
  return (
    <div className="overflow-x-auto rounded-2xl border border-line bg-surface">
      <table className="w-full min-w-[720px] text-left text-sm">
        <thead className="border-b border-line text-xs uppercase text-muted"><tr><th className="p-3">Order</th><th>Customer</th><th>Total</th><th>Status</th><th>Payment</th><th>Partner</th><th>Action</th></tr></thead>
        <tbody>{orders.map(o => (
          <tr key={o.id} className="border-b border-line last:border-0">
            <td className="p-3"><b>{o.orderNumber}</b><div className="text-xs text-muted">{formatDateTime(o.createdAtUtc)}</div></td>
            <td>{o.deliveryRecipient}</td><td>{formatCurrency(o.totalAmount)}</td>
            <td><Badge status={o.status} /></td><td>{o.latestPaymentStatus ? <Badge status={o.latestPaymentStatus} /> : "—"}</td>
            <td>{["Packed", "OutForDelivery"].includes(o.status) ? (
              <select aria-label={`Assign partner for ${o.orderNumber}`} value={o.deliveryPartner?.id ?? ""} onChange={e => e.target.value && onAssign(o, e.target.value)} className="h-9 rounded-lg border border-line bg-surface px-2">
                <option value="">Assign…</option>{partners.map(p => <option key={p.id} value={p.id}>{p.fullName}</option>)}
              </select>) : (o.deliveryPartner?.fullName ?? "—")}</td>
            <td>{NEXT[o.status] ? (
              <select aria-label={`Change status for ${o.orderNumber}`} value="" onChange={e => e.target.value && onStatus(o, e.target.value as OrderStatus)} className="h-9 rounded-lg border border-line bg-surface px-2">
                <option value="">Move to…</option>{NEXT[o.status]!.map(s => <option key={s} value={s}>{s}</option>)}
              </select>) : "—"}</td>
          </tr>))}</tbody>
      </table>
    </div>
  );
}
