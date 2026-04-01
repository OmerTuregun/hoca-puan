import { useParams, Link, useNavigate } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { MapPin, BookOpen, Globe, Mail, MessageSquarePlus } from 'lucide-react'
import { professorApi, reviewApi } from '../services/api'
import { useAuthStore } from '../store/authStore'
import RatingBadge from '../components/ui/RatingBadge'
import ReviewCard from '../components/review/ReviewCard'
import Spinner from '../components/ui/Spinner'
import { useState } from 'react'

export default function ProfessorPage() {
  const { id } = useParams<{ id: string }>()
  const profId = Number(id)
  const { isLoggedIn } = useAuthStore()
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [page, setPage] = useState(1)

  const { data: professor, isLoading: profLoading } = useQuery({
    queryKey: ['professor', profId],
    queryFn:  () => professorApi.get(profId),
    enabled:  !!profId,
  })

  const { data: reviews, isLoading: revLoading } = useQuery({
    queryKey: ['reviews', profId, page],
    queryFn:  () => reviewApi.byProfessor(profId, page, 10),
    enabled:  !!profId,
  })

  if (profLoading) return <Spinner />
  if (!professor)  return (
    <div className="max-w-3xl mx-auto px-4 py-16 text-center">
      <p className="text-text-muted">Hoca bulunamadı.</p>
      <Link to="/search" className="btn-primary mt-4 inline-flex">Geri dön</Link>
    </div>
  )

  const p = professor

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">

      {/* ─── Profil kartı ─── */}
      <div className="card p-6 mb-6 animate-fadeUp">
        <div className="flex flex-col sm:flex-row gap-6">
          {/* Avatar */}
          <div className="w-20 h-20 rounded-2xl bg-primary-light flex items-center justify-center text-primary font-bold text-2xl shrink-0">
            {p.firstName?.[0]}{p.lastName?.[0]}
          </div>

          {/* Bilgi */}
          <div className="flex-1">
            <p className="text-sm text-text-muted font-medium">{p.title}</p>
            <h1 className="font-display text-3xl text-text">{p.firstName} {p.lastName}</h1>

            <div className="mt-2 flex flex-wrap gap-x-4 gap-y-1 text-sm text-text-muted">
              <span className="flex items-center gap-1.5">
                <MapPin className="w-4 h-4" />{p.universityName}
              </span>
              <span className="flex items-center gap-1.5">
                <BookOpen className="w-4 h-4" />{p.departmentName}
              </span>
              {p.email && (
                <a href={`mailto:${p.email}`} className="flex items-center gap-1.5 hover:text-primary transition-colors">
                  <Mail className="w-4 h-4" />{p.email}
                </a>
              )}
              {p.personalWebsite && (
                <a href={p.personalWebsite} target="_blank" rel="noopener" className="flex items-center gap-1.5 hover:text-primary transition-colors">
                  <Globe className="w-4 h-4" />Web sitesi
                </a>
              )}
            </div>
          </div>

          {/* Yorum butonu */}
          <div className="shrink-0">
            {isLoggedIn ? (
              <Link to={`/professors/${profId}/review`} className="btn-primary">
                <MessageSquarePlus className="w-4 h-4" />
                Yorum yaz
              </Link>
            ) : (
              <Link to="/login" className="btn-outline">
                Giriş yapıp yorum yaz
              </Link>
            )}
          </div>
        </div>

        {/* ─── İstatistikler ─── */}
        {p.totalReviews > 0 ? (
          <div className="mt-6 pt-6 border-t border-surface-border grid grid-cols-2 sm:grid-cols-4 gap-4">
            <RatingBadge value={p.averageQuality}    size="lg" label="Kalite" />
            <RatingBadge value={p.averageDifficulty} size="lg" label="Zorluk" />
            <div className="flex flex-col items-center gap-1">
              <div className="w-14 h-14 rounded-xl bg-green-50 flex items-center justify-center font-bold text-green-600 text-base">
                {p.wouldTakeAgainPercent}%
              </div>
              <span className="text-xs text-text-muted">Tekrar alır</span>
            </div>
            <div className="flex flex-col items-center gap-1">
              <div className="w-14 h-14 rounded-xl bg-surface-alt flex items-center justify-center font-bold text-text text-base">
                {p.totalReviews}
              </div>
              <span className="text-xs text-text-muted">Yorum</span>
            </div>
          </div>
        ) : (
          <div className="mt-6 pt-6 border-t border-surface-border text-center text-text-muted text-sm">
            Henüz yorum yapılmamış. İlk yorumu sen yaz!
          </div>
        )}
      </div>

      {/* ─── Yorumlar ─── */}
      <h2 className="font-display text-xl text-text mb-4">Yorumlar</h2>

      {revLoading ? (
        <Spinner />
      ) : reviews?.items.length === 0 ? (
        <div className="card p-10 text-center text-text-muted">
          <p>Henüz yorum yok.</p>
          {isLoggedIn && (
            <Link to={`/professors/${profId}/review`} className="btn-primary mt-4 inline-flex">
              İlk yorumu yaz
            </Link>
          )}
        </div>
      ) : (
        <div className="flex flex-col gap-3">
          {reviews?.items.map(r => (
            <ReviewCard
              key={r.id}
              review={r}
              onDelete={() => qc.invalidateQueries({ queryKey: ['reviews', profId] })}
            />
          ))}

          {/* Sayfalama */}
          {reviews && reviews.totalPages > 1 && (
            <div className="flex justify-center gap-2 mt-4">
              <button
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={!reviews.hasPreviousPage}
                className="btn-outline disabled:opacity-40"
              >Önceki</button>
              <span className="btn-ghost cursor-default">{page} / {reviews.totalPages}</span>
              <button
                onClick={() => setPage(p => p + 1)}
                disabled={!reviews.hasNextPage}
                className="btn-outline disabled:opacity-40"
              >Sonraki</button>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
