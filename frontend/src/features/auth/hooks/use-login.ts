import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'

import { login } from '@/features/auth/api/auth'
import { useAuth } from '@/features/auth/context/auth-context'
import type { LoginRequest } from '@/features/auth/types/auth'

export function useLogin() {
  const navigate = useNavigate()
  const { setSession } = useAuth()

  return useMutation({
    mutationFn: (payload: LoginRequest) => login(payload),
    onSuccess: (data) => {
      setSession(data.username)
      void navigate('/', { replace: true })
    },
  })
}
