import { useEffect, useRef } from 'react'

import type { ChatMessage } from '@/features/chat/types/chat'

type MessageListProps = {
  messages: ChatMessage[]
  isPending?: boolean
}

export function MessageList({ messages, isPending }: MessageListProps) {
  const bottomRef = useRef<HTMLDivElement>(null)
  const isEmpty = messages.length === 0 && !isPending

  useEffect(() => {
    if (isEmpty) return
    bottomRef.current?.scrollIntoView({ behavior: 'smooth', block: 'end' })
  }, [messages, isPending, isEmpty])

  if (isEmpty) {
    return (
      <div className="flex min-h-0 flex-1 flex-col items-center justify-center px-6 text-center">
        <div className="mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-teal-soft/70 text-teal-accent">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.5"
            className="h-7 w-7"
            aria-hidden
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M8 10h8M8 14h5M5 5h14a2 2 0 012 2v8a2 2 0 01-2 2H9l-4 3V7a2 2 0 012-2z"
            />
          </svg>
        </div>
        <h3 className="font-display text-lg font-semibold text-charcoal">
          Book a room with conversation
        </h3>
        <p className="mt-2 max-w-sm text-sm leading-relaxed text-charcoal-soft">
          Ask about availability, reserve a room, or check your schedule. The
          assistant handles bookings on your behalf.
        </p>
      </div>
    )
  }

  return (
    <div className="flex min-h-0 flex-1 flex-col gap-4 overflow-y-auto overscroll-contain px-4 py-6 md:px-6">
      {messages.map((message, index) => {
        const isUser = message.role === 'user'
        return (
          <div
            key={`${message.role}-${index}-${message.content.slice(0, 24)}`}
            className={`message-enter flex ${isUser ? 'justify-end' : 'justify-start'}`}
            style={{ animationDelay: `${Math.min(index, 6) * 40}ms` }}
          >
            <div
              className={`max-w-[85%] rounded-2xl px-4 py-3 text-sm leading-relaxed shadow-sm md:max-w-[70%] ${
                isUser
                  ? 'rounded-br-md bg-teal-accent text-white'
                  : 'rounded-bl-md border border-stone-200/80 bg-white/90 text-charcoal'
              }`}
            >
              <p className="whitespace-pre-wrap">{message.content}</p>
            </div>
          </div>
        )
      })}

      {isPending ? (
        <div className="message-enter flex justify-start">
          <div className="rounded-2xl rounded-bl-md border border-stone-200/80 bg-white/90 px-4 py-3 shadow-sm">
            <div className="flex items-center gap-2 text-sm text-charcoal-soft">
              <span className="flex gap-1">
                <span className="h-1.5 w-1.5 animate-bounce rounded-full bg-teal-accent [animation-delay:0ms]" />
                <span className="h-1.5 w-1.5 animate-bounce rounded-full bg-teal-accent [animation-delay:120ms]" />
                <span className="h-1.5 w-1.5 animate-bounce rounded-full bg-teal-accent [animation-delay:240ms]" />
              </span>
              Thinking…
            </div>
          </div>
        </div>
      ) : null}

      <div ref={bottomRef} aria-hidden className="h-px shrink-0" />
    </div>
  )
}
