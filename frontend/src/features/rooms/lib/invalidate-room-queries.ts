import type { QueryClient } from '@tanstack/react-query'

import { roomsQueryKeys } from '@/features/rooms/lib/query-keys'

export function invalidateRoomQueries(queryClient: QueryClient): void {
  void queryClient.invalidateQueries({ queryKey: roomsQueryKeys.schedules })
  void queryClient.invalidateQueries({ queryKey: roomsQueryKeys.all })
}
