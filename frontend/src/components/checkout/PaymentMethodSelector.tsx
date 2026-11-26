import type { PaymentMethod } from "@/types/payment";

export const METHODS: { id: PaymentMethod; label: string; hint: string; icon: string }[] = [
  { id: "UpiQr", label: "Scan UPI QR", hint: "Scan with any UPI app. The QR is made for this order and amount.", icon: "🔳" },
  { id: "Upi", label: "UPI app", hint: "Pay in PhonePe, Google Pay, Paytm or another UPI app.", icon: "📲" },
  { id: "Card", label: "Credit / debit card", hint: "Entered securely in Razorpay's window.", icon: "💳" },
  { id: "NetBanking", label: "Net banking", hint: "Pay through your bank.", icon: "🏦" },
  { id: "CashOnDelivery", label: "Cash on delivery", hint: "Pay the delivery partner when it arrives.", icon: "💵" },
];
export function PaymentMethodSelector({ value, onChange }: { value: PaymentMethod; onChange: (m: PaymentMethod) => void }) {
  return (
    <fieldset className="space-y-2"><legend className="sr-only">Payment method</legend>
      {METHODS.map(m => (
        <label key={m.id} className={`flex cursor-pointer items-center gap-3 rounded-xl border-2 p-3 ${value === m.id ? "border-brand bg-brand-soft" : "border-line bg-surface"}`}>
          <input type="radio" name="method" checked={value === m.id} onChange={() => onChange(m.id)} />
          <span className="text-2xl" aria-hidden>{m.icon}</span>
          <span className="text-sm"><b>{m.label}</b><br /><span className="text-muted">{m.hint}</span></span>
        </label>
      ))}
    </fieldset>
  );
}
