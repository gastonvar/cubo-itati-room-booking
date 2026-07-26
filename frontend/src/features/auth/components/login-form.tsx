import { useForm } from 'react-hook-form'
import axios from 'axios'

import { Button } from '@/components/button'
import { Input } from '@/components/input'
import { useLogin } from '@/features/auth/hooks/use-login'
import type { LoginFormValues } from '@/features/auth/types/auth'
import { getApiErrorMessage } from '@/lib/api-response'

const DEMO_PASSWORD = 'TechnicalChallengePromtior'

const demoUsers = {
  User1: { username: 'User1', password: DEMO_PASSWORD },
  User2: { username: 'User2', password: DEMO_PASSWORD },
} as const

function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error) && error.response?.status === 401) {
    return getApiErrorMessage(error, 'Invalid username or password')
  }
  return getApiErrorMessage(error, 'Unable to sign in. Please try again.')
}

export function LoginForm() {
  const loginMutation = useLogin()
  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<LoginFormValues>({
    defaultValues: demoUsers.User1,
  })

  const fillDemoUser = (user: keyof typeof demoUsers) => {
    const credentials = demoUsers[user]
    setValue('username', credentials.username, { shouldDirty: true, shouldValidate: true })
    setValue('password', credentials.password, { shouldDirty: true, shouldValidate: true })
  }

  const onSubmit = handleSubmit((values) => {
    loginMutation.mutate(values)
  })

  return (
    <form onSubmit={onSubmit} className="space-y-5">
      <div className="space-y-2">
        <p className="text-xs font-medium text-charcoal-soft">Demo accounts</p>
        <div className="grid grid-cols-2 gap-2">
          <Button
            type="button"
            variant="secondary"
            size="sm"
            onClick={() => fillDemoUser('User1')}
          >
            Use User1
          </Button>
          <Button
            type="button"
            variant="secondary"
            size="sm"
            onClick={() => fillDemoUser('User2')}
          >
            Use User2
          </Button>
        </div>
      </div>

      <Input
        label="Username"
        autoComplete="username"
        error={errors.username?.message}
        {...register('username', { required: 'Username is required' })}
      />
      <Input
        label="Password"
        type="password"
        autoComplete="current-password"
        error={errors.password?.message}
        {...register('password', { required: 'Password is required' })}
      />

      {loginMutation.isError ? (
        <p className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {getErrorMessage(loginMutation.error)}
        </p>
      ) : null}

      <Button
        type="submit"
        size="lg"
        className="w-full"
        isLoading={loginMutation.isPending}
      >
        Sign in
      </Button>
    </form>
  )
}
