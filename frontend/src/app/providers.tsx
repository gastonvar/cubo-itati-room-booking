import { QueryClientProvider } from '@tanstack/react-query'
import type { ReactNode } from 'react'
import { BrowserRouter } from 'react-router-dom'

import { AuthProvider } from '@/features/auth/context/auth-context'
import { queryClient } from '@/lib/query-client'

type ProvidersProps = {
  children: ReactNode
}

export function Providers({ children }: ProvidersProps) {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter basename={import.meta.env.BASE_URL}>
        <AuthProvider>{children}</AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
