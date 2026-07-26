import { useQuery } from '@tanstack/react-query'

import { getRooms } from '@/features/rooms/api/get-rooms'
import { roomsQueryKeys } from '@/features/rooms/lib/query-keys'

export function useRooms() {
  return useQuery({
    queryKey: roomsQueryKeys.all,
    queryFn: getRooms,
  })
}
