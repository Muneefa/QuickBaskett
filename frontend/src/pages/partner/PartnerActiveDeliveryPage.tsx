import { DeliveryTaskCard } from "@/components/partner/DeliveryTaskCard";
import { Spinner, ErrorBox } from "@/components/ui/Spinner";
import { usePartnerOrders } from "@/hooks/usePartnerOrders";
import { partnerService } from "@/services/partnerService";
import { useToast } from "@/contexts/ToastContext";

export function PartnerActiveDeliveryPage() {
  const toast = useToast();
  const { orders, error, reload } = usePartnerOrders();
  if (error) return <ErrorBox message={error} />;
  if (!orders) return <Spinner />;
  const active = orders.filter(o => o.status === "Packed" || o.status === "OutForDelivery");
  const run = async (fn: () => Promise<unknown>, ok: string) => { try { await fn(); toast.push(ok); await reload(); } catch (e) { toast.push(e instanceof Error ? e.message : "Failed", "err"); } };
  return (
    <>
      <h1 className="mb-3 text-2xl font-extrabold">Active deliveries</h1>
      {!active.length ? <p className="text-muted">Nothing assigned right now.</p> : (
        <div className="grid gap-3 md:grid-cols-2">{active.map(o => (
          <DeliveryTaskCard key={o.id} order={o} onPickup={() => void run(() => partnerService.pickup(o.id), "Delivery started")} onDeliver={c => void run(() => partnerService.deliver(o.id, c), "Marked delivered")} />))}</div>)}
    </>
  );
}
