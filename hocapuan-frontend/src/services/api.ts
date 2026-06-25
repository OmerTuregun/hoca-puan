import axios from 'axios'
import { useAuthStore } from '../store/authStore'
import { ensureCsrfToken } from '../utils/ensureCsrfToken'

const MUTATING_METHODS = new Set(['post', 'put', 'patch', 'delete'])

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
  withCredentials: true,
})

api.interceptors.request.use(async config => {
  const method = config.method?.toLowerCase()
  if (method && MUTATING_METHODS.has(method)) {
    await ensureCsrfToken()
    const csrfToken = useAuthStore.getState().csrfToken
    if (csrfToken) {
      config.headers['X-CSRF-TOKEN'] = csrfToken
    }
  }
  return config
})

api.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status !== 401) {
      return Promise.reject(err)
    }

    const requestUrl = String(err.config?.url ?? '')
    if (requestUrl.includes('/auth/login') || requestUrl.includes('/auth/register') || requestUrl.includes('/auth/me')) {
      return Promise.reject(err)
    }

    const { hasHydrated, isLoggedIn } = useAuthStore.getState()
    if (!hasHydrated || !isLoggedIn) {
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
  totalProfessors: number
  departments: Department[]
}

export interface Department {
  id: number
  name: string
  facultyId: number
}

export interface UniversityDepartment {
  id: number
  name: string
  facultyId: number
  facultyName: string
  totalProfessors: number
}

export interface TopProfessor {
  id: number
  fullName: string
  title: string
  facultyName: string
  departmentName: string
  averageQuality: number
  totalReviews: number
}

export interface DepartmentProfessor {
  id: number
  fullName: string
  title: string
  averageQuality: number
  averageDifficulty: number
  wouldTakeAgainPercent: number
  totalReviews: number
}

export interface DepartmentDetail {
  id: number
  name: string
  facultyId: number
  facultyName: string
  universityId: number
  universityName: string
  avgQuality: number
  avgDifficulty: number
  totalProfessors: number
  totalReviews: number
  professors: DepartmentProfessor[]
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
  role: string
  isEmailVerified: boolean
  createdAt: string
  totalReviews: number
}

export interface ContributionHistory {
  totalReviews: number
  totalHelpfulVotes: number
  reviews: PagedResult<Review>
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
  infoMessage?: string
  manualReviewReasons?: string[]
  thumbsUp: number
  thumbsDown: number
  currentUserVote?: boolean | null
  createdAt: string
  isFreshnessVotingOpen: boolean
  freshnessStillValidPercentage?: number | null
  isFlaggedAsOutdated: boolean
  currentUserFreshnessVote?: boolean | null
}

export interface FreshnessVoteResult {
  message: string
  currentUserFreshnessVote?: boolean | null
  freshnessStillValidPercentage?: number | null
  isFlaggedAsOutdated: boolean
}

export interface SearchSuggestion {
  id: number
  name: string
  type: 'professor' | 'university' | 'department'
  context?: string
  universityId?: number
}

// ─── Search ───────────────────────────────────────────────────
export const searchApi = {
  suggestions: (query: string) =>
    api.get<SearchSuggestion[]>('/search/suggestions', { params: { query } }).then(r => r.data),
}

// ─── Auth ────────────────────────────────────────────────────
export const authApi = {
  getCsrfToken: () =>
    api.get<{ token: string }>('/auth/csrf-token').then(r => r.data),
  register: (data: { username: string; email: string; password: string; universityName?: string }) =>
    api.post('/auth/register', data).then(r => r.data),
  login: (data: { email: string; password: string }) =>
    api.post<{ success: boolean; user: UserProfile; message?: string }>('/auth/login', data).then(r => r.data),
  logout: () =>
    api.post('/auth/logout').then(r => r.data),
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
  universityDepartments: (id: number) =>
    api.get<UniversityDepartment[]>(`/universities/${id}/departments`).then(r => r.data),
  departments: (facultyId: number) =>
    api.get<Department[]>(`/universities/faculties/${facultyId}/departments`).then(r => r.data),
  topProfessors: (id: number, limit = 10) =>
    api.get<TopProfessor[]>(`/universities/${id}/top-professors`, { params: { limit } }).then(r => r.data),
  departmentDetail: (universityId: number, departmentId: number) =>
    api.get<DepartmentDetail>(`/universities/${universityId}/departments/${departmentId}`).then(r => r.data),
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
  byProfessor: (
    professorId: number,
    page = 1,
    pageSize = 10,
    options?: { sortBy?: string; tag?: string },
  ) =>
    api.get<PagedResult<Review>>(`/reviews/professor/${professorId}`, {
      params: {
        page,
        pageSize,
        sortBy: options?.sortBy,
        tag: options?.tag || undefined,
      },
    }).then(r => r.data),
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
  pending: (page = 1, pageSize = 20) =>
    api.get<PagedResult<Review>>('/reviews/pending', { params: { page, pageSize } }).then(r => r.data),
  moderate: (id: number, data: { approve: boolean; moderatorNote?: string }) =>
    api.post<Review>(`/reviews/${id}/moderate`, data).then(r => r.data),
  report: (id: number) =>
    api.post<{ message: string; reportCount: number }>(`/reviews/${id}/report`).then(r => r.data),
  freshnessVote: (id: number, isStillValid: boolean) =>
    api.post<FreshnessVoteResult>(`/reviews/${id}/freshness-vote`, { isStillValid }).then(r => r.data),
}

// ─── Users ────────────────────────────────────────────────────
export const userApi = {
  contributions: (page = 1, pageSize = 10) =>
    api.get<ContributionHistory>('/users/me/contributions', { params: { page, pageSize } }).then(r => r.data),
}

export default api
