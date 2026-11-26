import { useState, type FormEvent } from "react";
import { Button } from "@/components/ui/Button";
import type { Address, SaveAddressRequest } from "@/types/cart";

const blank: SaveAddressRequest = { label: "Home", recipientName: "", phone: "", line1: "", line2: "", city: "", state: "", postalCode: "", isDefault: false };

export function AddressForm({ initial = blank, onSave, onCancel }: { initial?: SaveAddressRequest; onSave: (a: SaveAddressRequest) => Promise<void>; onCancel?: () => void }) {
  const [f, setF] = useState<SaveAddressRequest>(initial);
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState<string | null>(null);
  const set = (k: keyof SaveAddressRequest) => (e: React.ChangeEvent<HTMLInputElement>) => setF({ ...f, [k]: e.target.value });
  const submit = async (e: FormEvent) => {
    e.preventDefault(); setBusy(true); setErr(null);
    try { await onSave({ ...f, line2: f.line2 || null }); } catch (x) { setErr(x instanceof Error ? x.message : "Could not save"); } finally { setBusy(false); }
  };
  const field = "h-11 w-full rounded-xl border border-line bg-surface2 px-3 text-sm";
  return (
    <form onSubmit={submit} className="grid grid-cols-2 gap-3 rounded-2xl border border-line bg-surface p-4">
      <label className="text-sm font-semibold">Label<input className={field} required value={f.label} onChange={set("label")} /></label>
      <label className="text-sm font-semibold">Recipient<input className={field} required value={f.recipientName} onChange={set("recipientName")} /></label>
      <label className="col-span-2 text-sm font-semibold">Phone<input className={field} required type="tel" value={f.phone} onChange={set("phone")} /></label>
      <label className="col-span-2 text-sm font-semibold">Address line 1<input className={field} required value={f.line1} onChange={set("line1")} /></label>
      <label className="col-span-2 text-sm font-semibold">Address line 2<input className={field} value={f.line2 ?? ""} onChange={set("line2")} /></label>
      <label className="text-sm font-semibold">City<input className={field} required value={f.city} onChange={set("city")} /></label>
      <label className="text-sm font-semibold">State<input className={field} required value={f.state} onChange={set("state")} /></label>
      <label className="text-sm font-semibold">PIN code<input className={field} required value={f.postalCode} onChange={set("postalCode")} /></label>
      <label className="flex items-end gap-2 pb-3 text-sm font-semibold"><input type="checkbox" checked={f.isDefault} onChange={e => setF({ ...f, isDefault: e.target.checked })} />Make default</label>
      {err && <p className="col-span-2 text-sm font-semibold text-bad" role="alert">{err}</p>}
      <div className="col-span-2 flex gap-2"><Button disabled={busy}>{busy ? "Saving…" : "Save address"}</Button>{onCancel && <Button type="button" variant="ghost" onClick={onCancel}>Cancel</Button>}</div>
    </form>
  );
}

export function AddressSelector({ addresses, selected, onSelect }: { addresses: Address[]; selected?: string; onSelect: (id: string) => void }) {
  return (
    <fieldset className="space-y-2"><legend className="sr-only">Delivery address</legend>
      {addresses.map(a => (
        <label key={a.id} className={`flex cursor-pointer gap-3 rounded-xl border-2 p-3 text-sm ${selected === a.id ? "border-brand bg-brand-soft" : "border-line bg-surface"}`}>
          <input type="radio" name="address" checked={selected === a.id} onChange={() => onSelect(a.id)} />
          <span><b>{a.label}</b> · {a.recipientName}<br /><span className="text-muted">{a.line1}{a.line2 ? `, ${a.line2}` : ""}, {a.city} {a.postalCode} · {a.phone}</span></span>
        </label>
      ))}
    </fieldset>
  );
}
