import { useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'

import { AppShell } from '@/app/components/app-shell'
import { Button } from '@/components/button'
import { ChatPanel } from '@/features/chat/components/chat-panel'
import { SchedulePanel } from '@/features/rooms/components/schedule-panel'
import { invalidateRoomQueries } from '@/features/rooms/lib/invalidate-room-queries'

export function WorkspaceRoute() {
  const [scheduleOpen, setScheduleOpen] = useState(false)
  const queryClient = useQueryClient()

  const scheduleToggle = (
    <Button
      variant="secondary"
      size="sm"
      className="lg:hidden"
      onClick={() => setScheduleOpen(true)}
    >
      Schedule
    </Button>
  )

  const panelShellClass = scheduleOpen
    ? 'drawer-enter fixed inset-y-0 right-0 z-50 w-[min(100%,22rem)] overflow-hidden rounded-l-3xl border-l border-stone-200/70 shadow-2xl lg:relative lg:inset-auto lg:z-auto lg:flex lg:w-[min(100%,22rem)] lg:shrink-0 lg:rounded-3xl lg:border lg:border-stone-200/70 lg:shadow-[var(--shadow-soft)]'
    : 'hidden lg:relative lg:flex lg:w-[min(100%,22rem)] lg:shrink-0 lg:overflow-hidden lg:rounded-3xl lg:border lg:border-stone-200/70 lg:shadow-[var(--shadow-soft)]'

  return (
    <AppShell scheduleToggle={scheduleToggle}>
      <div className="flex min-h-0 flex-1 gap-4 overflow-hidden md:gap-6">
        <div className="flex min-h-0 min-w-0 flex-1 flex-col overflow-hidden">
          <ChatPanel onResponseSuccess={() => invalidateRoomQueries(queryClient)} />
        </div>

        <div className={panelShellClass}>
          <SchedulePanel
            className={scheduleOpen ? 'h-full rounded-l-3xl lg:rounded-3xl' : 'rounded-3xl'}
            showClose={scheduleOpen}
            onClose={() => setScheduleOpen(false)}
          />
        </div>
      </div>

      {scheduleOpen ? (
        <button
          type="button"
          aria-label="Close schedule overlay"
          className="overlay-enter fixed inset-0 z-40 bg-charcoal/30 backdrop-blur-[2px] lg:hidden"
          onClick={() => setScheduleOpen(false)}
        />
      ) : null}
    </AppShell>
  )
}
