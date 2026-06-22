import { Link } from 'react-router-dom'
import { GraduationCap, Mail } from 'lucide-react'

export default function Footer() {
  return (
    <footer className="bg-white border-t border-surface-border mt-16">
      <div className="max-w-6xl mx-auto px-4 py-8 flex flex-col sm:flex-row items-center justify-between gap-4">
        <div className="flex items-center gap-2 text-text-muted">
          <GraduationCap className="w-5 h-5 text-primary" />
          <span className="font-display text-lg">Hocanı Yorumla</span>
          <span className="text-sm ml-2">— Hocanı değerlendir, başarını planla</span>
        </div>
        <div className="flex flex-col sm:flex-row items-center gap-3 sm:gap-6 text-sm text-text-muted">
          <Link to="/search" className="hover:text-primary transition-colors">Hocalar</Link>
          <Link to="/universities" className="hover:text-primary transition-colors">Üniversiteler</Link>
          <a
            href="mailto:info_hocapuan@gmail.com"
            className="inline-flex items-center gap-2 hover:text-primary transition-colors"
          >
            <Mail className="w-4 h-4" />
            info_hocapuan@gmail.com
          </a>
        </div>
      </div>
    </footer>
  )
}
