export const formatDateTime = (iso?: string | null): string =>
  iso ? new Date(iso.endsWith("Z") ? iso : iso + "Z").toLocaleString("en-IN", { day: "numeric", month: "short", hour: "numeric", minute: "2-digit" }) : "";

export const formatTime = (iso?: string | null): string =>
  iso ? new Date(iso.endsWith("Z") ? iso : iso + "Z").toLocaleTimeString("en-IN", { hour: "numeric", minute: "2-digit" }) : "";

/** Splits "PendingPayment" into "Pending payment" for display. */
export const humanize = (s: string): string => s.replace(/([a-z])([A-Z])/g, "$1 $2").replace(/^./, c => c.toUpperCase()).replace(/ ([A-Z])/g, (_, c: string) => " " + c.toLowerCase());
