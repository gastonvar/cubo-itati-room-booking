export type ApiError = {
  detail: string
  code?: string | null
}

export type ApiResponse<T> = {
  success: boolean
  data: T | null
  error: ApiError | null
}
