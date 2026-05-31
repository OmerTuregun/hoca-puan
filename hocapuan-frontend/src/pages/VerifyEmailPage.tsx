import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { GraduationCap } from 'lucide-react'
import { authApi } from '../services/api'

type Status = 'loading' | 'success' | 'error'

export default function VerifyEmailPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token') ?? ''
  const [status, setStatus] = useState<Status>('loading')

  useEffect(() => {
    if (!token) {
      setStatus('error')
      return
    }

    let cancelled = false
    authApi.verifyEmail(token)
      .then(() => { if (!cancelled) setStatus('success') })
      .catch(() => { if (!cancelled) setStatus('error') })

    return () => { cancelled = true }
  }, [token])

  return (
    <div className="min-h-[calc(100vh-64px)] flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-sm text-center">
        <div className="w-12 h-12 bg-primary rounded-xl flex items-center justify-center mx-auto mb-3">
          <GraduationCap className="w-7 h-7 text-white" />
        </div>

        <div className="card p-6">
          {status === 'loading' && (
            <p className="text-text-muted text-sm">Doğrulanıyor...</p>
          )}
          {status === 'success' && (
            <>
              <p className="text-green-800 text-sm mb-4">
                E-posta adresiniz doğrulandı! Giriş yapabilirsiniz.
              </p>
              <Link to="/login" className="btn-primary w-full justify-center py-3 inline-flex">
                Giriş yap
              </Link>
            </>
          )}
          {status === 'error' && (
            <>
              <p className="text-red-700 text-sm mb-4">
                Geçersiz veya süresi dolmuş link.
              </p>
              <Link to="/register" className="text-primary text-sm font-medium hover:underline">
                Yeniden kayıt ol
              </Link>
            </>
          )}
        </div>
      </div>
    </div>
  )
}
