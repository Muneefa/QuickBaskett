export type PaymentMethod = "Upi" | "UpiQr" | "Card" | "NetBanking" | "CashOnDelivery";
export type PaymentStatus = "Created" | "Pending" | "Authorized" | "Captured" | "Failed" | "Expired" | "Cancelled" | "PartiallyRefunded" | "Refunded";

export interface UpiQr { imageUrl: string; expiresAtUtc: string; amount: number; currency: string }
export interface InitiatePaymentResponse {
  paymentId: string; orderId: string; provider: string; publicKeyId?: string | null; gatewayOrderId?: string | null;
  amount: number; amountInPaise: number; currency: string; method: PaymentMethod; status: PaymentStatus;
  expiresAtUtc?: string | null; upiQr?: UpiQr | null;
}
export interface PaymentTransaction { type: string; statusAfter: string; amount: number; source: string; message?: string | null; occurredAtUtc: string }
export interface Refund {
  id: string; paymentId: string; orderId: string; amount: number; currency: string; status: string;
  gatewayRefundId?: string | null; reason?: string | null; createdAtUtc: string; processedAtUtc?: string | null;
}
export interface Payment {
  id: string; orderId: string; provider: string; method: PaymentMethod; status: PaymentStatus; amount: number; currency: string;
  gatewayOrderId?: string | null; gatewayPaymentId?: string | null; expiresAtUtc?: string | null; paidAtUtc?: string | null;
  refundedAmount: number; failureReason?: string | null; createdAtUtc: string; transactions: PaymentTransaction[]; refunds: Refund[];
}
export interface VerifyPaymentRequest { paymentId: string; gatewayOrderId: string; gatewayPaymentId: string; signature: string }
