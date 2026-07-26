import { Navigate } from 'react-router-dom'

import { LoginForm } from '@/features/auth/components/login-form'
import { useAuth } from '@/features/auth/context/auth-context'

export function LoginRoute() {
  const { isAuthenticated, isLoading } = useAuth()

  if (isLoading) {
    return null
  }

  if (isAuthenticated) {
    return <Navigate to="/" replace />
  }

  return (
    <div className="flex h-full items-center justify-center overflow-y-auto px-4 py-10">
      <div className="grid w-full max-w-5xl overflow-hidden rounded-[2rem] border border-stone-200/70 bg-[var(--surface-strong)] shadow-[var(--shadow-soft)] md:grid-cols-[1.05fr_0.95fr]">
        <section className="relative overflow-hidden px-8 py-12 md:px-10 md:py-14">
          <div className="absolute -left-8 -top-10 h-40 w-40 rounded-full bg-teal-soft/50 blur-2xl" />
          <div className="absolute bottom-0 right-0 h-32 w-32 rounded-full bg-stone-fog/70 blur-2xl" />

          <div className="relative">
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-teal-accent">
              Cubo Itatí
            </p>
            <h1 className="mt-4 font-display text-4xl font-bold leading-tight text-charcoal md:text-5xl">
              Room booking,
              <span className="block text-teal-accent">without the friction.</span>
            </h1>
            <p className="mt-5 max-w-md text-base leading-relaxed text-charcoal-soft">
              Reserve meeting rooms through conversation. Check live schedules,
              find availability, and manage bookings in one calm workspace.
            </p>
          </div>
        </section>

        <section className="border-t border-stone-200/60 bg-white/50 px-8 py-10 md:border-l md:border-t-0 md:px-10">
          <div className="mx-auto w-full max-w-sm">
            <h2 className="font-display text-2xl font-semibold text-charcoal">
              Welcome back
            </h2>
            <p className="mt-2 text-sm text-charcoal-soft">
              Sign in to access the booking assistant and room schedules.
            </p>
            <div className="mt-8">
              <LoginForm />
            </div>
          </div>
        </section>
      </div>
    </div>
  )
}
