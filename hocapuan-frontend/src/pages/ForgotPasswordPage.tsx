import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { GraduationCap } from 'lucide-react'
import { authApi } from '../services/api'
import { useState } from 'react'

interface FormData { email: string }

export default function ForgotPasswordPage() {
  const { register, handleSubmit, formState: { errors } } = useForm<FormData>()
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [submitted, setSubmitted] = useState(false)

  async function onSubmit(data: FormData) {
    setError('')
    setLoading(true)
    try {
      await authApi.forgotPassword(data.email.trim())
      setSubmitted(true)
    } catch (e: any) {
      setError(e.response?.data?.message || 'İstek gönderilemedi.')
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
          <h1 className="font-display text-2xl text-text">Şifremi unuttum</h1>
          <p className="text-text-muted text-sm mt-1">
            Kayıtlı e-posta adresine sıfırlama bağlantısı gönderilir
          </p>
        </div>

        {submitted ? (
          <div className="card p-6 flex flex-col gap-4">
            <div className="bg-green-50 border border-green-200 rounded-lg p-3 text-sm text-green-800">
              Kayıtlı bir hesap varsa şifre sıfırlama bağlantısı e-posta adresinize gönderildi.
              Gelen kutunuzu ve spam klasörünü kontrol edin (Mailtrap kullanıyorsanız Mailtrap gelen kutusuna bakın).
            </div>
            <Link to="/login" className="btn-primary w-full justify-center py-3 text-center">
              Giriş sayfasına dön
            </Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit(onSubmit)} className="card p-6 flex flex-col gap-4">
            <div>
              <label className="text-sm font-medium text-text mb-1.5 block">E-posta</label>
              <input
                {...register('email', { required: true })}
                type="email"
                className="input"
                placeholder="ornek@uni.edu.tr"
                autoComplete="email"
              />
              {errors.email && <p className="text-xs text-danger mt-1">E-posta gerekli</p>}
            </div>

            {error && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
                {error}
              </div>
            )}

            <button type="submit" disabled={loading} className="btn-primary w-full justify-center py-3">
              {loading ? 'Gönderiliyor...' : 'Sıfırlama bağlantısı gönder'}
            </button>
          </form>
        )}

        <p className="text-center text-sm text-text-muted mt-4">
          <Link to="/login" className="text-primary font-medium hover:underline">Giriş sayfasına dön</Link>
        </p>
      </div>
    </div>
  )
}
