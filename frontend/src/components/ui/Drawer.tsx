import { useEffect, type ReactNode } from "react";

export function Drawer({ open, title, onClose, children }: { open: boolean; title: string; onClose: () => void; children: ReactNode }) {
  useEffect(() => {
    if (!open) return;
    const key = (e: KeyboardEvent) => e.key === "Escape" && onClose();
    window.addEventListener("keydown", key);
    return () => window.removeEventListener("keydown", key);
  }, [open, onClose]);
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-black/50" onClick={onClose}>
      <aside role="dialog" aria-modal="true" aria-label={title} onClick={e => e.stopPropagation()}
        className="flex h-full w-full max-w-md flex-col border-l border-line bg-surface">
        <header className="flex items-center justify-between border-b border-line p-4">
          <h2 className="text-lg font-extrabold">{title}</h2>
          <button onClick={onClose} className="h-10 w-10 rounded-xl bg-surface2 text-lg font-bold" aria-label="Close">✕</button>
        </header>
        <div className="flex-1 overflow-auto">{children}</div>
      </aside>
    </div>
  );
}
