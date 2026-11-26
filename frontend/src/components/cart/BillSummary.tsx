import { formatCurrency } from "@/utils/formatCurrency";
import type { Cart } from "@/types/cart";

export function BillSummary({ cart }: { cart: Pick<Cart, "subtotal" | "deliveryFee" | "total"> }) {
  const row = "flex justify-between py-1 text-sm";
  return (
    <dl className="rounded-2xl border border-line bg-surface p-4">
      <div className={row}><dt className="text-muted">Items</dt><dd>{formatCurrency(cart.subtotal)}</dd></div>
      <div className={row}><dt className="text-muted">Delivery</dt><dd>{cart.deliveryFee === 0 ? <b className="text-good">Free</b> : formatCurrency(cart.deliveryFee)}</dd></div>
      <div className="mt-1 flex justify-between border-t border-line pt-2 text-lg font-extrabold"><dt>To pay</dt><dd>{formatCurrency(cart.total)}</dd></div>
    </dl>
  );
}
