import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'

import { logout } from '@/features/auth/api/auth'
import { useAuth } from '@/features/auth/context/auth-context'

export function useLogout() {
  const { clearSession } = useAuth()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: logout,
    onMutate: () => {
      // Clear local auth immediately; server revocation remains best-effort.
      clearSession()
      void navigate('/login', { replace: true })
    },
  })
}
