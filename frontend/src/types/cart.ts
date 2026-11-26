export interface CartItem { productId: string; name: string; unit: string; imageUrl?: string | null; unitPrice: number; quantity: number; lineTotal: number }
export interface Cart { items: CartItem[]; subtotal: number; deliveryFee: number; total: number; currency: string }
export interface Address {
  id: string; label: string; recipientName: string; phone: string; line1: string; line2?: string | null;
  city: string; state: string; postalCode: string; isDefault: boolean;
}
export type SaveAddressRequest = Omit<Address, "id">;
