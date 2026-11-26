import type { ReactNode } from "react";
import { Logo } from "@/components/layout/Logo";
export function AuthLayout({ title, subtitle, children }: { title: string; subtitle?: string; children: ReactNode }) {
  return (
    <div className="mx-auto grid min-h-[70vh] max-w-md place-items-center">
      <div className="w-full rounded-2xl border border-line bg-surface p-6">
        <Logo /><h1 className="mt-4 text-2xl font-extrabold">{title}</h1>
        {subtitle && <p className="mb-4 text-sm text-muted">{subtitle}</p>}
        {children}
      </div>
    </div>
  );
}
