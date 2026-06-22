import { useState, useRef, useEffect } from 'react'
import { Share2, Link2, MessageCircle } from 'lucide-react'
import clsx from 'clsx'

interface Props {
  professorName: string
  universityName: string
}

function buildShareUrl() {
  if (typeof window === 'undefined') return ''
  return window.location.href.split('#')[0]
}

function buildShareText(professorName: string, universityName: string) {
  return `${professorName} - ${universityName} hakkında yorumları gör`
}

export default function ShareProfessorButton({ professorName, universityName }: Props) {
  const [menuOpen, setMenuOpen] = useState(false)
  const [copied, setCopied] = useState(false)
  const menuRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  async function handleShare() {
    const url = buildShareUrl()
    const text = buildShareText(professorName, universityName)

    if (typeof navigator !== 'undefined' && typeof navigator.share === 'function') {
      try {
        await navigator.share({ title: professorName, text, url })
        return
      } catch {
        // Kullanıcı iptal etti veya API başarısız — menüye düş
      }
    }

    setMenuOpen(open => !open)
  }

  async function copyLink() {
    const url = buildShareUrl()
    await navigator.clipboard.writeText(url)
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
    setMenuOpen(false)
  }

  function openExternal(url: string) {
    window.open(url, '_blank', 'noopener,noreferrer')
    setMenuOpen(false)
  }

  const shareUrl = encodeURIComponent(buildShareUrl())
  const shareText = encodeURIComponent(buildShareText(professorName, universityName))

  return (
    <div className="relative" ref={menuRef}>
      <button
        type="button"
        onClick={handleShare}
        className="btn-outline min-h-[44px]"
        aria-expanded={menuOpen}
        aria-haspopup="menu"
      >
        <Share2 className="w-4 h-4" />
        Paylaş
      </button>

      {copied && (
        <span className="absolute -bottom-8 left-1/2 -translate-x-1/2 text-xs text-green-700 bg-green-50 border border-green-200 rounded px-2 py-1 whitespace-nowrap z-10">
          Kopyalandı!
        </span>
      )}

      {menuOpen && (
        <div
          role="menu"
          className="absolute right-0 mt-2 w-56 card p-2 shadow-lg z-20 flex flex-col gap-1"
        >
          <button
            type="button"
            role="menuitem"
            className={clsx(
              'w-full text-left px-3 py-2.5 rounded-lg text-sm hover:bg-surface-alt transition-colors',
              'flex items-center gap-2 min-h-[44px]'
            )}
            onClick={() => openExternal(`https://wa.me/?text=${shareText}%20${shareUrl}`)}
          >
            <MessageCircle className="w-4 h-4 text-green-600" />
            WhatsApp&apos;ta Paylaş
          </button>
          <button
            type="button"
            role="menuitem"
            className={clsx(
              'w-full text-left px-3 py-2.5 rounded-lg text-sm hover:bg-surface-alt transition-colors',
              'flex items-center gap-2 min-h-[44px]'
            )}
            onClick={() => openExternal(`https://twitter.com/intent/tweet?text=${shareText}&url=${shareUrl}`)}
          >
            <Share2 className="w-4 h-4" />
            Twitter&apos;da Paylaş
          </button>
          <button
            type="button"
            role="menuitem"
            className={clsx(
              'w-full text-left px-3 py-2.5 rounded-lg text-sm hover:bg-surface-alt transition-colors',
              'flex items-center gap-2 min-h-[44px]'
            )}
            onClick={copyLink}
          >
            <Link2 className="w-4 h-4" />
            Linki Kopyala
          </button>
        </div>
      )}
    </div>
  )
}
