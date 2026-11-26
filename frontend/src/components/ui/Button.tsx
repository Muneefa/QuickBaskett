import type { ButtonHTMLAttributes } from "react";

type Variant = "primary" | "accent" | "ghost" | "danger";
const styles: Record<Variant, string> = {
  primary: "bg-brand text-brand-ink hover:opacity-90",
  accent: "bg-accent text-white hover:opacity-90",
  ghost: "bg-surface2 text-ink hover:bg-line",
  danger: "bg-bad-bg text-bad hover:opacity-90",
};
export function Button({ variant = "primary", className = "", ...p }: ButtonHTMLAttributes<HTMLButtonElement> & { variant?: Variant }) {
  return (
    <button
      {...p}
      className={`inline-flex min-h-11 items-center justify-center gap-2 rounded-xl px-4 py-2 text-sm font-bold transition disabled:cursor-not-allowed disabled:opacity-50 ${styles[variant]} ${className}`}
    />
  );
}
