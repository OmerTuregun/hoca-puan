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

  return (
    <div className="flex gap-0.5">
      {[1, 2, 3, 4, 5].map(star => {
        const filled = (hover || value) >= star
        return (
          <button
            key={star}
            type="button"
            disabled={readonly}
            onClick={() => onChange?.(star)}
            onMouseEnter={() => !readonly && setHover(star)}
            onMouseLeave={() => !readonly && setHover(0)}
            className={clsx('transition-transform', !readonly && 'hover:scale-110 cursor-pointer', readonly && 'cursor-default')}
          >
            <Star
              size={size}
              className={clsx('transition-colors', filled ? 'fill-amber-400 text-amber-400' : 'text-surface-border')}
            />
          </button>
        )
      })}
    </div>
  )
}
