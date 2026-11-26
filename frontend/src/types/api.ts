export interface PagedResult<T> { items: T[]; page: number; pageSize: number; totalCount: number }
export interface ProblemDetails { status?: number; title?: string; detail?: string | null }
