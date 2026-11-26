const tone: Record<string, string> = {
  Captured: "good", Delivered: "good", Confirmed: "info", Packed: "info", OutForDelivery: "warn", Pending: "warn", PendingPayment: "warn",
  Created: "warn", Authorized: "info", Failed: "bad", Expired: "bad", Cancelled: "bad", Refunded: "info", PartiallyRefunded: "info",
};
const cls: Record<string, string> = { good: "bg-good-bg text-good", info: "bg-info-bg text-info", warn: "bg-warn-bg text-warn", bad: "bg-bad-bg text-bad", none: "bg-surface2 text-muted" };
export function Badge({ status }: { status: string }) {
  const t = tone[status] ?? "none";
  return <span className={`inline-block rounded-full px-2.5 py-1 text-xs font-bold ${cls[t]}`}>{status.replace(/([a-z])([A-Z])/g, "$1 $2")}</span>;
}
