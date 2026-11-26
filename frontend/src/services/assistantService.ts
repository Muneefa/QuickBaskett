// The shopping assistant is scripted and runs in the browser (see utils/assistantLogic.ts).
// There is no assistant API yet. When one is added it may only suggest products and must never call payment endpoints.
export { suggest, PROMPTS } from "@/utils/assistantLogic";
