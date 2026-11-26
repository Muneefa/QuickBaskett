export function FilterBar({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  return (
    <label className="mb-3 flex items-center gap-2 text-sm font-semibold">
      Sort
      <select value={value} onChange={e => onChange(e.target.value)} className="h-10 rounded-xl border border-line bg-surface px-3">
        <option value="relevance">Relevance</option><option value="low">Price: low to high</option><option value="high">Price: high to low</option>
      </select>
    </label>
  );
}
export const sortProducts = <T extends { price: number }>(list: T[], mode: string): T[] =>
  mode === "low" ? [...list].sort((a, b) => a.price - b.price) : mode === "high" ? [...list].sort((a, b) => b.price - a.price) : list;
