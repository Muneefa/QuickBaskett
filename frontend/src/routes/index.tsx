import { Route, Routes } from "react-router-dom";
import { ProtectedRoute } from "./ProtectedRoute";
import { paths } from "./routePaths";
import { CustomerLayout } from "@/layouts/CustomerLayout";
import { AdminLayout } from "@/layouts/AdminLayout";
import { PartnerLayout } from "@/layouts/PartnerLayout";
import { LoginPage } from "@/pages/auth/LoginPage";
import { RegisterPage } from "@/pages/auth/RegisterPage";
import { HomePage } from "@/pages/customer/HomePage";
import { CategoryPage } from "@/pages/customer/CategoryPage";
import { SearchPage } from "@/pages/customer/SearchPage";
import { ProductDetailPage } from "@/pages/customer/ProductDetailPage";
import { CartPage } from "@/pages/customer/CartPage";
import { CheckoutPage } from "@/pages/customer/CheckoutPage";
import { PaymentPage } from "@/pages/customer/PaymentPage";
import { UpiQrPaymentPage } from "@/pages/customer/UpiQrPaymentPage";
import { OrderConfirmationPage } from "@/pages/customer/OrderConfirmationPage";
import { OrderTrackingPage } from "@/pages/customer/OrderTrackingPage";
import { OrderHistoryPage } from "@/pages/customer/OrderHistoryPage";
import { AddressesPage } from "@/pages/customer/AddressesPage";
import { AdminDashboardPage } from "@/pages/admin/AdminDashboardPage";
import { AdminOrdersPage } from "@/pages/admin/AdminOrdersPage";
import { AdminProductsPage } from "@/pages/admin/AdminProductsPage";
import { AdminPaymentsPage } from "@/pages/admin/AdminPaymentsPage";
import { PartnerDashboardPage } from "@/pages/partner/PartnerDashboardPage";
import { PartnerActiveDeliveryPage } from "@/pages/partner/PartnerActiveDeliveryPage";
import { PartnerHistoryPage } from "@/pages/partner/PartnerHistoryPage";
import { NotFoundPage } from "@/pages/NotFoundPage";

export function AppRoutes() {
  return (
    <Routes>
      {/* Public: browsing and sign-in */}
      <Route element={<CustomerLayout />}>
        <Route path={paths.home} element={<HomePage />} />
        <Route path={paths.category()} element={<CategoryPage />} />
        <Route path={paths.search} element={<SearchPage />} />
        <Route path={paths.product()} element={<ProductDetailPage />} />
        <Route path={paths.login} element={<LoginPage role="Customer" />} />
        <Route path={paths.register} element={<RegisterPage />} />

        {/* Customer only */}
        <Route element={<ProtectedRoute role="Customer" />}>
          <Route path={paths.cart} element={<CartPage />} />
          <Route path={paths.checkout} element={<CheckoutPage />} />
          <Route path={paths.pay()} element={<PaymentPage />} />
          <Route path={paths.payQr()} element={<UpiQrPaymentPage />} />
          <Route path={paths.orders} element={<OrderHistoryPage />} />
          <Route path={paths.order()} element={<OrderTrackingPage />} />
          <Route path={paths.orderDone()} element={<OrderConfirmationPage />} />
          <Route path={paths.addresses} element={<AddressesPage />} />
        </Route>
      </Route>

      {/* Admin only, with its own sign-in */}
      <Route path={paths.adminLogin} element={<LoginPage role="Admin" />} />
      <Route element={<ProtectedRoute role="Admin" />}>
        <Route element={<AdminLayout />}>
          <Route path={paths.admin} element={<AdminDashboardPage />} />
          <Route path={paths.adminOrders} element={<AdminOrdersPage />} />
          <Route path={paths.adminProducts} element={<AdminProductsPage />} />
          <Route path={paths.adminPayments} element={<AdminPaymentsPage />} />
        </Route>
      </Route>

      {/* Delivery partner only, with its own sign-in */}
      <Route path={paths.partnerLogin} element={<LoginPage role="DeliveryPartner" />} />
      <Route element={<ProtectedRoute role="DeliveryPartner" />}>
        <Route element={<PartnerLayout />}>
          <Route path={paths.partner} element={<PartnerDashboardPage />} />
          <Route path={paths.partnerActive} element={<PartnerActiveDeliveryPage />} />
          <Route path={paths.partnerHistory} element={<PartnerHistoryPage />} />
        </Route>
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
