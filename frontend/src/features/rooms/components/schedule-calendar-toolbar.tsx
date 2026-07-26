import type { ToolbarProps, View, ViewsProps } from 'react-big-calendar'

import type { CalendarEvent } from '@/features/rooms/types/room'

const segmentBase =
  'border px-3 py-[0.35rem] text-[0.8125rem] font-medium transition-colors focus:outline-none'

const segmentIdle =
  'border-stone-900/10 bg-white/80 text-charcoal hover:border-teal-accent/35 hover:bg-white hover:text-teal-accent focus:border-teal-accent/35 focus:bg-white focus:text-teal-accent'

const segmentActive =
  'border-teal-accent bg-teal-accent text-white shadow-none hover:border-teal-accent hover:bg-teal-accent hover:text-white focus:border-teal-accent focus:bg-teal-accent focus:text-white'

function segmentClass(active: boolean, radius: string) {
  return `${segmentBase} ${active ? segmentActive : segmentIdle} ${radius}`
}

function viewNamesFromProps(views: ViewsProps<CalendarEvent>): View[] {
  if (Array.isArray(views)) {
    return views
  }
  return (Object.keys(views) as View[]).filter((name) => views[name])
}

export function ScheduleCalendarToolbar({
  label,
  localizer: { messages },
  onNavigate,
  onView,
  view,
  views,
}: ToolbarProps<CalendarEvent>) {
  const viewNames = viewNamesFromProps(views)

  return (
    <div className="mb-3 grid shrink-0 grid-cols-[1fr_auto] items-center gap-x-3 gap-y-2 [grid-template-areas:'label_label'_'nav_views']">
      <span className="p-0 text-center font-display text-[0.95rem] font-semibold text-charcoal [grid-area:label]">
        {label}
      </span>

      <span className="inline-flex justify-self-start [grid-area:nav]">
        <button
          type="button"
          className={segmentClass(false, 'rounded-l-xl rounded-r-none')}
          onClick={() => onNavigate('PREV')}
        >
          {messages.previous}
        </button>
        <button
          type="button"
          className={segmentClass(false, '-ml-px rounded-none')}
          onClick={() => onNavigate('TODAY')}
        >
          {messages.today}
        </button>
        <button
          type="button"
          className={segmentClass(false, '-ml-px rounded-l-none rounded-r-xl')}
          onClick={() => onNavigate('NEXT')}
        >
          {messages.next}
        </button>
      </span>

      {viewNames.length > 1 ? (
        <span className="inline-flex justify-self-end [grid-area:views]">
          {viewNames.map((name, index) => {
            const isFirst = index === 0
            const isLast = index === viewNames.length - 1
            const radius = isFirst
              ? 'rounded-l-xl rounded-r-none'
              : isLast
                ? '-ml-px rounded-l-none rounded-r-xl'
                : '-ml-px rounded-none'

            return (
              <button
                key={name}
                type="button"
                className={segmentClass(view === name, radius)}
                onClick={() => onView(name)}
              >
                {messages[name]}
              </button>
            )
          })}
        </span>
      ) : null}
    </div>
  )
}
