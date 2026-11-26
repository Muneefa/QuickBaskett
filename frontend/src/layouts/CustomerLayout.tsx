import { Outlet } from "react-router-dom";
import { Header } from "@/components/layout/Header";
import { Footer } from "@/components/layout/Footer";
import { BottomNav } from "@/components/layout/BottomNav";
import { AssistantPanel } from "@/components/assistant/AssistantPanel";
import { useAssistant } from "@/contexts/AssistantContext";
import { useCatalog } from "@/hooks/useProducts";

export function CustomerLayout() {
  const { setOpen } = useAssistant();
  const { products } = useCatalog();
  return (
    <div className="min-h-screen pb-16 sm:pb-0">
      <Header />
      <main className="mx-auto max-w-6xl px-4 py-4"><Outlet /></main>
      <Footer />
      <BottomNav />
      <button onClick={() => setOpen(true)} aria-label="Open shopping assistant"
        className="fixed bottom-20 right-4 z-30 rounded-full bg-accent px-4 py-3 text-sm font-extrabold text-white shadow-lg sm:bottom-6">✨ Assistant</button>
      <AssistantPanel products={products} />
    </div>
  );
}
