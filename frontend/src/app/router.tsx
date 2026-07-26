import { Navigate, Route, Routes } from 'react-router-dom'

import { ProtectedRoute } from '@/app/components/protected-route'
import { WorkspaceRoute } from '@/app/routes/workspace-route'
import { LoginRoute } from '@/features/auth/routes/login-route'

export function AppRouter() {
  return (
    <Routes>
      <Route path="/login" element={<LoginRoute />} />
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <WorkspaceRoute />
          </ProtectedRoute>
        }
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
