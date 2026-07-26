import axios from 'axios'

import type { ApiResponse } from '@/types/api'

export function unwrapApiResponse<T>(envelope: ApiResponse<T>): T {
  if (!envelope.success || envelope.data == null) {
    throw new Error(envelope.error?.detail ?? 'Request failed')
  }
  return envelope.data
}

export function getApiErrorMessage(
  error: unknown,
  fallback = 'Something went wrong. Please try again.',
): string {
  if (!axios.isAxiosError(error)) return fallback

  const data = error.response?.data as
    | { error?: { detail?: unknown }; detail?: unknown }
    | undefined

  const nested = data?.error?.detail
  if (typeof nested === 'string' && nested.trim()) return nested

  const legacy = data?.detail
  if (typeof legacy === 'string' && legacy.trim()) return legacy

  return fallback
}
