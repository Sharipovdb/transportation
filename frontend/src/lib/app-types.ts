export const appRoles = [
  'admin',
  'routeManager',
  'crewLead',
  'driverLead',
  'worker',
  'accountant',
] as const

export type AppRole = (typeof appRoles)[number]

export interface AuthSession {
  id: string
  login: string
  name: string
  role: AppRole
}

export const roleLabels: Record<AppRole, string> = {
  admin: 'Admin',
  routeManager: 'Route Manager',
  crewLead: 'Crew Lead',
  driverLead: 'Driver Lead',
  worker: 'Worker',
  accountant: 'Accountant',
}

// The backend's Identity role names (Transportation.Shared.Authorization.RoleNames)
// line up 1:1 with AppRole by design — this is the one place that mapping lives.
const backendRoleToAppRole: Partial<Record<string, AppRole>> = {
  Admin: 'admin',
  RouteManager: 'routeManager',
  CrewLead: 'crewLead',
  DriverLead: 'driverLead',
  Worker: 'worker',
  Accountant: 'accountant',
}

// A backend account can hold multiple roles; the frontend models one role per
// session, so the first recognized role wins — a deliberate simplification, every
// seeded account currently has exactly one role anyway.
export function resolveAppRole(backendRoles: string[]): AppRole | null {
  for (const roleName of backendRoles) {
    const appRole = backendRoleToAppRole[roleName]

    if (appRole) {
      return appRole
    }
  }

  return null
}

// The inverse lookup, for the one place that assigns/removes a backend role
// (features/employees/employees-context.tsx) — derived so the two directions can
// never drift apart.
export const appRoleToBackendRole = Object.fromEntries(
  Object.entries(backendRoleToAppRole).map(([backendRole, appRole]) => [appRole, backendRole]),
) as Record<AppRole, string>
