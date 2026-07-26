import { useRef, useState } from 'react'

import { useSendChat } from '@/features/chat/hooks/use-send-chat'
import type { ChatMessage } from '@/features/chat/types/chat'
import { getApiErrorMessage } from '@/lib/api-response'

import { Composer } from '@/features/chat/components/composer'
import { MessageList } from '@/features/chat/components/message-list'

type ChatPanelProps = {
  onResponseSuccess?: () => void
}

export function ChatPanel({ onResponseSuccess }: ChatPanelProps) {
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [error, setError] = useState<string | null>(null)
  const messagesRef = useRef(messages)
  messagesRef.current = messages
  const sendChat = useSendChat()

  function handleSend(content: string) {
    const nextMessages: ChatMessage[] = [
      ...messagesRef.current,
      { role: 'user', content },
    ]
    setMessages(nextMessages)
    setError(null)

    sendChat.mutate(nextMessages, {
      onSuccess: (data) => {
        setMessages((current) => [
          ...current,
          { role: 'assistant', content: data.reply },
        ])
        onResponseSuccess?.()
      },
      onError: (err) => {
        setError(getApiErrorMessage(err, 'Unable to send message. Please try again.'))
      },
    })
  }

  return (
    <section className="flex min-h-0 flex-1 flex-col overflow-hidden rounded-3xl border border-stone-200/70 bg-[var(--surface-strong)] shadow-[var(--shadow-soft)]">
      <header className="shrink-0 border-b border-stone-200/60 px-5 py-4 md:px-6">
        <h2 className="font-display text-lg font-semibold text-charcoal">
          Booking assistant
        </h2>
        <p className="mt-1 text-sm text-charcoal-soft">
          Natural language room reservations for Cubo Itatí
        </p>
      </header>

      <MessageList messages={messages} isPending={sendChat.isPending} />

      {error ? (
        <div className="mx-4 mb-2 shrink-0 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 md:mx-6">
          {error}
        </div>
      ) : null}

      <Composer onSend={handleSend} disabled={sendChat.isPending} />
    </section>
  )
}
