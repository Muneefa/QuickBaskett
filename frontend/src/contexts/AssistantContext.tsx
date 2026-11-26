import { createContext, useContext, useMemo, useState, type ReactNode } from "react";

interface AssistantApi { open: boolean; setOpen: (v: boolean) => void }
const Ctx = createContext<AssistantApi | null>(null);
export function AssistantProvider({ children }: { children: ReactNode }) {
  const [open, setOpen] = useState(false);
  const v = useMemo(() => ({ open, setOpen }), [open]);
  return <Ctx.Provider value={v}>{children}</Ctx.Provider>;
}
export function useAssistant(): AssistantApi {
  const c = useContext(Ctx);
  if (!c) throw new Error("useAssistant outside AssistantProvider");
  return c;
}
