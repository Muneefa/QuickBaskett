import { BrowserRouter } from "react-router-dom";
import { ToastProvider } from "@/contexts/ToastContext";
import { AuthProvider } from "@/contexts/AuthContext";
import { CartProvider } from "@/contexts/CartContext";
import { AssistantProvider } from "@/contexts/AssistantContext";
import { AppRoutes } from "@/routes";

export default function App() {
  return (
    <BrowserRouter>
      <ToastProvider>
        <AuthProvider>
          <CartProvider>
            <AssistantProvider>
              <AppRoutes />
            </AssistantProvider>
          </CartProvider>
        </AuthProvider>
      </ToastProvider>
    </BrowserRouter>
  );
}
