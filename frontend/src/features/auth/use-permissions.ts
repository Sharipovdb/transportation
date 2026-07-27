import { useMemo } from 'react'

import type { Capability } from '@/lib/permissions'
import { canAccessPath, hasCapability } from '@/lib/permissions'

import { useAuth } from './auth-context'

// Thin wrapper over the permission rules so components never touch the session role
// directly. Everything resolves to `false` when signed out.
export function usePermissions() {
  const { session } = useAuth()
  const role = session?.role ?? null

  return useMemo(
    () => ({
      role,
      can: (capability: Capability) => (role ? hasCapability(role, capability) : false),
      canOpen: (pathname: string) => (role ? canAccessPath(role, pathname) : false),
    }),
    [role],
  )
}
