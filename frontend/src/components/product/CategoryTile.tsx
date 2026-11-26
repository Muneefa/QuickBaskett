import { Link } from "react-router-dom";
import { paths } from "@/routes/routePaths";
import { productEmoji } from "@/utils/constants";
import type { Category } from "@/types/catalog";

const ICON: [RegExp, string][] = [[/veg|fruit/i, "🥬"], [/dairy|egg/i, "🥛"], [/bak|bread/i, "🍞"], [/staple|rice|atta/i, "🌾"], [/snack|bever/i, "🧃"], [/clean|home|house/i, "🧴"]];
export function CategoryTile({ category: c }: { category: Category }) {
  const icon = ICON.find(([r]) => r.test(c.name))?.[1] ?? productEmoji(c.name);
  return (
    <Link to={paths.category(c.id)} className="flex flex-col items-center gap-1 rounded-2xl border border-line bg-surface p-3 text-center hover:border-brand">
      <span className="text-3xl">{icon}</span><span className="text-xs font-bold">{c.name}</span>
    </Link>
  );
}
