import { useEffect, useState } from "react";

/** Seconds left until an ISO UTC timestamp. Returns null when no timestamp is given. */
export function useCountdown(expiresAtUtc?: string | null): number | null {
  const target = expiresAtUtc ? new Date(expiresAtUtc.endsWith("Z") ? expiresAtUtc : expiresAtUtc + "Z").getTime() : null;
  const calc = () => (target === null ? null : Math.max(0, Math.floor((target - Date.now()) / 1000)));
  const [left, setLeft] = useState<number | null>(calc);
  useEffect(() => {
    if (target === null) { setLeft(null); return; }
    setLeft(calc());
    const t = setInterval(() => setLeft(calc()), 1000);
    return () => clearInterval(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [target]);
  return left;
}
export const mmss = (s: number): string => `${String(Math.floor(s / 60)).padStart(2, "0")}:${String(s % 60).padStart(2, "0")}`;
