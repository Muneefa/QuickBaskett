export const BRAND = "QuickBasket";
export const TAGLINE = "Groceries. Essentials. Delivered.";
export const FREE_DELIVERY_THRESHOLD = 499;
export const PAYMENT_POLL_MS = 3000;
export const FINAL_PAYMENT_STATUSES = ["Captured", "Failed", "Expired", "Cancelled", "Refunded", "PartiallyRefunded"] as const;

/** The backend stores no product photos yet, so tiles show an emoji picked from the product name. */
const EMOJI: [RegExp, string][] = [
  [/banana/i, "🍌"], [/tomato/i, "🍅"], [/onion/i, "🧅"], [/spinach|palak/i, "🥬"], [/apple/i, "🍎"], [/carrot/i, "🥕"],
  [/milk/i, "🥛"], [/curd|yogurt/i, "🥣"], [/egg/i, "🥚"], [/paneer|cheese/i, "🧀"], [/bread/i, "🍞"], [/butter/i, "🧈"],
  [/rice/i, "🍚"], [/atta|wheat|flour/i, "🌾"], [/dal|lentil/i, "🫘"], [/oil/i, "🌻"], [/chip/i, "🥔"],
  [/coffee/i, "☕"], [/tea/i, "🍵"], [/juice/i, "🧃"], [/dish/i, "🧴"], [/detergent|laundry/i, "🧺"],
  [/toilet|tissue|paper/i, "🧻"], [/hand ?wash|soap/i, "🧼"],
];
export function productEmoji(name: string): string {
  return EMOJI.find(([re]) => re.test(name))?.[1] ?? "🛒";
}
