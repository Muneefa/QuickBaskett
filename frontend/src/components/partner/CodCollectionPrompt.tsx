export function CodCollectionPrompt({ amount, checked, onChange }: { amount: string; checked: boolean; onChange: (v: boolean) => void }) {
  return (
    <label className="flex items-start gap-3 rounded-xl bg-warn-bg p-3 text-sm font-semibold text-warn">
      <input type="checkbox" className="mt-1" checked={checked} onChange={e => onChange(e.target.checked)} />
      <span>I collected {amount} in cash from the customer.</span>
    </label>
  );
}
