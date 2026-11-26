import type { PaymentStatus } from "@/types/payment";

const MAP: Partial<Record<PaymentStatus, [string, string]>> = {
  Captured: ["bg-good-bg text-good", "Payment received. Thank you!"],
  Authorized: ["bg-info-bg text-info", "Payment authorised. Waiting for the bank to confirm."],
  Failed: ["bg-bad-bg text-bad", "The payment didn't go through. You haven't been charged. Please try again."],
  Expired: ["bg-bad-bg text-bad", "This payment expired. Start a new one to continue."],
  Cancelled: ["bg-bad-bg text-bad", "This payment was cancelled."],
};
export function PaymentStatusBanner({ status, reason }: { status: PaymentStatus; reason?: string | null }) {
  const [cls, text] = MAP[status] ?? ["bg-warn-bg text-warn", "Waiting for confirmation from the bank…"];
  return <div className={`rounded-xl p-3 text-sm font-bold ${cls}`} role="status">{text}{status === "Failed" && reason ? <span className="block font-normal">{reason}</span> : null}</div>;
}
