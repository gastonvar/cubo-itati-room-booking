export const ALL_ROOMS_CODE = 'all'

export type Room = {
  code: string
  capacity: number
}

export type OccupiedSlot = {
  start: string
  end: string
  title: string
  owner: string
  attendees: number
}

export type OccupiedSlotWithRoom = OccupiedSlot & {
  roomCode: string
}

export type FreeSlot = {
  start: string
  end: string
}

export type FreeSlotWithRoom = FreeSlot & {
  roomCode: string
}

export type RoomSchedule = {
  roomCode: string
  occupied: OccupiedSlot[]
  freeSlots: FreeSlot[]
}

export type RoomScheduleParams = {
  code: string
  fromDate: string
  toDateExclusive: string
}

export type UseSchedulesParams = {
  codes: string[]
  fromDate: string
  toDateExclusive: string
  enabled?: boolean
}

export type CalendarDateRange = {
  fromDate: string
  toDateExclusive: string
}

export type CalendarEvent = {
  kind: 'occupied' | 'free'
  title: string
  owner: string
  roomCode: string
  attendees: number
  start: Date
  end: Date
}

export type RoomColor = {
  bg: string
  selected: string
}
