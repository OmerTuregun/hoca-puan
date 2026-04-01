import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

// JWT token'ı her isteğe ekle
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// 401 → logout
api.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token')
      window.location.href = '/login'
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

export interface Review {
  id: number
  professorId: number
  professorFullName: string
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
  byProfessor: (professorId: number, page = 1, pageSize = 10) =>
    api.get<PagedResult<Review>>(`/reviews/professor/${professorId}`, { params: { page, pageSize } }).then(r => r.data),
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
  vote: (id: number, isUpvote: boolean) =>
    api.post(`/reviews/${id}/vote`, null, { params: { isUpvote } }).then(r => r.data),
  delete: (id: number) =>
    api.delete(`/reviews/${id}`).then(r => r.data),
}

export default api
