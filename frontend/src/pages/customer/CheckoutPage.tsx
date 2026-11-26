import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { AddressForm, AddressSelector } from "@/components/checkout/AddressSelector";
import { DeliverySlotPicker, SLOTS } from "@/components/checkout/DeliverySlotPicker";
import { PaymentMethodSelector } from "@/components/checkout/PaymentMethodSelector";
import { ConfirmPaymentDialog } from "@/components/checkout/ConfirmPaymentDialog";
import { BillSummary } from "@/components/cart/BillSummary";
import { Button } from "@/components/ui/Button";
import { Spinner } from "@/components/ui/Spinner";
import { addressService } from "@/services/addressService";
import { orderService } from "@/services/orderService";
import { paymentService } from "@/services/paymentService";
import { useCart } from "@/hooks/useCart";
import { useToast } from "@/contexts/ToastContext";
import { saveInitiated } from "@/utils/paymentSession";
import { paths } from "@/routes/routePaths";
import type { Address } from "@/types/cart";
import type { PaymentMethod } from "@/types/payment";

export function CheckoutPage() {
  const nav = useNavigate();
  const toast = useToast();
  const { cart, refresh } = useCart();
  const [addresses, setAddresses] = useState<Address[] | null>(null);
  const [addressId, setAddressId] = useState<string>();
  const [adding, setAdding] = useState(false);
  const [slot, setSlot] = useState(SLOTS[0]);
  const [method, setMethod] = useState<PaymentMethod>("UpiQr");
  const [confirm, setConfirm] = useState(false);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    addressService.list().then(l => { setAddresses(l); setAddressId((l.find(a => a.isDefault) ?? l[0])?.id); setAdding(l.length === 0); })
      .catch(e => { toast.push(e instanceof Error ? e.message : "Could not load addresses", "err"); setAddresses([]); });
  }, [toast]);

  if (!addresses) return <Spinner />;
  if (!cart.items.length) return <p className="p-10 text-center">Your basket is empty. <Link className="font-bold text-brand" to={paths.home}>Go shopping</Link></p>;

  const place = async () => {
    if (!addressId) return;
    setBusy(true);
    try {
      const order = await orderService.checkout(addressId, `Slot: ${slot}`);
      const pay = await paymentService.initiate(order.id, method, true);
      saveInitiated(pay);
      void refresh();
      nav(method === "CashOnDelivery" ? paths.orderDone(order.id) : method === "UpiQr" ? paths.payQr(order.id) : paths.pay(order.id), { replace: true });
    } catch (e) {
      toast.push(e instanceof Error ? e.message : "Could not place the order", "err");
      setConfirm(false);
    } finally { setBusy(false); }
  };

  return (
    <div className="grid gap-6 md:grid-cols-[1fr_340px]">
      <div className="space-y-6">
        <section><h2 className="mb-2 text-xl font-extrabold">1. Delivery address</h2>
          <AddressSelector addresses={addresses} selected={addressId} onSelect={setAddressId} />
          {adding ? <div className="mt-3"><AddressForm onCancel={addresses.length ? () => setAdding(false) : undefined}
            onSave={async a => { const n = await addressService.create(a); setAddresses([...addresses, n]); setAddressId(n.id); setAdding(false); }} /></div>
            : <button onClick={() => setAdding(true)} className="mt-2 text-sm font-bold text-brand">+ Add a new address</button>}
        </section>
        <section><h2 className="mb-2 text-xl font-extrabold">2. Delivery slot</h2><DeliverySlotPicker value={slot} onChange={setSlot} /></section>
        <section><h2 className="mb-2 text-xl font-extrabold">3. Payment</h2><PaymentMethodSelector value={method} onChange={setMethod} /></section>
      </div>
      <aside className="space-y-3 md:sticky md:top-24 md:self-start">
        <BillSummary cart={cart} />
        <Button className="w-full" disabled={!addressId} onClick={() => setConfirm(true)}>{method === "CashOnDelivery" ? "Place order" : "Review & pay"}</Button>
      </aside>
      <ConfirmPaymentDialog open={confirm} total={cart.total} method={method} busy={busy} onCancel={() => setConfirm(false)} onConfirm={() => void place()} />
    </div>
  );
}
