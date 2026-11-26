import { useEffect, useState } from "react";
import { AddressForm } from "@/components/checkout/AddressSelector";
import { Button } from "@/components/ui/Button";
import { Spinner } from "@/components/ui/Spinner";
import { addressService } from "@/services/addressService";
import { useToast } from "@/contexts/ToastContext";
import type { Address } from "@/types/cart";

export function AddressesPage() {
  const toast = useToast();
  const [list, setList] = useState<Address[] | null>(null);
  const [adding, setAdding] = useState(false);
  const load = () => addressService.list().then(setList).catch(e => toast.push(e instanceof Error ? e.message : "Could not load", "err"));
  useEffect(() => { void load(); /* eslint-disable-next-line react-hooks/exhaustive-deps */ }, []);
  if (!list) return <Spinner />;
  const remove = async (id: string) => { try { await addressService.remove(id); await load(); } catch (e) { toast.push(e instanceof Error ? e.message : "Could not delete", "err"); } };
  return (
    <div className="max-w-xl space-y-3">
      <h1 className="text-2xl font-extrabold">Saved addresses</h1>
      {list.map(a => (
        <div key={a.id} className="flex items-start justify-between gap-3 rounded-xl border border-line bg-surface p-3 text-sm">
          <div><b>{a.label}</b>{a.isDefault && <span className="ml-2 rounded-full bg-brand-soft px-2 py-0.5 text-xs font-bold text-brand">Default</span>}
            <div className="text-muted">{a.recipientName} · {a.phone}<br />{a.line1}{a.line2 ? `, ${a.line2}` : ""}, {a.city}, {a.state} {a.postalCode}</div></div>
          <button onClick={() => void remove(a.id)} className="text-sm font-bold text-bad">Delete</button>
        </div>
      ))}
      {adding ? <AddressForm onCancel={() => setAdding(false)} onSave={async a => { await addressService.create(a); setAdding(false); await load(); }} />
        : <Button variant="ghost" onClick={() => setAdding(true)}>+ Add address</Button>}
    </div>
  );
}
