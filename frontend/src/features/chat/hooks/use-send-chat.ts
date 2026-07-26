import { useMutation } from '@tanstack/react-query'

import { sendChat } from '@/features/chat/api/send-chat'
import type { ChatMessage } from '@/features/chat/types/chat'

export function useSendChat() {
  return useMutation({
    mutationFn: (messages: ChatMessage[]) => sendChat({ messages }),
  })
}
