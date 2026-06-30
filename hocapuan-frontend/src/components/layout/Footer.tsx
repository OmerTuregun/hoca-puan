import { Link } from 'react-router-dom'
import { GraduationCap, Mail } from 'lucide-react'

export default function Footer() {
  return (
    <footer className="bg-white border-t border-surface-border mt-16">
      <div className="max-w-6xl mx-auto px-4 py-8 flex flex-col items-center text-center gap-6 sm:flex-row sm:items-center sm:justify-between sm:text-left sm:gap-4">
        <div className="flex flex-col items-center gap-2 text-text-muted sm:flex-row sm:items-center sm:gap-2">
          <div className="flex items-center gap-2">
            <GraduationCap className="w-5 h-5 text-primary shrink-0" />
            <span className="font-display text-lg">Hocanı Yorumla</span>
          </div>
          <span className="text-sm sm:ml-2">— Hocanı değerlendir, başarını planla</span>
        </div>
        <div className="flex flex-col items-center gap-3 text-sm text-text-muted sm:flex-row sm:items-center sm:gap-6">
          <Link
            to="/search"
            className="touch-link hover:text-primary transition-colors"
          >
            Hocalar
          </Link>
          <Link
            to="/universities"
            className="touch-link hover:text-primary transition-colors"
          >
            Üniversiteler
          </Link>
          <a
            href="mailto:info_hocapuan@gmail.com"
            className="touch-link gap-2 hover:text-primary transition-colors"
          >
            <Mail className="w-4 h-4 shrink-0" />
            info_hocapuan@gmail.com
          </a>
        </div>
      </div>
    </footer>
  )
}
