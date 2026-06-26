import { Routes, Route } from 'react-router-dom'
import * as Sentry from '@sentry/react'
import Layout from './components/layout/Layout'
import HomePage from './pages/HomePage'
import SearchPage from './pages/SearchPage'
import ProfessorPage from './pages/ProfessorPage'
import UniversityPage from './pages/UniversityPage'
import UniversitiesPage from './pages/UniversitiesPage'
import DepartmentPage from './pages/DepartmentPage'
import FacultyPage from './pages/FacultyPage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import ForgotPasswordPage from './pages/ForgotPasswordPage'
import ResetPasswordPage from './pages/ResetPasswordPage'
import VerifyEmailPage from './pages/VerifyEmailPage'
import AddReviewPage from './pages/AddReviewPage'
import ProfilePage from './pages/ProfilePage'
import EditReviewPage from './pages/EditReviewPage'
import AdminModerationPage from './pages/AdminModerationPage'
import NotFoundPage from './pages/NotFoundPage'

function SentryFallback() {
  return (
    <div className="min-h-screen flex items-center justify-center p-8 text-center">
      <p className="text-lg text-gray-700">Beklenmeyen bir hata oluştu. Lütfen sayfayı yenileyin.</p>
    </div>
  )
}

export default function App() {
  return (
    <Sentry.ErrorBoundary fallback={<SentryFallback />}>
    <Routes>
      <Route element={<Layout />}>
        <Route path="/"                    element={<HomePage />} />
        <Route path="/search"              element={<SearchPage />} />
        <Route path="/professors/:id"      element={<ProfessorPage />} />
        <Route path="/universities"        element={<UniversitiesPage />} />
        <Route path="/universities/:universityId/faculties/:facultyId" element={<FacultyPage />} />
        <Route path="/universities/:universityId/departments/:departmentId" element={<DepartmentPage />} />
        <Route path="/universities/:id"    element={<UniversityPage />} />
        <Route path="/universite/:universityId/bolum/:departmentId" element={<DepartmentPage />} />
        <Route path="/professors/:id/review" element={<AddReviewPage />} />
        <Route path="/login"               element={<LoginPage />} />
        <Route path="/register"            element={<RegisterPage />} />
        <Route path="/forgot-password"     element={<ForgotPasswordPage />} />
        <Route path="/reset-password"     element={<ResetPasswordPage />} />
        <Route path="/verify-email"       element={<VerifyEmailPage />} />
        <Route path="/profile"            element={<ProfilePage />} />
        <Route path="/reviews/:id/edit"   element={<EditReviewPage />} />
        <Route path="/admin/moderation"  element={<AdminModerationPage />} />
        <Route path="*"                    element={<NotFoundPage />} />
      </Route>
    </Routes>
    </Sentry.ErrorBoundary>
  )
}
