import { Link } from "react-router-dom";
import { CartItemRow } from "@/components/cart/CartItemRow";
import { BillSummary } from "@/components/cart/BillSummary";
import { Button } from "@/components/ui/Button";
import { useCart } from "@/hooks/useCart";
import { paths } from "@/routes/routePaths";

export function CartPage() {
  const { cart, setQty, clear } = useCart();
  if (!cart.items.length) return (
    <div className="p-10 text-center"><p className="text-5xl">🧺</p><h1 className="mt-2 text-2xl font-extrabold">Your basket is empty</h1>
      <Link to={paths.home} className="mt-3 inline-block font-bold text-brand">Start shopping</Link></div>
  );
  return (
    <div className="grid gap-6 md:grid-cols-[1fr_320px]">
      <section>
        <div className="flex items-center justify-between"><h1 className="text-2xl font-extrabold">Your basket</h1><button onClick={() => void clear()} className="text-sm font-bold text-bad">Clear</button></div>
        <ul className="divide-y divide-line">{cart.items.map(i => <CartItemRow key={i.productId} item={i} onQty={n => void setQty(i.productId, n)} />)}</ul>
      </section>
      <aside className="space-y-3"><BillSummary cart={cart} /><Link to={paths.checkout}><Button className="w-full">Continue to checkout</Button></Link></aside>
    </div>
  );
}
