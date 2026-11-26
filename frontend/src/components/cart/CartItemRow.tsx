import { QuantityStepper } from "@/components/ui/QuantityStepper";
import { formatCurrency } from "@/utils/formatCurrency";
import { productEmoji } from "@/utils/constants";
import type { CartItem } from "@/types/cart";

export function CartItemRow({ item, onQty }: { item: CartItem; onQty: (n: number) => void }) {
  return (
    <li className="flex items-center gap-3 py-3">
      <span className="grid h-14 w-14 place-items-center rounded-xl bg-surface2 text-3xl">{productEmoji(item.name)}</span>
      <div className="flex-1"><b className="text-sm">{item.name}</b><div className="text-xs text-muted">{item.unit} · {formatCurrency(item.unitPrice)}</div></div>
      <QuantityStepper value={item.quantity} label={item.name} onChange={onQty} />
      <b className="w-20 text-right text-sm">{formatCurrency(item.lineTotal)}</b>
    </li>
  );
}
