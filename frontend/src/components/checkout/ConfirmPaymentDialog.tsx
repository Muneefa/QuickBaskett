import { Modal } from "@/components/ui/Modal";
import { Button } from "@/components/ui/Button";
import { formatCurrency } from "@/utils/formatCurrency";
import { METHODS } from "./PaymentMethodSelector";
import type { PaymentMethod } from "@/types/payment";

/** The customer must press the button here themselves. Nothing else, including the assistant, can start a payment. */
export function ConfirmPaymentDialog({ open, total, method, busy, onCancel, onConfirm }: {
  open: boolean; total: number; method: PaymentMethod; busy: boolean; onCancel: () => void; onConfirm: () => void;
}) {
  const m = METHODS.find(x => x.id === method)!;
  const cod = method === "CashOnDelivery";
  return (
    <Modal open={open} title={cod ? "Place your order" : "Confirm payment"} onClose={busy ? () => {} : onCancel}>
      <p className="text-sm text-muted">{cod ? "You'll pay in cash on delivery." : `You're about to pay using: ${m.label}.`}</p>
      <p className="my-3 text-3xl font-extrabold">{formatCurrency(total)}</p>
      <p className="mb-4 text-xs text-muted">The amount is calculated by our server from your order, not by this page.</p>
      <div className="flex gap-2"><Button onClick={onConfirm} disabled={busy} className="flex-1">{busy ? "Please wait…" : cod ? "Place order" : `Pay ${formatCurrency(total)}`}</Button>
        <Button variant="ghost" onClick={onCancel} disabled={busy}>Cancel</Button></div>
    </Modal>
  );
}
