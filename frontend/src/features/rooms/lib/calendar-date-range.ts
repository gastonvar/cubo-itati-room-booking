import {
  addDays,
  endOfMonth,
  endOfWeek,
  format,
  startOfMonth,
  startOfWeek,
} from 'date-fns'

import type { CalendarDateRange } from '@/features/rooms/types/room'

function toCalendarDate(date: Date): string {
  return format(date, 'yyyy-MM-dd')
}

export function rangeForDay(date: Date): CalendarDateRange {
  return {
    fromDate: toCalendarDate(date),
    toDateExclusive: toCalendarDate(addDays(date, 1)),
  }
}

export function rangeForMonthGrid(date: Date): CalendarDateRange {
  const firstVisibleDay = startOfWeek(startOfMonth(date), { weekStartsOn: 1 })
  const lastVisibleDay = endOfWeek(endOfMonth(date), { weekStartsOn: 1 })

  return {
    fromDate: toCalendarDate(firstVisibleDay),
    toDateExclusive: toCalendarDate(addDays(lastVisibleDay, 1)),
  }
}
