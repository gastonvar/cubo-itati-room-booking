import { useState } from 'react'
import type { View } from 'react-big-calendar'

import { useRooms } from '@/features/rooms/hooks/use-rooms'
import { useSchedules } from '@/features/rooms/hooks/use-schedules'
import {
  rangeForDay,
  rangeForMonthGrid,
} from '@/features/rooms/lib/calendar-date-range'
import { ALL_ROOMS_CODE } from '@/features/rooms/types/room'

export function useSchedulePanel() {
  const [selectedCode, setSelectedCode] = useState(ALL_ROOMS_CODE)
  const [date, setDate] = useState(() => new Date())
  const [view, setView] = useState<View>('day')

  const roomsQuery = useRooms()
  const rooms = roomsQuery.data ?? []
  const isAllRooms = selectedCode === ALL_ROOMS_CODE
  const scheduleCodes = isAllRooms
    ? rooms.map((room) => room.code)
    : selectedCode
      ? [selectedCode]
      : []
  const fetchRange = view === 'month' ? rangeForMonthGrid(date) : rangeForDay(date)
  const scheduleQuery = useSchedules({
    codes: scheduleCodes,
    fromDate: fetchRange.fromDate,
    toDateExclusive: fetchRange.toDateExclusive,
    enabled: scheduleCodes.length > 0,
  })

  // Free ranges overlap across rooms on one timeline; show them for a single room.
  const freeSlots = isAllRooms ? [] : scheduleQuery.freeSlots

  return {
    selectedCode,
    setSelectedCode,
    date,
    view,
    roomsQuery,
    rooms,
    scheduleQuery,
    occupied: scheduleQuery.occupied,
    freeSlots,
    isAllRooms,
    selectionLabel: isAllRooms ? 'all rooms' : `Room ${selectedCode}`,
    handleNavigate: setDate,
    handleView: setView,
  }
}
