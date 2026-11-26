import { useState, type FormEvent } from "react";
import { Link, Navigate, useLocation, useNavigate } from "react-router-dom";
import { AuthLayout } from "@/layouts/AuthLayout";
import { Button } from "@/components/ui/Button";
import { useAuth } from "@/hooks/useAuth";
import { homeFor } from "@/routes/ProtectedRoute";
import { paths } from "@/routes/routePaths";
import type { Role } from "@/types/auth";

const COPY: Record<Role, { title: string; sub: string }> = {
  Customer: { title: "Welcome back", sub: "Sign in to order groceries." },
  Admin: { title: "Admin sign-in", sub: "Staff only. Customer accounts can't sign in here." },
  DeliveryPartner: { title: "Partner sign-in", sub: "Delivery partners only." },
};

/** One screen for all three areas. Each route passes the role it accepts, and the page refuses every other role. */
export function LoginPage({ role }: { role: Role }) {
  const { user, login } = useAuth();
  const nav = useNavigate();
  const loc = useLocation();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  if (user && user.role === role) return <Navigate to={homeFor(role)} replace />;

  const submit = async (e: FormEvent) => {
    e.preventDefault(); setBusy(true); setErr(null);
    try {
      const u = await login(email.trim(), password, role);
      const from = (loc.state as { from?: string } | null)?.from;
      nav(role === "Customer" && from ? from : homeFor(u.role), { replace: true });
    } catch (x) { setErr(x instanceof Error ? x.message : "Sign-in failed"); }
    finally { setBusy(false); }
  };

  const field = "h-11 w-full rounded-xl border border-line bg-surface2 px-3 text-sm";
  return (
    <AuthLayout title={COPY[role].title} subtitle={COPY[role].sub}>
      <form onSubmit={submit} className="space-y-3">
        <label className="block text-sm font-semibold">Email<input className={field} type="email" required autoComplete="username" value={email} onChange={e => setEmail(e.target.value)} /></label>
        <label className="block text-sm font-semibold">Password<input className={field} type="password" required autoComplete="current-password" value={password} onChange={e => setPassword(e.target.value)} /></label>
        {err && <p className="rounded-lg bg-bad-bg p-2 text-sm font-semibold text-bad" role="alert">{err}</p>}
        <Button className="w-full" disabled={busy}>{busy ? "Signing in…" : "Sign in"}</Button>
      </form>
      {role === "Customer" && <p className="mt-4 text-center text-sm text-muted">New here? <Link className="font-bold text-brand" to={paths.register}>Create an account</Link></p>}
    </AuthLayout>
  );
}
