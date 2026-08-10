// Domain types mirroring the backend business entities (Transportation.Domain.Entities).
// Ids are kept as strings (stringified backend `long` ids) at this layer so existing
// UI bindings (<select value>, equality checks) stay simple — each context converts
// to/from the backend's numeric ids at the API boundary.

import type { AppRole } from './app-types'

export const employeeRolesAdd = [
  'Admin',
  'RouteManager',
  'Worker',
  'Accountant',
] as const

export const employeeRoles = [
  'Admin',
  'RouteManager',
  'CrewLead',
  'DriverLead',
  'Worker',
  'Accountant',
] as const

export type EmployeeRole = (typeof employeeRoles)[number]

export const employeeRoleLabels: Record<EmployeeRole, string> = {
  Admin: 'Admin',
  RouteManager: 'Route Manager',
  CrewLead: 'Crew Lead',
  DriverLead: 'Driver Lead',
  Worker: 'Worker',
  Accountant: 'Accountant',
}

// The backend's `Admin` role is a technical access level (see lib/app-types.ts），not
// one of the PRD's business roles, so it's deliberately excluded here — an Admin
// account never shows up as an "Employee".
export interface Employee {
  id: number
  email: string | null
  userName: string
  fullname: string
  phoneNumber: string
  telegramId: string | null
  roles: AppRole[]
}

// export function getEmployeeName(
//   employee: Pick<Employee, 'firstName' | 'lastName'>,
// ) {
//   return `${employee.firstName} ${employee.lastName}`.trim()
// }

export interface TransportRoute {
  id: number
  name: string
  distanceKm: number
}

export interface Vehicle {
  id: number
  driverId: number
  plate: string
  seatCount: number
  amortizationBasis: number
}

// A crew lead is either a Driver-Lead (drives their own car) or a Manager-Lead
// (arranges and pays for taxis). The backend models this as two separate nullable
// foreign keys on Crew (driverLeadId / crewLeadId); the frontend simplifies this to a
// single leadId + leadType pair for form handling — translated back to the two FKs at
// the API boundary in features/crews/crews-context.tsx.

export interface Crew {
  id: number
  name: string
  routeId: number
  driverLeadId: number | null
  crewLeadId: number | null
  seatCapacity: number
}

export interface CrewMembership {
  id: number
  crewId: number
  employeeId: number
  activeFrom: string
  activeTo: string | null
  isActive: boolean
}

// --- Daily operations (mirrors Transportation.Domain.Entities.TransportDay) ---

// How a crew travelled on a single leg (morning / afternoon) of a working day.
export const transportModes = ['Driven', 'Taxi', 'None'] as const
export type TransportMode = (typeof transportModes)[number]

export const transportModeLabels: Record<TransportMode, string> = {
  Driven: 'Driven',
  Taxi: 'Taxi',
  None: 'None',
}

// A working day for one crew. Splitting the mode into two legs is what makes the
// "drove in the morning, taxied home" case representable without a hack. The driver
// is derived server-side from the crew's Driver-Lead — it isn't set here.
export interface TransportDay {
  id: number
  crewId: number
  date: string // ISO yyyy-mm-dd
  morningMode: TransportMode
  afternoonMode: TransportMode | null
  driverId: number | null
  totalCommuteKm: number // backend's TotalCommuteKm (route distance + any extra commute km)
  extraBusinessKm: number // km driven for company purposes beyond the commute
  notes: string | null
  loggedBy: number
  loggedAt: string
  confirmed: boolean
  taxiExpenses: TaxiExpense[]
}

// --- Taxi expenses (mirrors Transportation.Domain.Entities.TaxiExpense) ---

export const legs = ['Morning', 'Afternoon'] as const
export type Leg = (typeof legs)[number]

export const legLabels: Record<Leg, string> = {
  Morning: 'Morning',
  Afternoon: 'Afternoon',
}

// Mirrors the backend's TaxiExpenseStatus exactly. Only Approved expenses count
// toward a driver's payout; confirming a monthly sheet bulk-flips Approved -> Paid.
export const taxiExpenseStatuses = [
  'Pending',
  'Approved',
  'Rejected',
  'Paid',
] as const
export type TaxiExpenseStatus = (typeof taxiExpenseStatuses)[number]

export const taxiExpenseStatusLabels: Record<TaxiExpenseStatus, string> = {
  Pending: 'Pending',
  Approved: 'Approved',
  Rejected: 'Rejected',
  Paid: 'Paid',
}

export interface TaxiExpense {
  id: number
  transportDayId: number
  paidById: number
  leg: Leg
  amount: number
  taxiExpenseStatus: TaxiExpenseStatus
}

// --- Payout line (mirrors Transportation.Domain.Entities.PayoutLine) ---
// Fully computed server-side (commute km x rate, extra km x rate, approved taxi
// expenses) — read-only from the frontend's perspective.
export interface PayoutLineTaxiExpense {
  id: number
  amount: number
  leg: Leg
  taxiExpenseStatus: TaxiExpenseStatus
}

export interface PayoutLine {
  id: number
  userId: number
  fullname: string // denormalized onto PayoutLineDto by the backend
  driverPayment: number
  extraKmPayment: number
  taxiCompensation: number
  totalAmount: number
  taxiExpenses: PayoutLineTaxiExpense[]
}

// --- Monthly sheet (mirrors Transportation.Domain.Entities.MonthlyTransportSheet) ---
// `id` is null for an unsaved preview (POST /GetPreview computes but doesn't persist).
export interface MonthlySheet {
  id: number
  crewId: number
  year: number
  month: number // 1-12
  isConfirmed: boolean
  payoutLines: PayoutLine[]
  totalAmount: number
}
