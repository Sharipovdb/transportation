// Domain types mirroring the backend business entities (Transportation.Domain.Entities).
// Ids are kept as strings (stringified backend `long` ids) at this layer so existing
// UI bindings (<select value>, equality checks) stay simple — each context converts
// to/from the backend's numeric ids at the API boundary.

import type { AppRole } from './app-types'

/**
 * Backend ids are `long`. Some contexts keep them as numbers (Employee, Crew, Vehicle,
 * TransportRoute, CrewMembership) while the operational ones stringify them
 * (TransportDay, TaxiExpense, PayoutLine, MonthlySheet). A plain `===` across that
 * boundary compares 1 to "1" and is silently always false, which is what used to turn
 * every crew into "Unknown crew". Cross-entity lookups must go through this helper.
 */
export function isSameId(
  left: string | number | null | undefined,
  right: string | number | null | undefined,
) {
  if (left === null || left === undefined) return false
  if (right === null || right === undefined) return false

  return String(left) === String(right)
}

export type EntityId = string | number

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
// and the route distance are derived server-side from the crew — not set here.
export interface TransportDay {
  id: number
  crewId: number
  date: string // ISO yyyy-mm-dd
  morningMode: TransportMode
  afternoonMode: TransportMode | null
  driverId: number | null
  // Length of one leg (crew route + any detour) versus what was actually driven:
  // a taxi leg costs money and covers no distance, so it adds nothing to drivenKm.
  commuteKmPerLeg: number
  drivenKm: number
  extraBusinessKm: number // km driven for company purposes beyond the commute
  notes: string | null
  loggedBy: number
  loggedAt: string
  confirmed: boolean
  // Every taxi leg carries its fare — recorded here, in the daily log, so a ride can
  // never end up without a reimbursable expense behind it.
  taxiFares: TaxiFare[]
}

// The fare of one taxi leg as captured on the transport day.
export interface TaxiFare {
  leg: Leg
  amount: number
  paidById: number
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
// Fully computed server-side — read-only from the frontend's perspective. Two units
// of account, deliberately never mixed: kilometres driven in a member's own car are
// *reported* (priced by hand outside this system), while approved taxi fares are
// *settled* — they are the only money a payout line owes.
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
  driverKm: number // distance driven on commute legs — reported, not priced
  extraBusinessKm: number // company km beyond the commute — reported, not priced
  taxiCompensation: number
  totalAmount: number // == taxiCompensation: km never turn into money here
  // Settlement is per member per month: once paid, the line is closed for the period.
  isPaid: boolean
  paidAt: string | null
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
  // Km and taxi spend are totalled separately: different units of account.
  totalDriverKm: number
  totalExtraBusinessKm: number
  totalTaxiAmount: number
  totalAmount: number
}
