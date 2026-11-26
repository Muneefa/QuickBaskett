import { useState } from "react";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { CodCollectionPrompt } from "./CodCollectionPrompt";
import { formatCurrency } from "@/utils/formatCurrency";
import type { Order } from "@/types/order";

export function DeliveryTaskCard({ order, onPickup, onDeliver }: { order: Order; onPickup: () => void; onDeliver: (codCollected: boolean) => void }) {
  const [cash, setCash] = useState(false);
  const unpaid = order.latestPaymentStatus !== "Captured";
  return (
    <article className="space-y-3 rounded-2xl border border-line bg-surface p-4">
      <div className="flex items-center justify-between"><b>{order.orderNumber}</b><Badge status={order.status} /></div>
      <p className="text-sm">{order.deliveryRecipient} · <a className="font-bold text-brand" href={`tel:${order.deliveryPhone}`}>{order.deliveryPhone}</a><br /><span className="text-muted">{order.deliveryAddress}</span></p>
      <p className="text-sm text-muted">{order.items.map(i => `${i.productName} × ${i.quantity}`).join(", ")}</p>
      {order.status === "Packed" && <Button className="w-full" onClick={onPickup}>Picked up, start delivery</Button>}
      {order.status === "OutForDelivery" && (
        <>
          {unpaid && <CodCollectionPrompt amount={formatCurrency(order.totalAmount)} checked={cash} onChange={setCash} />}
          <Button className="w-full" disabled={unpaid && !cash} onClick={() => onDeliver(unpaid && cash)}>Mark delivered</Button>
        </>
      )}
    </article>
  );
}
