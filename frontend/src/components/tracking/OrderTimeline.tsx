import { formatDateTime, humanize } from "@/utils/formatDate";
import type { OrderEvent, OrderStatus } from "@/types/order";

const STEPS: OrderStatus[] = ["PendingPayment", "Confirmed", "Packed", "OutForDelivery", "Delivered"];
export function OrderTimeline({ status, events }: { status: OrderStatus; events: OrderEvent[] }) {
  if (status === "Cancelled") return <p className="rounded-xl bg-bad-bg p-3 text-sm font-bold text-bad">This order was cancelled.</p>;
  const at = STEPS.indexOf(status);
  return (
    <ol className="space-y-3">
      {STEPS.map((s, i) => {
        const ev = [...events].reverse().find(e => e.status === s);
        return (
          <li key={s} className="flex items-start gap-3" aria-current={i === at ? "step" : undefined}>
            <span className={`mt-0.5 grid h-6 w-6 place-items-center rounded-full text-xs font-extrabold ${i <= at ? "bg-brand text-brand-ink" : "bg-surface2 text-muted"}`}>{i < at ? "✓" : i + 1}</span>
            <div><b className={`text-sm ${i > at ? "text-muted" : ""}`}>{humanize(s)}</b>{ev && <div className="text-xs text-muted">{formatDateTime(ev.occurredAtUtc)}</div>}</div>
          </li>
        );
      })}
    </ol>
  );
}
