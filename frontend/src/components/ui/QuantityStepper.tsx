export function QuantityStepper({ value, onChange, label }: { value: number; onChange: (n: number) => void; label: string }) {
  return (
    <div className="inline-flex items-center overflow-hidden rounded-xl bg-brand text-brand-ink" role="group" aria-label={`Quantity for ${label}`}>
      <button className="h-10 w-10 text-lg font-extrabold" onClick={() => onChange(value - 1)} aria-label={`Remove one ${label}`}>−</button>
      <span className="min-w-6 text-center text-sm font-bold" aria-live="polite">{value}</span>
      <button className="h-10 w-10 text-lg font-extrabold" onClick={() => onChange(value + 1)} aria-label={`Add one ${label}`}>+</button>
    </div>
  );
}
