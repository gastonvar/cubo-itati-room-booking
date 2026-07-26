import { useMemo } from 'react'
import {
  Calendar,
  dateFnsLocalizer,
  type DateFormat,
  type Formats,
  type View,
} from 'react-big-calendar'
import { format, getDay, parse, startOfWeek } from 'date-fns'
import { enUS } from 'date-fns/locale'

import { ScheduleCalendarToolbar } from '@/features/rooms/components/schedule-calendar-toolbar'
import { getRoomColor } from '@/features/rooms/lib/room-colors'
import type {
  CalendarEvent,
  FreeSlotWithRoom,
  OccupiedSlotWithRoom,
} from '@/features/rooms/types/room'

import 'react-big-calendar/lib/css/react-big-calendar.css'

const localizer = dateFnsLocalizer({
  format,
  parse,
  startOfWeek: (date: Date) => startOfWeek(date, { weekStartsOn: 1 }),
  getDay,
  locales: { 'en-US': enUS },
})

function formatTimeRange({ start, end }: { start: Date; end: Date }) {
  return `${format(start, 'HH:mm')} – ${format(end, 'HH:mm')}`
}

const formats: Formats = {
  timeGutterFormat: 'HH:mm' as DateFormat,
  eventTimeRangeFormat: formatTimeRange,
  eventTimeRangeStartFormat: ({ start }) => format(start, 'HH:mm'),
  eventTimeRangeEndFormat: ({ end }) => format(end, 'HH:mm'),
  selectRangeFormat: formatTimeRange,
  agendaTimeFormat: 'HH:mm' as DateFormat,
  agendaTimeRangeFormat: formatTimeRange,
}

type ScheduleCalendarProps = {
  occupied: OccupiedSlotWithRoom[]
  freeSlots: FreeSlotWithRoom[]
  date: Date
  view: View
  showRoomInEvent?: boolean
  onNavigate: (date: Date) => void
  onView: (view: View) => void
}

function toEvents(
  occupied: OccupiedSlotWithRoom[],
  freeSlots: FreeSlotWithRoom[],
): CalendarEvent[] {
  const occupiedEvents = occupied.map((slot) => ({
    kind: 'occupied' as const,
    title: slot.title?.trim() || 'Untitled',
    owner: slot.owner,
    roomCode: slot.roomCode,
    attendees: slot.attendees ?? 0,
    start: new Date(slot.start),
    end: new Date(slot.end),
  }))

  const freeEvents = freeSlots.map((slot) => ({
    kind: 'free' as const,
    title: 'Available',
    owner: '',
    roomCode: slot.roomCode,
    attendees: 0,
    start: new Date(slot.start),
    end: new Date(slot.end),
  }))

  return [...freeEvents, ...occupiedEvents]
}

function BookingEvent({
  event,
  showRoom,
}: {
  event: CalendarEvent
  showRoom: boolean
}) {
  if (event.kind === 'free') {
    const label = showRoom ? `Room ${event.roomCode} · Available` : 'Available'
    return (
      <div className="flex min-w-0 flex-col gap-0.5">
        <div className="truncate font-medium">{label}</div>
      </div>
    )
  }

  const title = event.title?.trim() || 'Untitled'
  const metaParts = [
    showRoom ? `Room ${event.roomCode}` : null,
    title,
    event.owner || null,
  ].filter(Boolean)
  const attendeesLabel =
    event.attendees === 1 ? '1 attendee' : `${event.attendees} attendees`

  return (
    <div className="flex min-w-0 flex-col gap-0.5">
      <div className="truncate font-semibold">{metaParts.join(' · ')}</div>
      {event.attendees > 0 ? (
        <div className="truncate text-[0.65rem] font-normal opacity-90">
          {attendeesLabel}
        </div>
      ) : null}
    </div>
  )
}

const dayMin = new Date(1970, 0, 1, 8, 0, 0)
const dayMax = new Date(1970, 0, 1, 20, 0, 0)

export function ScheduleCalendar({
  occupied,
  freeSlots,
  date,
  view,
  showRoomInEvent = false,
  onNavigate,
  onView,
}: ScheduleCalendarProps) {
  const events = useMemo(
    () => toEvents(occupied, freeSlots),
    [occupied, freeSlots],
  )

  const components = useMemo(
    () => ({
      toolbar: ScheduleCalendarToolbar,
      event: ({ event }: { event: CalendarEvent }) => (
        <BookingEvent event={event} showRoom={showRoomInEvent} />
      ),
    }),
    [showRoomInEvent],
  )

  return (
    <div className="schedule-calendar flex h-full min-h-0 flex-1 flex-col font-body text-charcoal">
      <Calendar
        localizer={localizer}
        formats={formats}
        culture="en-US"
        events={events}
        date={date}
        view={view}
        views={['month', 'day']}
        defaultView="day"
        titleAccessor="title"
        tooltipAccessor={(event: CalendarEvent) => {
          if (event.kind === 'free') {
            return showRoomInEvent
              ? `Room ${event.roomCode} · Available`
              : 'Available'
          }
          const parts = [
            showRoomInEvent ? `Room ${event.roomCode}` : null,
            event.title,
            event.owner || null,
            event.attendees > 0
              ? event.attendees === 1
                ? '1 attendee'
                : `${event.attendees} attendees`
              : null,
          ].filter(Boolean)
          return parts.join(' · ')
        }}
        onNavigate={onNavigate}
        onView={onView}
        startAccessor="start"
        endAccessor="end"
        popup
        selectable={false}
        min={dayMin}
        max={dayMax}
        scrollToTime={dayMin}
        step={30}
        timeslots={2}
        components={components}
        eventPropGetter={(event: CalendarEvent) => {
          if (event.kind === 'free') {
            return {
              className: 'rbc-event-free',
            }
          }
          const color = getRoomColor(event.roomCode)
          return {
            className: 'rbc-event-booking',
            style: {
              backgroundColor: color.bg,
            },
          }
        }}
      />
    </div>
  )
}
