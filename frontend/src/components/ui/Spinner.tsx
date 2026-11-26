export function Spinner({ label = "Loading" }: { label?: string }) {
  return (
    <div className="flex items-center justify-center gap-3 p-10 text-muted" role="status">
      <span className="h-5 w-5 animate-spin rounded-full border-2 border-line border-t-brand" />
      <span className="text-sm font-semibold">{label}…</span>
    </div>
  );
}
export function ErrorBox({ message, onRetry }: { message: string; onRetry?: () => void }) {
  return (
    <div className="m-4 rounded-xl border border-line bg-bad-bg p-4 text-sm text-bad" role="alert">
      <p className="font-semibold">{message}</p>
      {onRetry && <button onClick={onRetry} className="mt-2 rounded-lg bg-surface px-3 py-1.5 font-bold text-ink">Try again</button>}
    </div>
  );
}
