import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, Star, Shield, TrendingUp } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { universityApi } from '../services/api'

export default function HomePage() {
  const [query, setQuery] = useState('')
  const navigate = useNavigate()

  const { data: universities } = useQuery({
    queryKey: ['universities'],
    queryFn: () => universityApi.list(),
  })

  const universityCount = universities?.length

  function handleSearch(e: React.FormEvent) {
    e.preventDefault()
    if (query.trim()) navigate(`/search?q=${encodeURIComponent(query.trim())}`)
    else navigate('/search')
  }

  return (
    <div>
      {/* Hero */}
      <section className="bg-white border-b border-surface-border">
        <div className="max-w-3xl mx-auto px-4 py-20 text-center">
          <h1 className="font-display text-5xl sm:text-6xl text-text mb-4 animate-fadeUp">
            Hocanı değerlendir,
            <br />
            <span className="text-primary">başarını planla.</span>
          </h1>
          <p className="text-text-muted text-lg mb-10 animate-fadeUp animate-fadeUp-delay-1">
            Türkiye'deki {universityCount ?? 219}+ üniversitede gerçek öğrenci deneyimleri.
          </p>

          <form onSubmit={handleSearch} className="flex gap-2 max-w-xl mx-auto animate-fadeUp animate-fadeUp-delay-2">
            <div className="relative flex-1">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-text-light" />
              <input
                type="text"
                value={query}
                onChange={e => setQuery(e.target.value)}
                placeholder="Hoca adı veya üniversite..."
                className="input pl-12 py-3.5 text-base"
                autoFocus
              />
            </div>
            <button type="submit" className="btn-primary px-6 py-3.5">
              Ara
            </button>
          </form>
        </div>
      </section>

      {/* Özellikler */}
      <section className="max-w-5xl mx-auto px-4 py-16">
        <div className="grid sm:grid-cols-3 gap-6">
          {[
            { icon: Star,       title: 'Gerçek Puanlar',    desc: 'Öğrencilerden kalite, zorluk ve tekrar alma puanları.' },
            { icon: Shield,     title: 'Güvenilir İçerik',  desc: 'Moderasyon sistemi ile doğrulanmış yorumlar.' },
            { icon: TrendingUp, title: `${universityCount ?? 219}+ Üniversite`, desc: 'YÖK verisiyle Türkiye\'nin tüm üniversiteleri.' },
          ].map(({ icon: Icon, title, desc }) => (
            <div key={title} className="card p-6 text-center">
              <div className="w-12 h-12 bg-primary-light rounded-xl flex items-center justify-center mx-auto mb-4">
                <Icon className="w-6 h-6 text-primary" />
              </div>
              <h3 className="font-semibold text-text mb-1">{title}</h3>
              <p className="text-sm text-text-muted">{desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* Üniversiteler */}
      {universities && universities.length > 0 && (
        <section className="max-w-5xl mx-auto px-4 pb-16">
          <h2 className="font-display text-2xl text-text mb-6">Üniversiteler</h2>
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-3">
            {universities.slice(0, 9).map(uni => (
              <button
                key={uni.id}
                onClick={() => navigate(`/search?universityId=${uni.id}`)}
                className="card-hover p-4 text-left"
              >
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-semibold text-text text-sm leading-tight">{uni.name}</p>
                    <p className="text-xs text-text-muted mt-0.5">{uni.city} · {uni.type}</p>
                  </div>
                  {uni.totalReviews > 0 && (
                    <span className="badge-primary shrink-0">{uni.totalReviews} yorum</span>
                  )}
                </div>
              </button>
            ))}
          </div>
          <div className="text-center mt-6">
            <button onClick={() => navigate('/universities')} className="btn-outline">
              Tüm üniversiteleri gör
            </button>
          </div>
        </section>
      )}
    </div>
  )
}
