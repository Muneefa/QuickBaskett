import { Fragment, useCallback, useEffect, useState } from "react";
import { RefundDialog } from "@/components/admin/RefundDialog";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Spinner, ErrorBox } from "@/components/ui/Spinner";
import { adminService } from "@/services/adminService";
import { useToast } from "@/contexts/ToastContext";
import { IS_DEV } from "@/config/env";
import { formatCurrency } from "@/utils/formatCurrency";
import { formatDateTime } from "@/utils/formatDate";
import type { Payment } from "@/types/payment";

export function AdminPaymentsPage() {
  const toast = useToast();
  const [list, setList] = useState<Payment[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [refunding, setRefunding] = useState<Payment | null>(null);
  const [open, setOpen] = useState<string | null>(null);
  const load = useCallback(() => adminService.payments().then(r => setList(r.items)).catch(e => setError(e instanceof Error ? e.message : "Could not load")), []);
  useEffect(() => { void load(); }, [load]);
  if (error) return <ErrorBox message={error} />;
  if (!list) return <Spinner />;
  const canRefund = (p: Payment) => ["Captured", "PartiallyRefunded"].includes(p.status) && p.amount - p.refundedAmount > 0;

  return (
    <>
      <h1 className="mb-3 text-2xl font-extrabold">Payments</h1>
      <div className="overflow-x-auto rounded-2xl border border-line bg-surface">
        <table className="w-full min-w-[720px] text-left text-sm">
          <thead className="border-b border-line text-xs uppercase text-muted"><tr><th className="p-3">When</th><th>Method</th><th>Amount</th><th>Refunded</th><th>Status</th><th></th></tr></thead>
          <tbody>{list.map(p => (
            <Fragment key={p.id}>
              <tr className="border-b border-line">
                <td className="p-3">{formatDateTime(p.createdAtUtc)}<div className="text-xs text-muted">{p.provider}</div></td>
                <td>{p.method}</td><td>{formatCurrency(p.amount)}</td><td>{p.refundedAmount ? formatCurrency(p.refundedAmount) : "—"}</td><td><Badge status={p.status} /></td>
                <td className="space-x-2 p-2 text-right">
                  <button className="text-xs font-bold text-brand" onClick={() => setOpen(open === p.id ? null : p.id)}>{open === p.id ? "Hide" : "Details"}</button>
                  {canRefund(p) && <Button variant="danger" className="!min-h-8 !px-3 !py-1 text-xs" onClick={() => setRefunding(p)}>Refund</Button>}
                </td>
              </tr>
              {open === p.id && (
                <tr className="border-b border-line bg-surface2"><td colSpan={6} className="p-3 text-xs">
                  <div className="mb-1 text-muted">Gateway order {p.gatewayOrderId ?? "—"} · payment {p.gatewayPaymentId ?? "—"}</div>
                  <ul className="space-y-1">{p.transactions.map((t, i) => <li key={i}>{formatDateTime(t.occurredAtUtc)} · {t.type} → <b>{t.statusAfter}</b> ({t.source}){t.message ? ` · ${t.message}` : ""}</li>)}</ul>
                  {p.refunds.map(r => (
                    <div key={r.id} className="mt-1">Refund {formatCurrency(r.amount)} · <b>{r.status}</b>
                      {IS_DEV && r.status !== "Processed" && <button className="ml-2 font-bold text-brand" onClick={() => void adminService.devProcessRefund(r.id).then(load).catch(e => toast.push(e instanceof Error ? e.message : "Failed", "err"))}>(dev) process</button>}</div>))}
                </td></tr>)}
            </Fragment>))}</tbody>
        </table>
      </div>
      <RefundDialog key={refunding?.id ?? "none"} payment={refunding} onClose={() => setRefunding(null)}
        onSubmit={async (amount, reason, key) => { await adminService.refund(refunding!.id, amount, reason, key); toast.push("Refund requested"); await load(); }} />
    </>
  );
}
