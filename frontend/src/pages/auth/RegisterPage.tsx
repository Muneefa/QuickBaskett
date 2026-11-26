import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { AuthLayout } from "@/layouts/AuthLayout";
import { Button } from "@/components/ui/Button";
import { useAuth } from "@/hooks/useAuth";
import { paths } from "@/routes/routePaths";

export function RegisterPage() {
  const { register } = useAuth();
  const nav = useNavigate();
  const [f, setF] = useState({ fullName: "", email: "", phoneNumber: "", password: "" });
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState<string | null>(null);
  const set = (k: keyof typeof f) => (e: React.ChangeEvent<HTMLInputElement>) => setF({ ...f, [k]: e.target.value });

  const submit = async (e: FormEvent) => {
    e.preventDefault(); setBusy(true); setErr(null);
    try { await register({ ...f, phoneNumber: f.phoneNumber || undefined }); nav(paths.home, { replace: true }); }
    catch (x) { setErr(x instanceof Error ? x.message : "Could not create account"); }
    finally { setBusy(false); }
  };
  const field = "h-11 w-full rounded-xl border border-line bg-surface2 px-3 text-sm";
  return (
    <AuthLayout title="Create your account" subtitle="Customer accounts only.">
      <form onSubmit={submit} className="space-y-3">
        <label className="block text-sm font-semibold">Full name<input className={field} required value={f.fullName} onChange={set("fullName")} autoComplete="name" /></label>
        <label className="block text-sm font-semibold">Email<input className={field} type="email" required value={f.email} onChange={set("email")} autoComplete="email" /></label>
        <label className="block text-sm font-semibold">Phone (optional)<input className={field} type="tel" value={f.phoneNumber} onChange={set("phoneNumber")} autoComplete="tel" /></label>
        <label className="block text-sm font-semibold">Password<input className={field} type="password" required minLength={8} value={f.password} onChange={set("password")} autoComplete="new-password" /></label>
        {err && <p className="rounded-lg bg-bad-bg p-2 text-sm font-semibold text-bad" role="alert">{err}</p>}
        <Button className="w-full" disabled={busy}>{busy ? "Creating…" : "Create account"}</Button>
      </form>
      <p className="mt-4 text-center text-sm text-muted">Already have an account? <Link className="font-bold text-brand" to={paths.login}>Sign in</Link></p>
    </AuthLayout>
  );
}
