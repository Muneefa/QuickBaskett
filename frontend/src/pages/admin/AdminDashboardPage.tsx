import { useEffect, useState } from "react";
import { StatCard } from "@/components/admin/StatCard";
import { Spinner, ErrorBox } from "@/components/ui/Spinner";
import { adminService } from "@/services/adminService";
import { catalogService } from "@/services/catalogService";
import { formatCurrency } from "@/utils/formatCurrency";
import type { DashboardStats } from "@/types/admin";

/** Numbers are worked out in the browser from the first page of orders; a server-side stats endpoint is on the backlog. */
export function AdminDashboardPage() {
  const [s, setS] = useState<DashboardStats | null>(null);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => {
    Promise.all([adminService.orders(undefined, 1, 100), catalogService.products({ pageSize: 100 })]).then(([o, p]) => {
      const live = o.items.filter(x => x.status !== "Cancelled");
      setS({
        ordersLoaded: o.items.length, totalOrders: o.totalCount,
        revenue: live.filter(x => x.latestPaymentStatus === "Captured" || x.status === "Delivered").reduce((n, x) => n + x.totalAmount, 0),
        active: o.items.filter(x => ["Confirmed", "Packed", "OutForDelivery"].includes(x.status)).length,
        pendingPayment: o.items.filter(x => x.status === "PendingPayment").length,
        lowStock: p.items.filter(x => x.stockQuantity <= 10).length,
      });
    }).catch(e => setError(e instanceof Error ? e.message : "Could not load"));
  }, []);
  if (error) return <ErrorBox message={error} />;
  if (!s) return <Spinner />;
  return (
    <>
      <h1 className="mb-3 text-2xl font-extrabold">Overview</h1>
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        <StatCard label="Orders" value={s.totalOrders} hint={s.totalOrders > s.ordersLoaded ? `latest ${s.ordersLoaded} used below` : undefined} />
        <StatCard label="Paid revenue" value={formatCurrency(s.revenue)} hint="from loaded orders" />
        <StatCard label="In progress" value={s.active} />
        <StatCard label="Awaiting payment" value={s.pendingPayment} />
        <StatCard label="Low stock items" value={s.lowStock} hint="10 or fewer left" />
      </div>
    </>
  );
}
