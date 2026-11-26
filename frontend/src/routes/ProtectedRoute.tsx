import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "@/hooks/useAuth";
import { Spinner } from "@/components/ui/Spinner";
import { paths } from "./routePaths";
import type { Role } from "@/types/auth";

const home: Record<Role, string> = { Customer: paths.home, Admin: paths.admin, DeliveryPartner: paths.partner };
const loginFor: Record<Role, string> = { Customer: paths.login, Admin: paths.adminLogin, DeliveryPartner: paths.partnerLogin };

/** Lets a signed-in user of exactly `role` through. Everyone else is sent to their own area or the right sign-in. */
export function ProtectedRoute({ role }: { role: Role }) {
  const { user, loading } = useAuth();
  const loc = useLocation();
  if (loading) return <Spinner label="Checking your session" />;
  if (!user) return <Navigate to={loginFor[role]} state={{ from: loc.pathname }} replace />;
  if (user.role !== role) return <Navigate to={home[user.role]} replace />;
  return <Outlet />;
}
export const homeFor = (r: Role) => home[r];
