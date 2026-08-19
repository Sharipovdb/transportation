import type { MonthlySheet, MonthlySheetDay } from '@/lib/domain-types'

/**
 * Lays a crew's month out as a calendar.
 *
 * The sheet already carries the figures — this only arranges its days into weeks, so
 * the printed report and the screen show the same numbers rather than two calculations
 * that can drift apart. One marked cell is one working day: the distance the crew
 * covered in its own car, the taxi fare it cost, or both when it drove one leg and
 * taxied the other.
 */

export interface ReportDay {
  dayOfMonth: number
  drivenKm: number
  extraBusinessKm: number
  taxiAmount: number
}

/** `null` pads the leading and trailing cells of a week that fall outside the month. */
export type ReportWeek = (ReportDay | null)[]

export interface MonthlyReport {
  crewName: string
  recipientFullname: string
  year: number
  month: number
  isConfirmed: boolean
  isPaid: boolean
  weeks: ReportWeek[]
  travelledDayCount: number
  totalDrivenKm: number
  totalExtraBusinessKm: number
  totalTaxiAmount: number
}

const daysPerWeek = 7

// The day-of-month is read off the ISO string rather than through a Date, which would
// shift the day for anyone east or west of UTC.
function dayOfMonthOf(day: MonthlySheetDay) {
  return Number(day.date.slice(8, 10))
}

function daysInMonth(year: number, month: number) {
  return new Date(Date.UTC(year, month, 0)).getUTCDate()
}

/** Weekday of the 1st, counted from Monday, which is how the printed grid reads. */
function leadingBlankCount(year: number, month: number) {
  return (new Date(Date.UTC(year, month - 1, 1)).getUTCDay() + 6) % daysPerWeek
}

function toWeeks(year: number, month: number, days: Map<number, ReportDay>) {
  const cells: ReportWeek = [
    ...Array<null>(leadingBlankCount(year, month)).fill(null),
    ...Array.from({ length: daysInMonth(year, month) }, (_, index) => {
      const dayOfMonth = index + 1

      return (
        days.get(dayOfMonth) ?? {
          dayOfMonth,
          drivenKm: 0,
          extraBusinessKm: 0,
          taxiAmount: 0,
        }
      )
    }),
  ]

  while (cells.length % daysPerWeek !== 0) {
    cells.push(null)
  }

  return Array.from({ length: cells.length / daysPerWeek }, (_, week) =>
    cells.slice(week * daysPerWeek, (week + 1) * daysPerWeek),
  )
}

export function buildMonthlyReport(
  sheet: MonthlySheet,
  crewName: string,
): MonthlyReport {
  const days = new Map<number, ReportDay>(
    sheet.days.map((day) => [
      dayOfMonthOf(day),
      {
        dayOfMonth: dayOfMonthOf(day),
        drivenKm: day.drivenKm,
        extraBusinessKm: day.extraBusinessKm,
        taxiAmount: day.taxiAmount,
      },
    ]),
  )

  return {
    crewName,
    recipientFullname: sheet.recipientFullname,
    year: sheet.year,
    month: sheet.month,
    isConfirmed: sheet.isConfirmed,
    isPaid: sheet.isPaid,
    weeks: toWeeks(sheet.year, sheet.month, days),
    travelledDayCount: days.size,
    totalDrivenKm: sheet.totalDrivenKm,
    totalExtraBusinessKm: sheet.totalExtraBusinessKm,
    totalTaxiAmount: sheet.totalTaxiAmount,
  }
}

/** A day the crew neither drove nor paid for prints as a plain, unmarked cell. */
export function hasTravel(day: ReportDay) {
  return day.drivenKm > 0 || day.extraBusinessKm > 0 || day.taxiAmount > 0
}
