import axios from 'axios'
import { useAuthStore } from '../store/authStore'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

function getAuthToken(): string | null {
  const fromStore = useAuthStore.getState().token
  if (fromStore) return fromStore
  return localStorage.getItem('token')
}

// JWT token'ı her isteğe ekle
api.interceptors.request.use(config => {
  const token = getAuthToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// 401 → yalnızca geçerli oturumla yapılan isteklerde logout
api.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status !== 401) {
      return Promise.reject(err)
    }

    const requestUrl = String(err.config?.url ?? '')
    if (requestUrl.includes('/auth/login') || requestUrl.includes('/auth/register')) {
      return Promise.reject(err)
    }

    const { hasHydrated } = useAuthStore.getState()
    if (!hasHydrated) {
      return Promise.reject(err)
    }

    const hadToken = Boolean(getAuthToken())
    if (!hadToken) {
      return Promise.reject(err)
    }

    const path = window.location.pathname
    useAuthStore.getState().logout()
    if (!path.startsWith('/login') && !path.startsWith('/register')) {
      const returnPath = encodeURIComponent(path + window.location.search)
      window.location.href = `/login?session=expired&from=${returnPath}`
    }
    return Promise.reject(err)
  }
)

// ─── Types ──────────────────────────────────────────────────
export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface University {
  id: number
  name: string
  shortName: string
  city: string
  type: string
  averageRating: number
  totalProfessors: number
  totalReviews: number
  logoUrl?: string
  website?: string
  emailDomain?: string
  faculties?: Faculty[]
}

export interface Faculty {
  id: number
  name: string
  universityId: number
  departments: Department[]
}

export interface Department {
  id: number
  name: string
  facultyId: number
}

export interface Professor {
  id: number
  fullName: string
  firstName: string
  lastName: string
  title: string
  universityName: string
  universityId: number
  departmentName: string
  departmentId: number
  facultyName: string
  email?: string
  photoUrl?: string
  personalWebsite?: string
  averageQuality: number
  averageDifficulty: number
  wouldTakeAgainPercent: number
  totalReviews: number
}

export interface VoteResult {
  thumbsUp: number
  thumbsDown: number
  userVote: boolean | null
}

export interface UserProfile {
  id: number
  username: string
  email: string
  universityName?: string
  createdAt: string
  totalReviews: number
}

export interface Review {
  id: number
  userId: number
  professorId: number
  professorFullName: string
  universityName: string
  username: string
  courseCode?: string
  grade?: string
  year: number
  qualityRating: number
  difficultyRating: number
  wouldTakeAgain: boolean
  attendanceMandatory: boolean
  comment: string
  tags: string[]
  status: string
  thumbsUp: number
  thumbsDown: number
  currentUserVote?: boolean | null
  createdAt: string
}

// ─── Auth ────────────────────────────────────────────────────
export const authApi = {
  register: (data: { username: string; email: string; password: string; universityName?: string }) =>
    api.post('/auth/register', data).then(r => r.data),
  login: (data: { email: string; password: string }) =>
    api.post('/auth/login', data).then(r => r.data),
  forgotPassword: (email: string) =>
    api.post<{ message: string }>('/auth/forgot-password', { email }).then(r => r.data),
  resetPassword: (data: { token: string; newPassword: string }) =>
    api.post('/auth/reset-password', data).then(r => r.data),
  verifyEmail: (token: string) =>
    api.get(`/auth/verify-email/${token}`).then(r => r.data),
  me: () =>
    api.get<UserProfile>('/auth/me').then(r => r.data),
}

// ─── Universities ─────────────────────────────────────────────
export const universityApi = {
  list: (search?: string) =>
    api.get<University[]>('/universities', { params: { search } }).then(r => r.data),
  get: (id: number) =>
    api.get<University>(`/universities/${id}`).then(r => r.data),
  faculties: (id: number) =>
    api.get<Faculty[]>(`/universities/${id}/faculties`).then(r => r.data),
  departments: (facultyId: number) =>
    api.get<Department[]>(`/universities/faculties/${facultyId}/departments`).then(r => r.data),
}

// ─── Professors ───────────────────────────────────────────────
export const professorApi = {
  search: (params: {
    query?: string
    universityId?: number
    facultyId?: number
    departmentId?: number
    sortBy?: string
    page?: number
    pageSize?: number
  }) => {
    const { query, ...rest } = params
    return api.get<PagedResult<Professor>>('/professors', {
      params: { ...rest, search: query }
    }).then(r => r.data)
  },
  get: (id: number) =>
    api.get<Professor>(`/professors/${id}`).then(r => r.data),
}

// ─── Reviews ──────────────────────────────────────────────────
export const reviewApi = {
  get: (id: number) =>
    api.get<Review>(`/reviews/${id}`).then(r => r.data),
  byProfessor: (professorId: number, page = 1, pageSize = 10) =>
    api.get<PagedResult<Review>>(`/reviews/professor/${professorId}`, { params: { page, pageSize } }).then(r => r.data),
  myReviews: (page = 1, pageSize = 10) =>
    api.get<PagedResult<Review>>('/reviews/my', { params: { page, pageSize } }).then(r => r.data),
  create: (data: {
    professorId: number
    courseCode?: string
    grade?: string
    year: number
    qualityRating: number
    difficultyRating: number
    wouldTakeAgain: boolean
    attendanceMandatory: boolean
    comment: string
    tags: string[]
  }) => api.post<Review>('/reviews', data).then(r => r.data),
  update: (id: number, data: {
    courseCode?: string
    grade?: string
    year: number
    qualityRating: number
    difficultyRating: number
    wouldTakeAgain: boolean
    attendanceMandatory: boolean
    comment: string
    tags: string[]
  }) => api.put<Review>(`/reviews/${id}`, data).then(r => r.data),
  vote: (id: number, isUpvote: boolean) =>
    api.post<VoteResult>(`/reviews/${id}/vote`, null, { params: { isUpvote } }).then(r => r.data),
  delete: (id: number) =>
    api.delete(`/reviews/${id}`).then(r => r.data),
}

export default api
