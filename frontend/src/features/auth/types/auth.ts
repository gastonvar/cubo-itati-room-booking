export type LoginRequest = {
  username: string
  password: string
}

export type LoginFormValues = LoginRequest

export type AuthUserResponse = {
  username: string
}

export type AuthStatus = 'loading' | 'authenticated' | 'anonymous'

export type AuthContextValue = {
  username: string | null
  isAuthenticated: boolean
  isLoading: boolean
  setSession: (username: string) => void
  clearSession: () => void
}
