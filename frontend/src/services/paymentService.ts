import { api, newIdempotencyKey } from "./apiClient";
import type { InitiatePaymentResponse, Payment, PaymentMethod, VerifyPaymentRequest } from "@/types/payment";

export const paymentService = {
  /** The amount is never sent: the server computes it from the order. */
  initiate: (orderId: string, method: PaymentMethod, confirmedByCustomer: boolean, idempotencyKey = newIdempotencyKey()) =>
    api<InitiatePaymentResponse>("/api/payments/initiate", {
      method: "POST", body: { orderId, method, confirmedByCustomer }, headers: { "Idempotency-Key": idempotencyKey },
    }),
  verify: (b: VerifyPaymentRequest) => api<Payment>("/api/payments/verify", { method: "POST", body: b }),
  get: (id: string) => api<Payment>(`/api/payments/${id}`),
  forOrder: (orderId: string) => api<Payment[]>(`/api/payments/order/${orderId}`),
  /** Development only: asks the backend's mock gateway to complete or fail a payment. */
  devSimulate: (paymentId: string, success: boolean) =>
    api<{ gatewayOrderId: string; gatewayPaymentId: string; checkoutSignature: string }>(
      `/api/dev/mock-payments/${paymentId}/simulate?success=${success}`, { method: "POST" }),
};
