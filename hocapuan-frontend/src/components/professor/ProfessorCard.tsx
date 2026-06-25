import { Link } from 'react-router-dom'
import { MapPin, BookOpen, Users } from 'lucide-react'
import type { Professor } from '../../services/api'
import RatingBadge from '../ui/RatingBadge'
import { displayProfessorName, professorInitials } from '../../utils/professorDisplay'

export default function ProfessorCard({ professor: p }: { professor: Professor }) {
  const displayName = displayProfessorName(p)
  const initials = professorInitials(p)

  return (
    <Link to={`/professors/${p.id}`} className="card-hover block p-5">
      <div className="flex gap-4">
        {/* Avatar */}
        <div className="w-12 h-12 rounded-xl bg-primary-light flex items-center justify-center shrink-0 text-primary font-bold text-lg">
          {initials}
        </div>

        {/* Bilgi */}
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2">
            <div>
              <p className="text-xs text-text-muted font-medium">{p.title}</p>
              <h3 className="font-semibold text-text leading-tight truncate">{displayName}</h3>
            </div>
            <RatingBadge value={p.averageQuality} size="sm" />
          </div>

          <div className="mt-2 flex flex-wrap gap-x-3 gap-y-1 text-xs text-text-muted">
            <span className="flex items-center gap-1">
              <MapPin className="w-3 h-3" />
              {p.universityName}
            </span>
            <span className="flex items-center gap-1">
              <BookOpen className="w-3 h-3" />
              {p.departmentName}
            </span>
          </div>

          {p.totalReviews > 0 && (
            <div className="mt-2 flex gap-3 text-xs">
              <span className="text-text-muted">
                <span className="font-semibold text-text">{p.totalReviews}</span> yorum
              </span>
              <span className="text-text-muted">
                Zorluk: <span className="font-semibold text-text">{p.averageDifficulty.toFixed(1)}</span>/5
              </span>
              <span className="text-text-muted">
                Tekrar alır: <span className="font-semibold text-green-600">{p.wouldTakeAgainPercent}%</span>
              </span>
            </div>
          )}
        </div>
      </div>
    </Link>
  )
}
