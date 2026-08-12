import { describe, expect, it } from 'vitest'

import { buildPayoutReport } from './payout-report'
import type {
  PayoutLine,
  TaxiExpense,
  TaxiExpenseStatus,
  TransportDay,
} from '@/lib/domain-types'

const DRIVER_ID = 5
const PASSENGER_ID = 9

function aLine(overrides: Partial<PayoutLine> = {}): PayoutLine {
  return {
    id: 1,
    userId: DRIVER_ID,
    fullname: 'Abbos Kamolov',
    driverKm: 0,
    extraBusinessKm: 0,
    taxiCompensation: 0,
    totalAmount: 0,
    isPaid: false,
    paidAt: null,
    taxiExpenses: [],
    ...overrides,
  }
}

function aDay(overrides: Partial<TransportDay> = {}): TransportDay {
  return {
    id: 100,
    crewId: 1,
    date: '2026-08-03',
    morningMode: 'Driven',
    afternoonMode: 'Driven',
    driverId: DRIVER_ID,
    commuteKmPerLeg: 20,
    drivenKm: 40,
    extraBusinessKm: 0,
    notes: '',
    loggedBy: DRIVER_ID,
    loggedAt: '2026-08-03T00:00:00Z',
    confirmed: true,
    taxiFares: [],
    ...overrides,
  }
}

function anExpense(overrides: Partial<TaxiExpense> = {}): TaxiExpense {
  return {
    id: 900,
    transportDayId: 100,
    leg: 'Morning',
    amount: 30,
    paidById: DRIVER_ID,
    taxiExpenseStatus: 'Approved',
    ...overrides,
  }
}

function build(days: TransportDay[], taxiExpenses: TaxiExpense[] = [], line = aLine()) {
  return buildPayoutReport({
    line,
    crewName: 'Crew A',
    year: 2026,
    month: 8,
    days,
    taxiExpenses,
  })
}

describe('buildPayoutReport', () => {
  it('credits the day driver with the distance actually driven', () => {
    const report = build([aDay({ drivenKm: 40, extraBusinessKm: 6 })])

    expect(report.rows).toHaveLength(1)
    expect(report.totalDrivenKm).toBe(40)
    expect(report.totalExtraBusinessKm).toBe(6)
  })

  it('ignores unconfirmed days, matching what the monthly sheet counts', () => {
    const report = build([aDay({ confirmed: false })])

    expect(report.rows).toEqual([])
    expect(report.totalDrivenKm).toBe(0)
  })

  it('gives a passenger no distance even on a day the crew drove', () => {
    const report = build([aDay()], [], aLine({ userId: PASSENGER_ID }))

    expect(report.rows).toEqual([])
  })

  it.each<TaxiExpenseStatus>(['Approved', 'Paid'])(
    'reimburses a %s taxi fare to whoever fronted it',
    (taxiExpenseStatus) => {
      const report = build(
        [aDay({ morningMode: 'Taxi', drivenKm: 0 })],
        [anExpense({ taxiExpenseStatus, amount: 45 })],
      )

      expect(report.totalTaxiAmount).toBe(45)
      expect(report.rows[0].taxiAmount).toBe(45)
    },
  )

  it.each<TaxiExpenseStatus>(['Pending', 'Rejected'])(
    'leaves a %s taxi fare out of the report',
    (taxiExpenseStatus) => {
      const report = build(
        [aDay({ morningMode: 'Taxi', drivenKm: 0 })],
        [anExpense({ taxiExpenseStatus })],
      )

      expect(report.totalTaxiAmount).toBe(0)
      expect(report.rows).toEqual([])
    },
  )

  it('only counts fares fronted by this member', () => {
    const report = build(
      [aDay({ morningMode: 'Taxi', drivenKm: 0 })],
      [anExpense({ paidById: PASSENGER_ID })],
    )

    expect(report.totalTaxiAmount).toBe(0)
  })

  it('sums both legs of a day into a single row', () => {
    const report = build(
      [aDay({ morningMode: 'Taxi', afternoonMode: 'Taxi', drivenKm: 0 })],
      [
        anExpense({ id: 901, leg: 'Morning', amount: 30 }),
        anExpense({ id: 902, leg: 'Afternoon', amount: 25 }),
      ],
    )

    expect(report.rows).toHaveLength(1)
    expect(report.rows[0].taxiAmount).toBe(55)
  })

  it('reports a mixed day as both distance and money', () => {
    const report = build(
      [aDay({ morningMode: 'Driven', afternoonMode: 'Taxi', drivenKm: 20 })],
      [anExpense({ leg: 'Afternoon', amount: 35 })],
    )

    expect(report.rows[0]).toMatchObject({ drivenKm: 20, taxiAmount: 35 })
  })

  it('orders rows by date so the printout reads as a calendar', () => {
    const report = build([
      aDay({ id: 3, date: '2026-08-20' }),
      aDay({ id: 1, date: '2026-08-04' }),
      aDay({ id: 2, date: '2026-08-11' }),
    ])

    expect(report.rows.map((row) => row.date)).toEqual([
      '2026-08-04',
      '2026-08-11',
      '2026-08-20',
    ])
  })

  it('carries the settlement state through to the printed sheet', () => {
    const report = build([aDay()], [], aLine({ isPaid: true }))

    expect(report.isPaid).toBe(true)
    expect(report.fullname).toBe('Abbos Kamolov')
    expect(report.crewName).toBe('Crew A')
  })
})
