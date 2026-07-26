import type { ReactNode } from 'react'

import { Button } from '@/components/button'
import { useAuth } from '@/features/auth/context/auth-context'
import { useLogout } from '@/features/auth/hooks/use-logout'

type AppShellProps = {
  children: ReactNode
  scheduleToggle?: ReactNode
}

export function AppShell({ children, scheduleToggle }: AppShellProps) {
  const { username } = useAuth()
  const logoutMutation = useLogout()

  return (
    <div className="flex h-dvh flex-col overflow-hidden">
      <header className="shrink-0 border-b border-stone-200/60 bg-white/70 backdrop-blur-md">
        <div className="mx-auto flex w-full max-w-[94rem] items-center justify-between gap-4 px-4 py-3 md:px-6">
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-teal-accent text-sm font-display font-bold text-white">
              CI
            </div>
            <div>
              <p className="font-display text-base font-semibold leading-tight text-charcoal">
                Cubo Itatí
              </p>
              <p className="text-xs text-charcoal-soft">Room booking</p>
            </div>
          </div>

          <div className="flex items-center gap-2 md:gap-3">
            {scheduleToggle}
            {username ? (
              <span className="hidden text-sm text-charcoal-soft sm:inline">
                {username}
              </span>
            ) : null}
            <Button
              variant="ghost"
              size="sm"
              disabled={logoutMutation.isPending}
              onClick={() => logoutMutation.mutate()}
            >
              Log out
            </Button>
          </div>
        </div>
      </header>

      <main className="mx-auto flex min-h-0 w-full max-w-[94rem] flex-1 flex-col overflow-hidden px-4 py-4 md:px-6 md:py-6">
        {children}
      </main>
    </div>
  )
}
