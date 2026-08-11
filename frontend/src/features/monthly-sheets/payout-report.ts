import { isSameId } from '@/lib/domain-types'
import type {
  PayoutLine,
  TaxiExpense,
  TaxiExpenseStatus,
  TransportDay,
  TransportMode,
} from '@/lib/domain-types'

/**
 * A taxi fare counts towards a member once it has been approved. Confirming the monthly
 * sheet moves the same fare on to `paid`, so both states describe money the company
 * owes (or has settled) — only `pending` and `rejected` are left out.
 */
const settledTaxiStatuses: TaxiExpenseStatus[] = ['approved', 'paid']

export interface PayoutReportRow {
  date: string // ISO yyyy-mm-dd
  morningMode: TransportMode
  afternoonMode: TransportMode
  drivenKm: number
  extraBusinessKm: number
  taxiAmount: number
}

export interface PayoutReport {
  employeeName: string
  crewName: string
  year: number
  month: number
  isPaid: boolean
  rows: PayoutReportRow[]
  totalDrivenKm: number
  totalExtraBusinessKm: number
  totalTaxiAmount: number
}

interface BuildPayoutReportInput {
  line: PayoutLine
  crewName: string
  year: number
  month: number
  /** Every transport day logged for the crew in the period, confirmed or not. */
  days: TransportDay[]
  /** Every taxi expense known to the app; filtered down to this member's settled ones. */
  taxiExpenses: TaxiExpense[]
}

/**
 * Rebuilds the day-by-day detail behind one payout line.
 *
 * It mirrors what the backend's MonthlyTransportCalculator did to produce the line:
 * only confirmed days count, the day's driver earns its distance, and whoever fronted a
 * taxi fare is owed that money. Distance and money stay separate — kilometres driven in
 * a member's own car are reported here and priced by hand elsewhere.
 */
export function buildPayoutReport({
  line,
  crewName,
  year,
  month,
  days,
  taxiExpenses,
}: BuildPayoutReportInput): PayoutReport {
  const taxiByDay = new Map<string, number>()

  for (const expense of taxiExpenses) {
    if (!isSameId(expense.paidById, line.employeeId)) continue
    if (!settledTaxiStatuses.includes(expense.status)) continue

    taxiByDay.set(
      expense.transportDayId,
      (taxiByDay.get(expense.transportDayId) ?? 0) + expense.amount,
    )
  }

  const rows = days
    .filter((day) => day.confirmed)
    .map((day) => {
      const drove = isSameId(day.driverId, line.employeeId)

      return {
        date: day.date,
        morningMode: day.morningMode,
        afternoonMode: day.afternoonMode,
        drivenKm: drove ? day.drivenKm : 0,
        extraBusinessKm: drove ? day.extraBusinessKm : 0,
        taxiAmount: taxiByDay.get(day.id) ?? 0,
      }
    })
    // A day this member neither drove nor paid for tells them nothing — leaving it out
    // keeps the printout to the days they are actually being reported for.
    .filter((row) => row.drivenKm > 0 || row.extraBusinessKm > 0 || row.taxiAmount > 0)
    .sort((first, second) => first.date.localeCompare(second.date))

  const total = (pick: (row: PayoutReportRow) => number) =>
    rows.reduce((sum, row) => sum + pick(row), 0)

  return {
    employeeName: line.employeeName,
    crewName,
    year,
    month,
    isPaid: line.isPaid,
    rows,
    totalDrivenKm: total((row) => row.drivenKm),
    totalExtraBusinessKm: total((row) => row.extraBusinessKm),
    totalTaxiAmount: total((row) => row.taxiAmount),
  }
}
