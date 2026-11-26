import { useRef, useState, useEffect } from "react";
import { Drawer } from "@/components/ui/Drawer";
import { ChatBubble } from "./ChatBubble";
import { ShoppingListCard } from "./ShoppingListCard";
import { SuggestedPrompts } from "./SuggestedPrompts";
import { useAssistant } from "@/contexts/AssistantContext";
import { useCart } from "@/hooks/useCart";
import { useAuth } from "@/hooks/useAuth";
import { useToast } from "@/contexts/ToastContext";
import { PROMPTS, suggest } from "@/utils/assistantLogic";
import type { Product } from "@/types/catalog";
import type { ChatMessage } from "@/types/assistant";

let id = 0;
const hello: ChatMessage = { id: 0, role: "bot", text: "Hi! Tell me what you're planning and I'll suggest a list. I can only fill your cart — you choose and pay yourself." };

export function AssistantPanel({ products }: { products: Product[] }) {
  const { open, setOpen } = useAssistant();
  const { add } = useCart();
  const { user } = useAuth();
  const toast = useToast();
  const [msgs, setMsgs] = useState<ChatMessage[]>([hello]);
  const [text, setText] = useState("");
  const end = useRef<HTMLDivElement>(null);
  useEffect(() => { end.current?.scrollIntoView({ block: "end" }); }, [msgs, open]);

  const send = (t: string) => {
    if (!t.trim()) return;
    const list = suggest(t, products);
    setMsgs(m => [...m, { id: ++id, role: "me", text: t },
      list ? { id: ++id, role: "bot", text: "Here's a starting list. Untick anything you don't need.", list }
           : { id: ++id, role: "bot", text: "I can plan breakfast, dinner, snacks, healthy picks or home cleaning. Try one of those." }]);
    setText("");
  };
  const toggle = (msgId: number, pid: string) => setMsgs(m => m.map(x => x.id === msgId && x.list
    ? { ...x, list: { ...x.list, items: x.list.items.map(i => i.productId === pid ? { ...i, on: !i.on } : i) } } : x));
  const addAll = async (msgId: number) => {
    if (user?.role !== "Customer") { toast.push("Sign in as a customer to add to your cart", "info"); return; }
    const msg = msgs.find(x => x.id === msgId);
    if (!msg?.list) return;
    for (const i of msg.list.items.filter(i => i.on)) await add(i.productId, i.qty);
    setMsgs(m => m.map(x => x.id === msgId && x.list ? { ...x, list: { ...x.list, added: true } } : x));
    toast.push("List added to your cart");
  };

  return (
    <Drawer open={open} title="Shopping assistant" onClose={() => setOpen(false)}>
      <div className="flex h-full flex-col">
        <div className="flex-1 space-y-2 overflow-auto p-3">
          {msgs.map(m => (
            <ChatBubble key={m.id} me={m.role === "me"}>
              {m.text}
              {m.list && <ShoppingListCard list={m.list} products={products} onToggle={p => toggle(m.id, p)} onAdd={() => void addAll(m.id)} />}
            </ChatBubble>
          ))}
          <div ref={end} />
        </div>
        <SuggestedPrompts prompts={PROMPTS} onPick={send} />
        <form onSubmit={e => { e.preventDefault(); send(text); }} className="flex gap-2 border-t border-line p-3">
          <input value={text} onChange={e => setText(e.target.value)} placeholder="What are you shopping for?" aria-label="Message"
            className="h-11 flex-1 rounded-xl border border-line bg-surface2 px-3 text-sm" />
          <button className="rounded-xl bg-brand px-4 text-sm font-bold text-brand-ink">Send</button>
        </form>
      </div>
    </Drawer>
  );
}
