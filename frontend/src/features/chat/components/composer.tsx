import { useState, type FormEvent } from 'react'

import { Button } from '@/components/button'

type ComposerProps = {
  onSend: (content: string) => void
  disabled?: boolean
}

export function Composer({ onSend, disabled }: ComposerProps) {
  const [value, setValue] = useState('')

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    const trimmed = value.trim()
    if (!trimmed || disabled) return
    onSend(trimmed)
    setValue('')
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="shrink-0 border-t border-stone-200/70 bg-white/60 p-4 backdrop-blur-sm md:p-5"
    >
      <div className="flex items-end gap-3">
        <textarea
          value={value}
          onChange={(event) => setValue(event.target.value)}
          onKeyDown={(event) => {
            if (event.key === 'Enter' && !event.shiftKey) {
              event.preventDefault()
              handleSubmit(event)
            }
          }}
          placeholder="Ask to book a room, check availability…"
          rows={2}
          disabled={disabled}
          className="min-h-[52px] flex-1 resize-none rounded-2xl border border-stone-300/80 bg-white/80 px-4 py-3 text-sm text-charcoal shadow-sm placeholder:text-stone-400 focus:border-teal-accent focus:outline-none focus:ring-2 focus:ring-teal-accent/20 disabled:opacity-60"
        />
        <Button type="submit" disabled={disabled || !value.trim()} className="shrink-0">
          Send
        </Button>
      </div>
    </form>
  )
}
