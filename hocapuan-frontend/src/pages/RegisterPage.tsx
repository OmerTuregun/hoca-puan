import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { GraduationCap } from 'lucide-react'
import { authApi, universityApi } from '../services/api'
import { useQuery } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { getEduTrEmailWarning, parseApiErrorMessage } from '../utils/eduTrEmail'
import { isStrongPassword, STRONG_PASSWORD_MESSAGE } from '../utils/passwordStrength'
import PasswordStrengthChecklist from '../components/auth/PasswordStrengthChecklist'
import { useDebounce } from '../hooks/useDebounce'

interface FormData {
  username: string
  email: string
  password: string
  universityName: string
}

export default function RegisterPage() {
  const { register, handleSubmit, watch, formState: { errors } } = useForm<FormData>()
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [registered, setRegistered] = useState(false)
  const [eduTrWarning, setEduTrWarning] = useState<string | null>(null)
  const [passwordWarning, setPasswordWarning] = useState<string | null>(null)
  const email = watch('email')
  const password = watch('password') ?? ''
  const debouncedPassword = useDebounce(password, 400)

  const passwordValid = isStrongPassword(password)
  const canSubmit = passwordValid && !eduTrWarning && !loading

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setEduTrWarning(getEduTrEmailWarning(email))
    }, 400)
    return () => window.clearTimeout(timer)
  }, [email])

  useEffect(() => {
    if (!debouncedPassword) {
      setPasswordWarning(null)
      return
    }
    if (!isStrongPassword(debouncedPassword)) {
      setPasswordWarning(STRONG_PASSWORD_MESSAGE)
    } else {
      setPasswordWarning(null)
    }
  }, [debouncedPassword])

  const { data: universities } = useQuery({
    queryKey: ['universities'],
    queryFn:  () => universityApi.list(),
  })

  async function onSubmit(data: FormData) {
    setError('')
    setLoading(true)
    try {
      const res = await authApi.register(data)
      if (!res.success) return setError(res.message || 'Kayıt başarısız.')
      setRegistered(true)
    } catch (e: unknown) {
      setError(parseApiErrorMessage(e, 'Kayıt olunamadı.'))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-[calc(100vh-64px)] flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-sm">
        <div className="text-center mb-8">
          <div className="w-12 h-12 bg-primary rounded-xl flex items-center justify-center mx-auto mb-3">
            <GraduationCap className="w-7 h-7 text-white" />
          </div>
          <h1 className="font-display text-2xl text-text">Hesap oluştur</h1>
          <p className="text-text-muted text-sm mt-1">Ücretsiz kayıt ol, yorum yaz</p>
        </div>

        {registered ? (
          <div className="card p-6 flex flex-col gap-4">
            <div className="bg-green-50 border border-green-200 rounded-lg p-3 text-sm text-green-800">
              E-postanıza doğrulama linki gönderdik. Lütfen e-postanızı kontrol edin.
            </div>
            <Link to="/login" className="btn-primary w-full justify-center py-3 text-center">
              Giriş sayfasına git
            </Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit(onSubmit)} className="card p-6 flex flex-col gap-4">
            <div>
              <label className="text-sm font-medium text-text mb-1.5 block">Kullanıcı adı</label>
              <input
                {...register('username', { required: true, minLength: 3 })}
                className="input"
                placeholder="kullanici_adi"
              />
              {errors.username && <p className="text-xs text-danger mt-1">En az 3 karakter</p>}
            </div>

            <div>
              <label className="text-sm font-medium text-text mb-1.5 block">E-posta</label>
              <input
                {...register('email', { required: true })}
                type="email"
                className="input"
                placeholder="ornek@uni.edu.tr"
                autoComplete="email"
              />
              {eduTrWarning && (
                <p className="text-xs text-danger mt-1">{eduTrWarning}</p>
              )}
            </div>

            <div>
              <label className="text-sm font-medium text-text mb-1.5 block">Şifre</label>
              <input
                {...register('password', {
                  required: 'Şifre gerekli.',
                  validate: v => isStrongPassword(v) || STRONG_PASSWORD_MESSAGE,
                })}
                type="password"
                className="input"
                placeholder="En az 8 karakter, büyük/küçük harf ve sayı"
                autoComplete="new-password"
                aria-invalid={Boolean(password && !passwordValid)}
                aria-describedby="password-rules"
              />
              <div id="password-rules">
                <PasswordStrengthChecklist password={password} />
              </div>
              {passwordWarning && (
                <p className="text-xs text-danger mt-1">{passwordWarning}</p>
              )}
              {errors.password && !passwordWarning && (
                <p className="text-xs text-danger mt-1">{errors.password.message}</p>
              )}
            </div>

            <div>
              <label className="text-sm font-medium text-text mb-1.5 block">
                Üniversite <span className="text-text-light">(isteğe bağlı)</span>
              </label>
              <select {...register('universityName')} className="input">
                <option value="">Seç...</option>
                {universities?.map(u => (
                  <option key={u.id} value={u.name}>{u.name}</option>
                ))}
              </select>
            </div>

            {error && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
                {error}
              </div>
            )}

            <button
              type="submit"
              disabled={!canSubmit}
              className="btn-primary w-full justify-center py-3 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Kayıt olunuyor...' : 'Kayıt ol'}
            </button>
          </form>
        )}

        <p className="text-center text-sm text-text-muted mt-4">
          Zaten hesabın var mı?{' '}
          <Link to="/login" className="text-primary font-medium hover:underline">Giriş yap</Link>
        </p>
      </div>
    </div>
  )
}
