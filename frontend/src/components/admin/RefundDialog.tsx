import { useState } from "react";
import { Modal } from "@/components/ui/Modal";
import { Button } from "@/components/ui/Button";
import { newIdempotencyKey } from "@/services/apiClient";
import { formatCurrency } from "@/utils/formatCurrency";
import type { Payment } from "@/types/payment";

/** One Idempotency-Key per dialog opening, so a double click or a retry can't refund twice. */
export function RefundDialog({ payment, onClose, onSubmit }: { payment: Payment | null; onClose: () => void; onSubmit: (amount: number, reason: string, key: string) => Promise<void> }) {
  const [key] = useState(newIdempotencyKey);
  const left = payment ? payment.amount - payment.refundedAmount : 0;
  const [amount, setAmount] = useState<string>("");
  const [reason, setReason] = useState("");
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState<string | null>(null);
  if (!payment) return null;
  const n = amount === "" ? left : Number(amount);
  const submit = async () => {
    if (!(n > 0) || n > left) { setErr(`Enter an amount between 0 and ${formatCurrency(left)}.`); return; }
    setBusy(true); setErr(null);
    try { await onSubmit(n, reason, key); onClose(); } catch (e) { setErr(e instanceof Error ? e.message : "Refund failed"); } finally { setBusy(false); }
  };
  return (
    <Modal open title="Refund payment" onClose={onClose}>
      <p className="mb-3 text-sm text-muted">Refundable: <b className="text-ink">{formatCurrency(left)}</b></p>
      <label className="block text-sm font-semibold">Amount<input type="number" min="0.01" step="0.01" placeholder={String(left)} value={amount} onChange={e => setAmount(e.target.value)} className="mt-1 h-11 w-full rounded-xl border border-line bg-surface2 px-3" /></label>
      <label className="mt-3 block text-sm font-semibold">Reason<input value={reason} onChange={e => setReason(e.target.value)} className="mt-1 h-11 w-full rounded-xl border border-line bg-surface2 px-3" /></label>
      {err && <p className="mt-2 text-sm font-semibold text-bad" role="alert">{err}</p>}
      <div className="mt-4 flex gap-2"><Button disabled={busy} onClick={() => void submit()} className="flex-1">{busy ? "Refunding…" : `Refund ${formatCurrency(n || 0)}`}</Button><Button variant="ghost" onClick={onClose}>Cancel</Button></div>
    </Modal>
  );
}
