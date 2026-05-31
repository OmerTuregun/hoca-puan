import { useState } from 'react'
import { Star } from 'lucide-react'
import clsx from 'clsx'

interface Props {
  value: number
  onChange?: (v: number) => void
  readonly?: boolean
  size?: number
}

export default function StarRating({ value, onChange, readonly = false, size = 20 }: Props) {
  const [hover, setHover] = useState(0)
  const display = hover || value

  return (
    <div
      className="flex gap-1"
      onMouseLeave={() => !readonly && setHover(0)}
      role={readonly ? undefined : 'group'}
      aria-label={readonly ? undefined : `Puan: ${value || 'seçilmedi'}`}
    >
      {[1, 2, 3, 4, 5].map(star => {
        const filled = display >= star
        return (
          <button
            key={star}
            type="button"
            disabled={readonly}
            aria-label={`${star} yıldız`}
            onClick={() => onChange?.(star)}
            onMouseEnter={() => !readonly && setHover(star)}
            className={clsx(
              'rounded p-0.5 transition-all',
              !readonly && 'hover:scale-110 cursor-pointer',
              readonly && 'cursor-default'
            )}
          >
            <Star
              size={size}
              className={clsx(
                'transition-colors duration-150',
                filled
                  ? 'fill-amber-400 text-amber-400 drop-shadow-sm'
                  : 'fill-transparent text-surface-border hover:text-amber-200'
              )}
            />
          </button>
        )
      })}
    </div>
  )
}
