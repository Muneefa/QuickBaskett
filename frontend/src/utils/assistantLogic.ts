import type { Product } from "@/types/catalog";
import type { ShoppingList } from "@/types/assistant";

/**
 * Scripted shopping-list suggestions. This runs in the browser and can only propose products from the real
 * catalogue; it has no access to checkout or payments and never calls a payment endpoint.
 */
const PLANS: { match: RegExp; title: string; words: [string, number][] }[] = [
  { match: /breakfast|morning/i, title: "Easy breakfast", words: [["milk", 2], ["bread", 1], ["egg", 1], ["banana", 1], ["butter", 1]] },
  { match: /dinner|dal|rice|curry|meal/i, title: "Simple dinner", words: [["rice", 1], ["dal", 1], ["onion", 1], ["tomato", 1], ["oil", 1]] },
  { match: /clean|home|house|laundry/i, title: "Home cleaning", words: [["dish", 1], ["detergent", 1], ["toilet", 1], ["hand", 1]] },
  { match: /snack|party|guest/i, title: "Snacks for guests", words: [["chip", 2], ["juice", 2], ["cheese", 1], ["paneer", 1]] },
  { match: /healthy|veg|fruit|fresh/i, title: "Fresh & healthy", words: [["apple", 1], ["banana", 1], ["spinach", 1], ["carrot", 1], ["tomato", 1]] },
];

export function suggest(text: string, products: Product[]): ShoppingList | null {
  const plan = PLANS.find(p => p.match.test(text));
  if (!plan) return null;
  const items = plan.words.flatMap(([w, qty]) => {
    const p = products.find(x => x.stockQuantity > 0 && x.name.toLowerCase().includes(w));
    return p ? [{ productId: p.id, qty, on: true }] : [];
  });
  return items.length ? { title: plan.title, items, added: false } : null;
}
export const PROMPTS = ["Plan a quick breakfast", "Simple dinner tonight", "Snacks for guests", "Home cleaning basics"];
