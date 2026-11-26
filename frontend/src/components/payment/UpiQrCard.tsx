import { useState } from "react";
import { ExpiryTimer } from "./ExpiryTimer";
import { formatCurrency } from "@/utils/formatCurrency";
import type { UpiQr } from "@/types/payment";

export function UpiQrCard({ qr, expired }: { qr: UpiQr; expired: boolean }) {
  const [broken, setBroken] = useState(false);
  return (
    <div className="mx-auto w-full max-w-xs rounded-2xl border border-line bg-surface p-4 text-center">
      <p className="text-sm text-muted">Pay exactly</p>
      <p className="text-3xl font-extrabold">{formatCurrency(qr.amount)}</p>
      <div className={`relative mx-auto my-3 grid h-56 w-56 place-items-center rounded-xl bg-white p-2 ${expired ? "opacity-30" : ""}`}>
        {broken || !qr.imageUrl
          ? <p className="text-sm text-black">QR image unavailable. Use the “UPI app” method instead.</p>
          : <img src={qr.imageUrl} alt={`UPI QR code for ${formatCurrency(qr.amount)}`} className="h-full w-full" onError={() => setBroken(true)} />}
      </div>
      <ExpiryTimer expiresAtUtc={qr.expiresAtUtc} />
      <p className="mt-2 text-xs text-muted">Made for this order only. Open any UPI app and scan.</p>
    </div>
  );
}
