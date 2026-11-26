import type { DeliveryPartner } from "@/types/order";
export function PartnerCard({ partner }: { partner?: DeliveryPartner | null }) {
  if (!partner) return null;
  return (
    <div className="flex items-center gap-3 rounded-xl border border-line bg-surface p-3">
      <span className="grid h-10 w-10 place-items-center rounded-full bg-brand-soft text-xl">🛵</span>
      <div className="flex-1 text-sm"><b>{partner.fullName}</b><div className="text-muted">Your delivery partner</div></div>
      {partner.phoneNumber && <a className="rounded-lg bg-surface2 px-3 py-2 text-sm font-bold" href={`tel:${partner.phoneNumber}`}>Call</a>}
    </div>
  );
}
