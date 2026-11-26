/** Public settings only. Anything VITE_* is visible in the browser, so never put secrets here. */
export const API_BASE_URL: string = (import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7080").replace(/\/$/, "");
export const IS_DEV: boolean = import.meta.env.DEV;
