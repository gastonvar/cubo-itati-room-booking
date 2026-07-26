import type { RoomColor } from '@/features/rooms/types/room'

/** Stable palette — colors are assigned by room code hash, not a fixed A–E map. */
const PALETTE: RoomColor[] = [
  { bg: '#0f766e', selected: '#0d9488' },
  { bg: '#1d4ed8', selected: '#2563eb' },
  { bg: '#b45309', selected: '#d97706' },
  { bg: '#be123c', selected: '#e11d48' },
  { bg: '#6d28d9', selected: '#7c3aed' },
  { bg: '#0e7490', selected: '#0891b2' },
  { bg: '#a16207', selected: '#ca8a04' },
  { bg: '#9f1239', selected: '#e11d48' },
]

function hashCode(code: string): number {
  let hash = 0
  for (let i = 0; i < code.length; i++) {
    hash = (hash * 31 + code.charCodeAt(i)) | 0
  }
  return Math.abs(hash)
}

export function getRoomColor(code: string): RoomColor {
  return PALETTE[hashCode(code) % PALETTE.length]!
}
