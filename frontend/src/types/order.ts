export type OrderStatus = "PendingPayment" | "Confirmed" | "Packed" | "OutForDelivery" | "Delivered" | "Cancelled";
export interface OrderItem { productId: string; productName: string; unit: string; unitPrice: number; quantity: number; lineTotal: number }
export interface OrderEvent { status: OrderStatus; note?: string | null; occurredAtUtc: string }
export interface DeliveryPartner { id: string; fullName: string; phoneNumber?: string | null }
export interface Order {
  id: string; orderNumber: string; status: OrderStatus; subtotal: number; deliveryFee: number; totalAmount: number; currency: string;
  deliveryRecipient: string; deliveryPhone: string; deliveryAddress: string; notes?: string | null;
  deliveryPartner?: DeliveryPartner | null; createdAtUtc: string; deliveredAtUtc?: string | null;
  latestPaymentStatus?: string | null; items: OrderItem[]; events: OrderEvent[];
}
