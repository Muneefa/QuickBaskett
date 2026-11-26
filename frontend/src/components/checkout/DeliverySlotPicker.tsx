export const SLOTS = ["ASAP (about 30 min)", "Today 6–8 PM", "Tomorrow 8–10 AM"];
export function DeliverySlotPicker({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  return (
    <fieldset className="flex flex-wrap gap-2"><legend className="sr-only">Delivery slot</legend>
      {SLOTS.map(s => (
        <label key={s} className={`cursor-pointer rounded-xl border-2 px-3 py-2 text-sm font-bold ${value === s ? "border-brand bg-brand-soft" : "border-line bg-surface"}`}>
          <input type="radio" name="slot" className="sr-only" checked={value === s} onChange={() => onChange(s)} />{s}
        </label>
      ))}
    </fieldset>
  );
}
