import { useForm } from 'react-hook-form'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { GraduationCap } from 'lucide-react'
import { authApi } from '../services/api'
import { useState } from 'react'
import { isStrongPassword, STRONG_PASSWORD_MESSAGE } from '../utils/passwordStrength'
import PasswordStrengthChecklist from '../components/auth/PasswordStrengthChecklist'
import { parseApiErrorMessage } from '../utils/eduTrEmail'

interface FormData {
  newPassword: string
  confirmPassword: string
}

export default function ResetPasswordPage() {
  const { register, handleSubmit, watch, formState: { errors } } = useForm<FormData>()
  const newPassword = watch('newPassword')
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token') ?? ''
  const navigate = useNavigate()
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function onSubmit(data: FormData) {
    if (!token) {
      setError('Geçersiz veya eksik sıfırlama bağlantısı.')
      return
    }
    setError('')
    setLoading(true)
    try {
      await authApi.resetPassword({ token, newPassword: data.newPassword })
      navigate('/login', { replace: true, state: { passwordReset: true } })
    } catch (e: unknown) {
      setError(parseApiErrorMessage(e, 'Şifre güncellenemedi.'))
    } finally {
      setLoading(false)
    }
  }

  if (!token) {
    return (
      <div className="min-h-[calc(100vh-64px)] flex items-center justify-center px-4 py-12">
        <div className="w-full max-w-sm card p-6 text-center">
          <p className="text-sm text-red-700 mb-4">Geçersiz veya eksik sıfırlama bağlantısı.</p>
          <Link to="/forgot-password" className="text-primary font-medium hover:underline">
            Yeni bağlantı iste
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-[calc(100vh-64px)] flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-sm">
        <div className="text-center mb-8">
          <div className="w-12 h-12 bg-primary rounded-xl flex items-center justify-center mx-auto mb-3">
            <GraduationCap className="w-7 h-7 text-white" />
          </div>
          <h1 className="font-display text-2xl text-text">Yeni şifre belirle</h1>
          <p className="text-text-muted text-sm mt-1">Hesabın için yeni bir şifre gir</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="card p-6 flex flex-col gap-4">
          <div>
            <label className="text-sm font-medium text-text mb-1.5 block">Yeni şifre</label>
            <input
              {...register('newPassword', {
                required: 'Şifre gerekli.',
                validate: v => isStrongPassword(v) || STRONG_PASSWORD_MESSAGE,
              })}
              type="password"
              className="input"
              placeholder="En az 8 karakter"
              autoComplete="new-password"
            />
            <PasswordStrengthChecklist password={newPassword ?? ''} />
            {errors.newPassword && (
              <p className="text-xs text-danger mt-1">{errors.newPassword.message}</p>
            )}
          </div>
          <div>
            <label className="text-sm font-medium text-text mb-1.5 block">Şifre tekrar</label>
            <input
              {...register('confirmPassword', {
                required: true,
                validate: v => v === watch('newPassword') || 'Şifreler eşleşmiyor',
              })}
              type="password"
              className="input"
              placeholder="Şifreni tekrar gir"
              autoComplete="new-password"
            />
            {errors.confirmPassword && (
              <p className="text-xs text-danger mt-1">{errors.confirmPassword.message}</p>
            )}
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
              {error}
            </div>
          )}

          <button type="submit" disabled={loading} className="btn-primary w-full justify-center py-3">
            {loading ? 'Kaydediliyor...' : 'Şifreyi güncelle'}
          </button>
        </form>

        <p className="text-center text-sm text-text-muted mt-4">
          <Link to="/login" className="text-primary font-medium hover:underline">Giriş yap</Link>
        </p>
      </div>
    </div>
  )
}
