/** Loads Razorpay's hosted Checkout script on demand (only when the backend says the provider is Razorpay). Card and bank details are entered inside Razorpay's window, never in our code. */
interface RzpOptions {
  key: string; order_id: string; amount: number; currency: string; name: string; description?: string;
  prefill?: { name?: string; email?: string; contact?: string };
  method?: Record<string, boolean>;
  handler: (r: { razorpay_payment_id: string; razorpay_order_id: string; razorpay_signature: string }) => void;
  modal?: { ondismiss?: () => void };
  theme?: { color?: string };
}
interface RzpInstance { open: () => void; on: (ev: string, cb: (r: { error?: { description?: string } }) => void) => void }
declare global { interface Window { Razorpay?: new (o: RzpOptions) => RzpInstance } }

export function loadRazorpay(): Promise<void> {
  if (window.Razorpay) return Promise.resolve();
  return new Promise((resolve, reject) => {
    const s = document.createElement("script");
    s.src = "https://checkout.razorpay.com/v1/checkout.js";
    s.onload = () => resolve();
    s.onerror = () => reject(new Error("Could not load Razorpay Checkout."));
    document.head.appendChild(s);
  });
}
export type { RzpOptions };
export function newRazorpay(o: RzpOptions) { return new window.Razorpay!(o); }
