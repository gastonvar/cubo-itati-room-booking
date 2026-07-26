import { Spinner } from '@/components/spinner'
import { ScheduleCalendar } from '@/features/rooms/components/schedule-calendar'
import { RoomList } from '@/features/rooms/components/room-list'
import { useSchedulePanel } from '@/features/rooms/hooks/use-schedule-panel'
import { getRoomColor } from '@/features/rooms/lib/room-colors'

type SchedulePanelProps = {
  className?: string
  onClose?: () => void
  showClose?: boolean
}

export function SchedulePanel({
  className = '',
  onClose,
  showClose = false,
}: SchedulePanelProps) {
  const {
    selectedCode,
    setSelectedCode,
    date,
    view,
    roomsQuery,
    rooms,
    scheduleQuery,
    occupied,
    freeSlots,
    isAllRooms,
    selectionLabel,
    handleNavigate,
    handleView,
  } = useSchedulePanel()

  let bookingSuffix = ''
  if (!scheduleQuery.isLoading) {
    const parts: string[] = []
    if (occupied.length === 0) {
      parts.push('no bookings in this range')
    } else {
      const plural = occupied.length === 1 ? '' : 's'
      parts.push(`${occupied.length} booking${plural}`)
    }
    if (!isAllRooms) {
      if (freeSlots.length === 0) {
        parts.push('no free slots')
      } else {
        const plural = freeSlots.length === 1 ? '' : 's'
        parts.push(`${freeSlots.length} free slot${plural}`)
      }
    }
    bookingSuffix = ` · ${parts.join(' · ')}`
  }

  let calendarBody
  if (roomsQuery.isLoading) {
    calendarBody = (
      <div className="flex flex-1 items-center justify-center py-12">
        <Spinner />
      </div>
    )
  } else if (roomsQuery.isError) {
    calendarBody = (
      <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-6 text-sm text-red-700">
        Unable to load rooms. Check your connection and try again.
      </div>
    )
  } else if (rooms.length === 0) {
    calendarBody = (
      <div className="rounded-2xl border border-stone-200 bg-white/70 px-4 py-6 text-sm text-charcoal-soft">
        No rooms are configured yet.
      </div>
    )
  } else if (scheduleQuery.isError) {
    calendarBody = (
      <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-6 text-sm text-red-700">
        Unable to load schedule for {selectionLabel}.
      </div>
    )
  } else {
    calendarBody = (
      <div className="relative flex min-h-0 flex-1 flex-col">
        {scheduleQuery.isFetching ? (
          <div className="absolute right-2 top-2 z-10">
            <Spinner />
          </div>
        ) : null}
        <ScheduleCalendar
          occupied={occupied}
          freeSlots={freeSlots}
          date={date}
          view={view}
          showRoomInEvent={isAllRooms}
          onNavigate={handleNavigate}
          onView={handleView}
        />
      </div>
    )
  }

  return (
    <aside
      className={`flex h-full min-h-0 w-full flex-col border-stone-200/70 bg-[var(--surface)] ${className}`}
    >
      <header className="flex items-start justify-between gap-3 border-b border-stone-200/60 px-4 py-4 md:px-5">
        <div>
          <h2 className="font-display text-base font-semibold text-charcoal">
            Room schedule
          </h2>
          <p className="mt-1 text-xs text-charcoal-soft">
            Occupied and available slots · pick a room for free ranges
          </p>
        </div>
        {showClose && onClose ? (
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg p-2 text-charcoal-soft transition-colors hover:bg-stone-200/60 hover:text-charcoal"
            aria-label="Close schedule"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="1.5"
              className="h-5 w-5"
            >
              <path strokeLinecap="round" d="M6 6l12 12M18 6L6 18" />
            </svg>
          </button>
        ) : null}
      </header>

      <div className="border-b border-stone-200/60 px-4 py-4 md:px-5">
        <p className="mb-1.5 text-xs font-medium uppercase tracking-wide text-charcoal-soft">
          Rooms
        </p>
        <RoomList
          rooms={rooms}
          selectedCode={selectedCode}
          onSelect={setSelectedCode}
          isLoading={roomsQuery.isLoading}
        />
        {isAllRooms && rooms.length > 0 ? (
          <div className="mt-3 flex flex-wrap gap-x-3 gap-y-1.5">
            {rooms.map((room) => {
              const color = getRoomColor(room.code)
              return (
                <span
                  key={room.code}
                  className="inline-flex items-center gap-1.5 text-[11px] text-charcoal-soft"
                >
                  <span
                    className="h-2 w-2 shrink-0 rounded-full"
                    style={{ backgroundColor: color.bg }}
                    aria-hidden
                  />
                  Room {room.code}
                </span>
              )
            })}
          </div>
        ) : null}
        <p className="mt-2 text-xs text-charcoal-soft">
          Showing schedule for{' '}
          <span className="font-medium text-charcoal">{selectionLabel}</span>
          {bookingSuffix}
        </p>
      </div>

      <div className="flex min-h-0 flex-1 flex-col px-4 py-4 md:px-5">
        {calendarBody}
      </div>
    </aside>
  )
}
