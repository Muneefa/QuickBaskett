import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from "react";

type Kind = "ok" | "err" | "info";
interface ToastItem { id: number; kind: Kind; text: string }
interface ToastApi { push: (text: string, kind?: Kind) => void }

const Ctx = createContext<ToastApi | null>(null);
let seq = 0;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<ToastItem[]>([]);
  const push = useCallback((text: string, kind: Kind = "ok") => {
    const id = ++seq;
    setItems(l => [...l, { id, kind, text }]);
    setTimeout(() => setItems(l => l.filter(t => t.id !== id)), 4000);
  }, []);
  const api = useMemo(() => ({ push }), [push]);
  const tone = { ok: "bg-good-bg text-good", err: "bg-bad-bg text-bad", info: "bg-info-bg text-info" } as const;
  return (
    <Ctx.Provider value={api}>
      {children}
      <div className="fixed bottom-4 left-1/2 z-[100] flex w-[min(92vw,420px)] -translate-x-1/2 flex-col gap-2" role="status" aria-live="polite">
        {items.map(t => (
          <div key={t.id} className={`rounded-xl border border-line px-4 py-3 text-sm font-semibold shadow-lg ${tone[t.kind]}`}>{t.text}</div>
        ))}
      </div>
    </Ctx.Provider>
  );
}
export function useToast(): ToastApi {
  const c = useContext(Ctx);
  if (!c) throw new Error("useToast outside ToastProvider");
  return c;
}
