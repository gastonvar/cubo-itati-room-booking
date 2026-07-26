import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { useNavigate } from 'react-router-dom'

import { getMe, refreshSession } from '@/features/auth/api/auth'
import { bindUnauthorizedHandler } from '@/features/auth/lib/auth-session'
import type {
  AuthContextValue,
  AuthStatus,
} from '@/features/auth/types/auth'

const AuthContext = createContext<AuthContextValue | null>(null)

type AuthProviderProps = {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [username, setUsername] = useState<string | null>(null)
  const [status, setStatus] = useState<AuthStatus>('loading')
  const navigate = useNavigate()

  const setSession = useCallback((nextUsername: string) => {
    setUsername(nextUsername)
    setStatus('authenticated')
  }, [])

  const clearSession = useCallback(() => {
    setUsername(null)
    setStatus('anonymous')
  }, [])

  useEffect(() => {
    return bindUnauthorizedHandler(() => {
      setUsername(null)
      setStatus('anonymous')
      void navigate('/login', { replace: true })
    })
  }, [navigate])

  useEffect(() => {
    let cancelled = false

    async function restoreSession() {
      try {
        const me = await getMe()
        if (!cancelled) setSession(me.username)
        return
      } catch {
        // Access cookie missing/expired — try refresh cookie.
      }

      try {
        const refreshed = await refreshSession()
        if (!cancelled) setSession(refreshed.username)
      } catch {
        if (!cancelled) clearSession()
      }
    }

    void restoreSession()
    return () => {
      cancelled = true
    }
  }, [clearSession, setSession])

  const value = useMemo<AuthContextValue>(
    () => ({
      username,
      isAuthenticated: status === 'authenticated',
      isLoading: status === 'loading',
      setSession,
      clearSession,
    }),
    [username, status, setSession, clearSession],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return ctx
}
