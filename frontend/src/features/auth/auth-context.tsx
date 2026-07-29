import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'

import { apiClient } from '@/lib/api-client'
import type { ApiResponse } from '@/lib/api-client'
// import { resolveAppRole } from '@/lib/app-types'
import type { AuthSession, AppRole } from '@/lib/app-types'
import {
  clearAuth,
  getStoredSession,
  setAuthTokens,
  setStoredSession,
} from '@/lib/auth-tokens'
import { decodeJwtPayload } from '@/lib/jwt'

interface LoginPayload {
  login: string
  password: string
}

interface AuthContextValue {
  session: AuthSession | null
  isAuthenticated: boolean
  login: (payload: LoginPayload) => Promise<boolean>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

// Shape of GET /api/User/GetMe's `data` — note the backend's own casing (`firstname`,
// not `firstName`; see UserDto.cs).
interface CurrentUserResponse {
  id: number
  email: string
  userName: string
  firstName: string
  lastName: string
  phoneNumber: string
  telegramId: string
}

export function AuthProvider({ children }: { children: ReactNode }) {
  // getStoredSession() returns null unless matching tokens also exist, so a stale
  // session never boots the app into a doomed "authenticated" state.
  const [session, setSession] = useState<AuthSession | null>(() =>
    getStoredSession(),
  )

  useEffect(() => {
    if (session) {
      setStoredSession(session)
    }
    // Clearing is handled centrally by clearAuth() (logout + refresh failure), so a
    // null session here just means "nothing to persist".
  }, [session])

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      isAuthenticated: session !== null,
      login: async ({ login, password }) => {
        try {
          const loginResponse = await apiClient.post<
            ApiResponse<{ accessToken: string; refreshToken: string }>
          >('/api/Auth/login', { userName: login, password })

          if (!loginResponse.data.isSuccess || !loginResponse.data.data) {
            return false
          }

          setAuthTokens(loginResponse.data.data)

          const meResponse =
            await apiClient.get<ApiResponse<CurrentUserResponse>>(
              '/api/User/GetMe',
            )
          const user = meResponse.data.data
          const jwtPayload = decodeJwtPayload(
            loginResponse.data.data.accessToken,
          )

          const roles = jwtPayload?.[
            'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
          ] as AppRole[]

          const formattedRoles = [...new Set(roles)]

          // const role = user ? resolveAppRole(user.roles) : null

          if (!user || formattedRoles.length === 0) {
            clearAuth()
            return false
          }

          setSession({
            id: user.id,
            userName: user.userName,
            firstName: user.firstName,
            lastName: user.lastName,
            email: user.email,
            phoneNumber: user.phoneNumber,
            telegramId: user.telegramId,
            roles: formattedRoles,
          })

          return true
        } catch {
          clearAuth()
          return false
        }
      },
      logout: () => {
        clearAuth()
        setSession(null)
      },
    }),
    [session],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider')
  }

  return context
}
