import { describe, expect, it } from 'vitest'

import { buildMonthlyReport, hasTravel } from './monthly-report'
import type { ReportDay } from './monthly-report'
import type { MonthlySheet, MonthlySheetDay } from '@/lib/domain-types'

// August 2026 starts on a Saturday and has 31 days.
const year = 2026
const month = 8

function aDay(overrides: Partial<MonthlySheetDay> = {}): MonthlySheetDay {
  return {
    transportDayId: 1,
    date: '2026-08-03',
    drivenKm: 40,
    extraBusinessKm: 0,
    taxiAmount: 0,
    ...overrides,
  }
}

function aSheet(overrides: Partial<MonthlySheet> = {}): MonthlySheet {
  const days = overrides.days ?? [aDay()]

  return {
    id: 1,
    crewId: 1,
    year,
    month,
    recipientId: 7,
    recipientFullname: 'Abbos Kamolov',
    isConfirmed: false,
    isPaid: false,
    paidAt: null,
    totalDrivenKm: days.reduce((sum, day) => sum + day.drivenKm, 0),
    totalExtraBusinessKm: days.reduce((sum, day) => sum + day.extraBusinessKm, 0),
    totalTaxiAmount: days.reduce((sum, day) => sum + day.taxiAmount, 0),
    ...overrides,
    days,
  }
}

function cellFor(report: ReturnType<typeof buildMonthlyReport>, dayOfMonth: number) {
  return report.weeks
    .flat()
    .find((cell): cell is ReportDay => cell?.dayOfMonth === dayOfMonth)
}

describe('buildMonthlyReport', () => {
  it('lays the month out Monday-first, padding the days before the 1st', () => {
    const report = buildMonthlyReport(aSheet(), 'Crew A')

    // 1 August 2026 is a Saturday: five blank cells precede it.
    expect(report.weeks[0].slice(0, 5)).toEqual([null, null, null, null, null])
    expect(report.weeks[0][5]).toMatchObject({ dayOfMonth: 1 })
  })

  it('gives every week seven cells', () => {
    const report = buildMonthlyReport(aSheet(), 'Crew A')

    expect(report.weeks.every((week) => week.length === 7)).toBe(true)
  })

  it('covers every day of the month exactly once', () => {
    const report = buildMonthlyReport(aSheet(), 'Crew A')
    const dayNumbers = report.weeks.flat().filter(Boolean).map((cell) => cell!.dayOfMonth)

    expect(dayNumbers).toEqual(Array.from({ length: 31 }, (_, index) => index + 1))
  })

  it('marks a driven day with its round-trip distance', () => {
    const report = buildMonthlyReport(
      aSheet({ days: [aDay({ date: '2026-08-12', drivenKm: 40 })] }),
      'Crew A',
    )

    expect(cellFor(report, 12)).toMatchObject({ drivenKm: 40, taxiAmount: 0 })
  })

  it('marks a taxi day with its fare and no distance', () => {
    const report = buildMonthlyReport(
      aSheet({ days: [aDay({ date: '2026-08-12', drivenKm: 0, taxiAmount: 55 })] }),
      'Crew A',
    )

    expect(cellFor(report, 12)).toMatchObject({ drivenKm: 0, taxiAmount: 55 })
  })

  it('leaves a day with no logged travel unmarked', () => {
    const report = buildMonthlyReport(aSheet({ days: [aDay({ date: '2026-08-12' })] }), 'Crew A')

    expect(hasTravel(cellFor(report, 13)!)).toBe(false)
    expect(hasTravel(cellFor(report, 12)!)).toBe(true)
  })

  it('reads the day off the ISO date rather than a local Date', () => {
    // A Date-based reading shifts 1 August back to 31 July west of UTC.
    const report = buildMonthlyReport(
      aSheet({ days: [aDay({ date: '2026-08-01' })] }),
      'Crew A',
    )

    expect(hasTravel(cellFor(report, 1)!)).toBe(true)
    expect(report.travelledDayCount).toBe(1)
  })

  it('takes its totals from the sheet instead of recomputing them', () => {
    const report = buildMonthlyReport(
      aSheet({
        days: [
          aDay({ date: '2026-08-03', drivenKm: 40, extraBusinessKm: 6 }),
          aDay({ transportDayId: 2, date: '2026-08-04', drivenKm: 0, taxiAmount: 55 }),
        ],
      }),
      'Crew A',
    )

    expect(report).toMatchObject({
      travelledDayCount: 2,
      totalDrivenKm: 40,
      totalExtraBusinessKm: 6,
      totalTaxiAmount: 55,
    })
  })

  it('carries the crew, the lead being paid, and the settlement state', () => {
    const report = buildMonthlyReport(
      aSheet({ isConfirmed: true, isPaid: true }),
      'Crew A',
    )

    expect(report).toMatchObject({
      crewName: 'Crew A',
      recipientFullname: 'Abbos Kamolov',
      isConfirmed: true,
      isPaid: true,
    })
  })
})
