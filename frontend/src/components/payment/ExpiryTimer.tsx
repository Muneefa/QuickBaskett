import { mmss, useCountdown } from "@/hooks/useCountdown";
export function ExpiryTimer({ expiresAtUtc }: { expiresAtUtc?: string | null }) {
  const left = useCountdown(expiresAtUtc);
  if (left === null) return null;
  return <p className={`text-sm font-bold ${left < 60 ? "text-bad" : "text-muted"}`} aria-live="off">{left > 0 ? `Expires in ${mmss(left)}` : "This payment has expired"}</p>;
}
