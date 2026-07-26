import { useQueries } from '@tanstack/react-query'

import { getRoomSchedule } from '@/features/rooms/api/get-room-schedule'
import { roomsQueryKeys } from '@/features/rooms/lib/query-keys'
import type {
  FreeSlotWithRoom,
  OccupiedSlotWithRoom,
  UseSchedulesParams,
} from '@/features/rooms/types/room'

export function useSchedules({
  codes,
  fromDate,
  toDateExclusive,
  enabled = true,
}: UseSchedulesParams) {
  return useQueries({
    queries: codes.map((code) => ({
      queryKey: roomsQueryKeys.schedule({ code, fromDate, toDateExclusive }),
      queryFn: () => getRoomSchedule({ code, fromDate, toDateExclusive }),
      enabled: enabled && Boolean(code && fromDate && toDateExclusive),
    })),
    combine: (results) => {
      const occupied: OccupiedSlotWithRoom[] = results.flatMap((result, index) => {
        const roomCode = codes[index] ?? ''
        return (result.data?.occupied ?? []).map((slot) => ({
          ...slot,
          roomCode,
        }))
      })

      const freeSlots: FreeSlotWithRoom[] = results.flatMap((result, index) => {
        const roomCode = codes[index] ?? ''
        return (result.data?.freeSlots ?? []).map((slot) => ({
          ...slot,
          roomCode,
        }))
      })

      return {
        occupied,
        freeSlots,
        isLoading: results.some((result) => result.isLoading),
        isFetching: results.some((result) => result.isFetching),
        isError: results.some((result) => result.isError),
        isSuccess: results.length > 0 && results.every((result) => result.isSuccess),
      }
    },
  })
}
