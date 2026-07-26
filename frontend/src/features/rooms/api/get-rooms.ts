import { api } from '@/lib/api-client'
import { unwrapApiResponse } from '@/lib/api-response'

import type { Room } from '@/features/rooms/types/room'
import type { ApiResponse } from '@/types/api'

export async function getRooms(): Promise<Room[]> {
  const { data } = await api.get<ApiResponse<Room[]>>('/rooms')
  return unwrapApiResponse(data)
}
