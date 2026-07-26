import type { RoomScheduleParams } from '@/features/rooms/types/room'

export const roomsQueryKeys = {
  all: ['rooms'] as const,
  schedules: ['room-schedule'] as const,
  schedule: (params: RoomScheduleParams) =>
    [...roomsQueryKeys.schedules, params] as const,
}
