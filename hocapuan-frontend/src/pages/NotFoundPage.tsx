import { Link } from 'react-router-dom'

export default function NotFoundPage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] text-center px-4">
      <p className="font-display text-8xl text-surface-border mb-4">404</p>
      <h1 className="font-display text-2xl text-text mb-2">Sayfa bulunamadı</h1>
      <p className="text-text-muted mb-6">Aradığın sayfa mevcut değil ya da taşınmış olabilir.</p>
      <Link to="/" className="btn-primary">Ana sayfaya dön</Link>
    </div>
  )
}
