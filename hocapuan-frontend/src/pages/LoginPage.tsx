import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { GraduationCap } from 'lucide-react'
import { authApi } from '../services/api'
import { useAuthStore } from '../store/authStore'
import { useState } from 'react'

interface FormData { email: string; password: string }

export default function LoginPage() {
  const { register, handleSubmit, formState: { errors } } = useForm<FormData>()
  const login = useAuthStore(s => s.login)
  const navigate = useNavigate()
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function onSubmit(data: FormData) {
    setError('')
    setLoading(true)
    try {
      const res = await authApi.login(data)
      if (!res.success) return setError(res.message || 'Giriş başarısız.')
      login(res.user, res.token)
      navigate('/')
    } catch (e: any) {
      setError(e.response?.data?.message || 'Giriş yapılamadı.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-[calc(100vh-64px)] flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-sm">
        {/* Logo */}
        <div className="text-center mb-8">
          <div className="w-12 h-12 bg-primary rounded-xl flex items-center justify-center mx-auto mb-3">
            <GraduationCap className="w-7 h-7 text-white" />
          </div>
          <h1 className="font-display text-2xl text-text">Tekrar hoş geldin</h1>
          <p className="text-text-muted text-sm mt-1">Hesabına giriş yap</p>
        </div>

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
          </div>
          <div>
            <label className="text-sm font-medium text-text mb-1.5 block">Şifre</label>
            <input
              {...register('password', { required: true })}
              type="password"
              className="input"
              placeholder="••••••••"
              autoComplete="current-password"
            />
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
              {error}
            </div>
          )}

          <button type="submit" disabled={loading} className="btn-primary w-full justify-center py-3">
            {loading ? 'Giriş yapılıyor...' : 'Giriş yap'}
          </button>
        </form>

        <p className="text-center text-sm text-text-muted mt-4">
          Hesabın yok mu?{' '}
          <Link to="/register" className="text-primary font-medium hover:underline">Kayıt ol</Link>
        </p>
      </div>
    </div>
  )
}
