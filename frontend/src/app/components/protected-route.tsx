import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'

import { Spinner } from '@/components/spinner'
import { useAuth } from '@/features/auth/context/auth-context'

type ProtectedRouteProps = {
  children: ReactNode
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading } = useAuth()

  if (isLoading) {
    return (
      <div className="flex h-dvh items-center justify-center">
        <Spinner />
      </div>
    )
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  return children
}
