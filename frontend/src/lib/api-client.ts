import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'

import { env } from '@/config/env'
import { refreshSession } from '@/features/auth/api/auth'
import { notifyUnauthorized } from '@/features/auth/lib/auth-session'

type RetryConfig = InternalAxiosRequestConfig & { _retry?: boolean }

export const api = axios.create({
  baseURL: env.apiUrl,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Tokens live in httpOnly cookies (sent via withCredentials).
// On 401: refresh once (deduped via refreshInFlight), retry the request.
// If refresh fails or a retried request still 401s → notifyUnauthorized().
let refreshInFlight: Promise<boolean> | null = null

async function tryRefreshSession(): Promise<boolean> {
  try {
    await refreshSession()
    return true
  } catch {
    return false
  }
}

function getOrStartRefresh(): Promise<boolean> {
  if (!refreshInFlight) {
    refreshInFlight = tryRefreshSession().finally(() => {
      refreshInFlight = null
    })
  }
  return refreshInFlight
}

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as RetryConfig | undefined

    if (!axios.isAxiosError(error) || error.response?.status !== 401 || !original) {
      return Promise.reject(error)
    }

    if (original._retry) {
      notifyUnauthorized()
      return Promise.reject(error)
    }

    original._retry = true
    const refreshed = await getOrStartRefresh()
    if (!refreshed) {
      notifyUnauthorized()
      return Promise.reject(error)
    }

    return api(original)
  },
)
