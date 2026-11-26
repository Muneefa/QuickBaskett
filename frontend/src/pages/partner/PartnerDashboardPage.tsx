import { StatCard } from "@/components/admin/StatCard";
import { Spinner, ErrorBox } from "@/components/ui/Spinner";
import { usePartnerOrders } from "@/hooks/usePartnerOrders";

export function PartnerDashboardPage() {
  const { orders, error } = usePartnerOrders();
  if (error) return <ErrorBox message={error} />;
  if (!orders) return <Spinner />;
  const today = new Date().toDateString();
  return (
    <>
      <h1 className="mb-3 text-2xl font-extrabold">Today</h1>
      <div className="grid grid-cols-2 gap-3">
        <StatCard label="To pick up" value={orders.filter(o => o.status === "Packed").length} />
        <StatCard label="Out for delivery" value={orders.filter(o => o.status === "OutForDelivery").length} />
        <StatCard label="Delivered today" value={orders.filter(o => o.status === "Delivered" && o.deliveredAtUtc && new Date(o.deliveredAtUtc + "Z").toDateString() === today).length} />
        <StatCard label="All time" value={orders.filter(o => o.status === "Delivered").length} />
      </div>
    </>
  );
}
