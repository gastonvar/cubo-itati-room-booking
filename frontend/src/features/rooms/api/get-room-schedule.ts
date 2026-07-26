import { api } from '@/lib/api-client'
import { unwrapApiResponse } from '@/lib/api-response'

import type { RoomSchedule, RoomScheduleParams } from '@/features/rooms/types/room'
import type { ApiResponse } from '@/types/api'

export async function getRoomSchedule(
  params: RoomScheduleParams,
): Promise<RoomSchedule> {
  const { data } = await api.get<ApiResponse<RoomSchedule>>(
    `/rooms/${params.code}/schedule`,
    {
      params: {
        fromDate: params.fromDate,
        toDateExclusive: params.toDateExclusive,
      },
    },
  )
  return unwrapApiResponse(data)
}
