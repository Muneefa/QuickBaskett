import type { Product } from "@/types/catalog";
import type { ShoppingList } from "@/types/assistant";
import { formatCurrency } from "@/utils/formatCurrency";

export function ShoppingListCard({ list, products, onToggle, onAdd }: {
  list: ShoppingList; products: Product[]; onToggle: (productId: string) => void; onAdd: () => void;
}) {
  const rows = list.items.map(i => ({ ...i, p: products.find(p => p.id === i.productId) })).filter(r => r.p);
  const total = rows.filter(r => r.on).reduce((s, r) => s + r.p!.price * r.qty, 0);
  return (
    <div className="mt-2 rounded-xl border border-line bg-surface p-3">
      <b className="text-sm">{list.title}</b>
      <ul className="my-2 space-y-1">
        {rows.map(r => (
          <li key={r.productId}>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={r.on} onChange={() => onToggle(r.productId)} disabled={list.added} />
              <span className="flex-1">{r.p!.name} × {r.qty}</span>
              <span className="text-muted">{formatCurrency(r.p!.price * r.qty)}</span>
            </label>
          </li>
        ))}
      </ul>
      <button onClick={onAdd} disabled={list.added || total === 0}
        className="min-h-10 w-full rounded-lg bg-brand text-sm font-bold text-brand-ink disabled:opacity-50">
        {list.added ? "Added to cart ✓" : `Add to cart · ${formatCurrency(total)}`}
      </button>
      <p className="mt-2 text-xs text-muted">This only fills your cart. You review and pay yourself at checkout.</p>
    </div>
  );
}
