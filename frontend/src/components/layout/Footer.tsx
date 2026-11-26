import { BRAND, TAGLINE } from "@/utils/constants";
export function Footer() {
  return <footer className="mt-10 border-t border-line px-4 py-6 text-center text-sm text-muted"><b className="text-ink">{BRAND}</b> · {TAGLINE}</footer>;
}
