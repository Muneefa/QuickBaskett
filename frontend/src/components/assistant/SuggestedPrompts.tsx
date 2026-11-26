export function SuggestedPrompts({ prompts, onPick }: { prompts: string[]; onPick: (p: string) => void }) {
  return (
    <div className="flex flex-wrap gap-2 p-3">
      {prompts.map(p => <button key={p} onClick={() => onPick(p)} className="rounded-full bg-surface2 px-3 py-2 text-xs font-bold hover:bg-line">{p}</button>)}
    </div>
  );
}
