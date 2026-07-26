import { ALL_ROOMS_CODE, type Room } from '@/features/rooms/types/room'

type RoomListProps = {
  rooms: Room[]
  selectedCode: string
  onSelect: (code: string) => void
  isLoading?: boolean
}

export function RoomList({
  rooms,
  selectedCode,
  onSelect,
  isLoading,
}: RoomListProps) {
  const isAllSelected = selectedCode === ALL_ROOMS_CODE

  return (
    <div className="grid grid-cols-3 gap-2 sm:grid-cols-6">
      <button
        type="button"
        disabled={isLoading || rooms.length === 0}
        onClick={() => onSelect(ALL_ROOMS_CODE)}
        className={`flex min-w-0 flex-col items-center justify-center rounded-xl px-1 py-2 text-center transition-colors ${
          isAllSelected
            ? 'bg-teal-accent text-white shadow-sm'
            : 'border border-stone-300/80 bg-white/70 text-charcoal hover:border-teal-accent/40 hover:bg-white'
        } disabled:opacity-60`}
      >
        <span className="text-sm font-medium leading-tight">All</span>
        <span
          className={`mt-0.5 text-[10px] leading-tight ${
            isAllSelected ? 'text-teal-soft' : 'text-charcoal-soft'
          }`}
        >
          rooms
        </span>
      </button>
      {rooms.map((room) => {
        const isSelected = room.code === selectedCode
        return (
          <button
            key={room.code}
            type="button"
            disabled={isLoading}
            onClick={() => onSelect(room.code)}
            className={`flex min-w-0 flex-col items-center justify-center rounded-xl px-1 py-2 text-center transition-colors ${
              isSelected
                ? 'bg-teal-accent text-white shadow-sm'
                : 'border border-stone-300/80 bg-white/70 text-charcoal hover:border-teal-accent/40 hover:bg-white'
            } disabled:opacity-60`}
          >
            <span className="text-sm font-medium leading-tight">
              {room.code}
            </span>
            {room.capacity > 0 ? (
              <span
                className={`mt-0.5 text-[10px] leading-tight ${
                  isSelected ? 'text-teal-soft' : 'text-charcoal-soft'
                }`}
              >
                {room.capacity} seats
              </span>
            ) : null}
          </button>
        )
      })}
    </div>
  )
}
