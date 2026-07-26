import { api } from '@/lib/api-client'
import { unwrapApiResponse } from '@/lib/api-response'

import type { ChatRequest, ChatResponse } from '@/features/chat/types/chat'
import type { ApiResponse } from '@/types/api'

export async function sendChat(payload: ChatRequest): Promise<ChatResponse> {
  const { data } = await api.post<ApiResponse<ChatResponse>>('/chat', payload)
  return unwrapApiResponse(data)
}
