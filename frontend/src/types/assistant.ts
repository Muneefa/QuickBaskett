export interface ListItem { productId: string; qty: number; on: boolean }
export interface ShoppingList { title: string; items: ListItem[]; added: boolean }
export interface ChatMessage { id: number; role: "bot" | "me"; text: string; list?: ShoppingList }
