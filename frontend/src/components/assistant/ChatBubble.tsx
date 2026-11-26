import type { ReactNode } from "react";
export function ChatBubble({ me, children }: { me?: boolean; children: ReactNode }) {
  return (
    <div className={`max-w-[88%] rounded-2xl px-3 py-2 text-sm ${me ? "ml-auto bg-brand text-brand-ink" : "bg-surface2"}`}>{children}</div>
  );
}
