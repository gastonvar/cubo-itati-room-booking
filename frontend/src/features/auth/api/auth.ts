import { authApi } from '@/features/auth/lib/auth-api'
import { unwrapApiResponse } from '@/lib/api-response'

import type { AuthUserResponse, LoginRequest } from '@/features/auth/types/auth'
import type { ApiResponse } from '@/types/api'

export async function login(payload: LoginRequest): Promise<AuthUserResponse> {
  const { data } = await authApi.post<ApiResponse<AuthUserResponse>>(
    '/auth/login',
    payload,
  )
  return unwrapApiResponse(data)
}

/** Rotates httpOnly cookies; no body — refresh token comes from cookie. */
export async function refreshSession(): Promise<AuthUserResponse> {
  const { data } = await authApi.post<ApiResponse<AuthUserResponse>>('/auth/refresh')
  return unwrapApiResponse(data)
}

/** Best-effort server revoke + cookie clear. */
export async function logout(): Promise<void> {
  await authApi.post('/auth/logout')
}

export async function getMe(): Promise<AuthUserResponse> {
  const { data } = await authApi.get<ApiResponse<AuthUserResponse>>('/auth/me')
  return unwrapApiResponse(data)
}
