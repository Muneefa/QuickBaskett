import { Link } from "react-router-dom";
export function NotFoundPage() {
  return <div className="grid min-h-[60vh] place-items-center text-center"><div><h1 className="text-3xl font-extrabold">Page not found</h1><Link to="/" className="mt-3 inline-block font-bold text-brand">Back to QuickBasket</Link></div></div>;
}
