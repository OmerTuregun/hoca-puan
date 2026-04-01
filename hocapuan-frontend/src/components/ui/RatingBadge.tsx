import clsx from 'clsx'

interface Props {
  value: number
  size?: 'sm' | 'md' | 'lg'
  label?: string
}

function ratingColor(v: number) {
  if (v >= 4.0) return 'bg-green-500'
  if (v >= 3.0) return 'bg-amber-400'
  if (v >= 2.0) return 'bg-orange-400'
  return 'bg-red-500'
}

export default function RatingBadge({ value, size = 'md', label }: Props) {
  const sizes = { sm: 'w-8 h-8 text-xs', md: 'w-11 h-11 text-sm', lg: 'w-14 h-14 text-base' }

  if (value === 0) {
    return (
      <div className="flex flex-col items-center gap-1">
        <div className={clsx('rounded-xl flex items-center justify-center bg-surface-border text-text-light font-bold', sizes[size])}>
          —
        </div>
        {label && <span className="text-xs text-text-light">{label}</span>}
      </div>
    )
  }

  return (
    <div className="flex flex-col items-center gap-1">
      <div className={clsx('rounded-xl flex items-center justify-center text-white font-bold', ratingColor(value), sizes[size])}>
        {value.toFixed(1)}
      </div>
      {label && <span className="text-xs text-text-muted">{label}</span>}
    </div>
  )
}
