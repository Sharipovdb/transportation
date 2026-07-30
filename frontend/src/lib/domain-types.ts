// Domain types mirroring the backend business entities (Transportation.Domain.Entities).
// Ids are kept as strings (stringified backend `long` ids) at this layer so existing
// UI bindings (<select value>, equality checks) stay simple — each context converts
// to/from the backend's numeric ids at the API boundary.

export const employeeRoles = [
  'routeManager',
  'crewLead',
  'driverLead',
  'worker',
  'accountant',
] as const

export type EmployeeRole = (typeof employeeRoles)[number]

export const employeeRoleLabels: Record<EmployeeRole, string> = {
  routeManager: 'Route Manager',
  crewLead: 'Crew Lead',
  driverLead: 'Driver Lead',
  worker: 'Worker',
  accountant: 'Accountant',
}

// The backend's `Admin` role is a technical access level (see lib/app-types.ts），not
// one of the PRD's business roles, so it's deliberately excluded here — an Admin
// account never shows up as an "Employee".
export interface Employee {
  id: number
  email: string
  userName: string
  firstName: string
  lastName: string
  password: string
  phoneNumber: string
  telegramId: string
}

export function getEmployeeName(
  employee: Pick<Employee, 'firstName' | 'lastName'>,
) {
  return `${employee.firstName} ${employee.lastName}`.trim()
}

export interface TransportRoute {
  id: string
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
export const leadTypes = ['driver', 'manager'] as const
export type LeadType = (typeof leadTypes)[number]

export interface Crew {
  id: string
  name: string
  routeId: string
  leadType: LeadType
  leadId: string
  seatCapacity: number
}

export interface CrewMembership {
  id: string
  crewId: string
  employeeId: string
  activeFrom: string
  activeTo: string | null
  isActive: boolean
}

// --- Daily operations (mirrors Transportation.Domain.Entities.TransportDay) ---

// How a crew travelled on a single leg (morning / afternoon) of a working day.
export const transportModes = ['driven', 'taxi', 'none'] as const
export type TransportMode = (typeof transportModes)[number]

export const transportModeLabels: Record<TransportMode, string> = {
  driven: 'Driven',
  taxi: 'Taxi',
  none: 'None',
}

// A working day for one crew. Splitting the mode into two legs is what makes the
// "drove in the morning, taxied home" case representable without a hack. The driver
// is derived server-side from the crew's Driver-Lead — it isn't set here.
export interface TransportDay {
  id: string
  crewId: string
  date: string // ISO yyyy-mm-dd
  morningMode: TransportMode
  afternoonMode: TransportMode
  driverId: string | null
  commuteKm: number // backend's TotalCommuteKm (route distance + any extra commute km)
  extraBusinessKm: number // km driven for company purposes beyond the commute
  notes: string
  confirmed: boolean
}

// --- Taxi expenses (mirrors Transportation.Domain.Entities.TaxiExpense) ---

export const legs = ['morning', 'afternoon'] as const
export type Leg = (typeof legs)[number]

export const legLabels: Record<Leg, string> = {
  morning: 'Morning',
  afternoon: 'Afternoon',
}

// Mirrors the backend's TaxiExpenseStatus exactly. Only Approved expenses count
// toward a driver's payout; confirming a monthly sheet bulk-flips Approved -> Paid.
export const taxiExpenseStatuses = [
  'pending',
  'approved',
  'rejected',
  'paid',
] as const
export type TaxiExpenseStatus = (typeof taxiExpenseStatuses)[number]

export const taxiExpenseStatusLabels: Record<TaxiExpenseStatus, string> = {
  pending: 'Pending',
  approved: 'Approved',
  rejected: 'Rejected',
  paid: 'Paid',
}

export interface TaxiExpense {
  id: string
  transportDayId: string
  leg: Leg
  amount: number
  paidById: string // employee who fronted the cash
  status: TaxiExpenseStatus
}

// --- Payout line (mirrors Transportation.Domain.Entities.PayoutLine) ---
// Fully computed server-side (commute km x rate, extra km x rate, approved taxi
// expenses) — read-only from the frontend's perspective.
export interface PayoutLineTaxiExpense {
  id: string
  amount: number
  leg: Leg
  status: TaxiExpenseStatus
}

export interface PayoutLine {
  id: string
  employeeId: string
  employeeName: string // denormalized onto PayoutLineDto by the backend
  driverPayment: number
  extraKmPayment: number
  taxiCompensation: number
  totalAmount: number
  taxiExpenses: PayoutLineTaxiExpense[]
}

// --- Monthly sheet (mirrors Transportation.Domain.Entities.MonthlyTransportSheet) ---
// `id` is null for an unsaved preview (POST /GetPreview computes but doesn't persist).
export interface MonthlySheet {
  id: string | null
  crewId: string
  year: number
  month: number // 1-12
  isConfirmed: boolean
  payoutLines: PayoutLine[]
  totalAmount: number
}
