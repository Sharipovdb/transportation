// Single source of truth for access control.
//
// Page access is declared once here and consumed by BOTH the sidebar and the route
// guard, so a role can never see a link it is not allowed to open. Capabilities cover
// the finer-grained rights *inside* a page that several roles share — e.g. a crew lead
// and an accountant both open Monthly Sheets, but only the lead confirms them.

import type { AppRole } from './app-types'

export const appPagePaths = {
  dashboard: '/',
  employees: '/employees',
  crews: '/crews',
  transportDays: '/transport-days',
  routes: '/routes',
  vehicles: '/vehicles',
  taxiExpenses: '/taxi-expenses',
  payouts: '/payouts',
  monthlySheets: '/monthly-sheets',
} as const

const {
  dashboard,
  employees,
  crews,
  transportDays,
  routes,
  vehicles,
  taxiExpenses,
  payouts,
  monthlySheets,
} = appPagePaths

// Derived from the roles in the spec:
// - Route Manager assigns people to crews and routes; no financial screens.
// - Crew leads (manager or driver) log days, record taxis, and confirm their sheet.
// - The accountant "sees all" for reconciliation.
// - A worker only notifies absence, so they get the dashboard until an Absences page exists.
// Employees (login accounts + business profile in one, backed by /api/User) is where
// an Admin manages everyone, including role assignment — there's no separate "Users" page.
const rolePagePaths: Record<AppRole, ReadonlyArray<string>> = {
  Admin: [
    dashboard,
    employees,
    crews,
    transportDays,
    routes,
    vehicles,
    taxiExpenses,
    payouts,
    monthlySheets,
  ],
  Accountant: [
    dashboard,
    employees,
    crews,
    transportDays,
    routes,
    vehicles,
    taxiExpenses,
    payouts,
    monthlySheets,
  ],
  RouteManager: [dashboard, employees, crews, transportDays, routes, vehicles],
  CrewLead: [dashboard, crews, transportDays, taxiExpenses],
  DriverLead: [dashboard, crews, transportDays, taxiExpenses, vehicles],
  Worker: [dashboard],
}

function matchesPath(allowedPath: string, pathname: string) {
  if (allowedPath === dashboard) {
    return pathname === dashboard
  }

  return pathname === allowedPath || pathname.startsWith(`${allowedPath}/`)
}

export function canAccessPath(roles: AppRole[], pathname: string) {
  return roles.some((role) =>
    rolePagePaths[role].some((allowedPath) =>
      matchesPath(allowedPath, pathname),
    ),
  )
}

// Rights inside a page that is already visible to several roles.
export type Capability =
  | 'viewFinance' // payout figures and outstanding money on the dashboard
  | 'generateSheet'
  | 'confirmSheet' // the crew lead confirms their own sheet (FR-20)
  | 'deleteSheet' // the backend has no "reopen" — the correction path is delete + regenerate
  | 'approveTaxiExpense' // Approve/Reject a taxi expense before it counts toward a payout

const roleCapabilities: Record<AppRole, ReadonlyArray<Capability>> = {
  Admin: [
    'viewFinance',
    'generateSheet',
    'confirmSheet',
    'deleteSheet',
    'approveTaxiExpense',
  ],
  Accountant: [
    'viewFinance',
    'generateSheet',
    'deleteSheet',
    'approveTaxiExpense',
  ],
  RouteManager: [],
  CrewLead: ['confirmSheet'],
  DriverLead: ['confirmSheet'],
  Worker: [],
}

export function hasCapability(roles: AppRole[], capability: Capability) {
  return roles.some((role) => roleCapabilities[role].includes(capability))
}
