import { describe, expect, it } from 'vitest'

import { getNavigationItemsForRole } from '@/app/navigation'
import { appPagePaths, canAccessPath, hasCapability } from '@/lib/permissions'

const { dashboard, payouts, monthlySheets, employees, crews, taxiExpenses, vehicles } =
  appPagePaths

// A session carries a list of roles, so every assertion passes one — a single-role
// array is the common case.
const admin = ['Admin'] as const
const accountant = ['Accountant'] as const
const routeManager = ['RouteManager'] as const
const crewLead = ['CrewLead'] as const
const driverLead = ['DriverLead'] as const
const worker = ['Worker'] as const

describe('page access', () => {
  it('admin can open every page', () => {
    for (const path of Object.values(appPagePaths)) {
      expect(canAccessPath([...admin], path)).toBe(true)
    }
  })

  it('worker only gets the dashboard', () => {
    expect(canAccessPath([...worker], dashboard)).toBe(true)
    expect(getNavigationItemsForRole([...worker]).map((item) => item.to)).toEqual([dashboard])
  })

  it('route manager handles assignment but no finance', () => {
    expect(canAccessPath([...routeManager], employees)).toBe(true)
    expect(canAccessPath([...routeManager], crews)).toBe(true)
    expect(canAccessPath([...routeManager], payouts)).toBe(false)
    expect(canAccessPath([...routeManager], monthlySheets)).toBe(false)
  })

  it('crew lead works its crew, not payroll or the directory', () => {
    expect(canAccessPath([...crewLead], monthlySheets)).toBe(false)
    expect(canAccessPath([...crewLead], taxiExpenses)).toBe(true)
    expect(canAccessPath([...crewLead], payouts)).toBe(false)
    expect(canAccessPath([...crewLead], employees)).toBe(false)
  })

  it('driver lead additionally sees vehicles', () => {
    expect(canAccessPath([...driverLead], vehicles)).toBe(true)
    expect(canAccessPath([...crewLead], vehicles)).toBe(false)
  })

  it('accountant sees the finance pages but not the directory', () => {
    expect(canAccessPath([...accountant], payouts)).toBe(true)
    expect(canAccessPath([...accountant], monthlySheets)).toBe(true)
    expect(canAccessPath([...accountant], employees)).toBe(false)
  })

  it('nested paths inherit access, and dashboard stays exact', () => {
    expect(canAccessPath([...admin], `${crews}/crew-001`)).toBe(true)
    expect(canAccessPath([...worker], `${crews}/crew-001`)).toBe(false)
    // '/' must not act as a prefix for every route
    expect(canAccessPath([...worker], payouts)).toBe(false)
  })

  it('multiple roles union their access', () => {
    expect(canAccessPath([...worker, ...accountant], payouts)).toBe(true)
  })
})

describe('capabilities', () => {
  it('the crew lead confirms, the accountant deletes (no reopen exists server-side)', () => {
    expect(hasCapability([...crewLead], 'confirmSheet')).toBe(true)
    expect(hasCapability([...crewLead], 'deleteSheet')).toBe(false)
    expect(hasCapability([...accountant], 'deleteSheet')).toBe(true)
    expect(hasCapability([...accountant], 'confirmSheet')).toBe(false)
  })

  it('money is limited to admin and accountant', () => {
    expect(hasCapability([...admin], 'viewFinance')).toBe(true)
    expect(hasCapability([...accountant], 'viewFinance')).toBe(true)
    expect(hasCapability([...crewLead], 'viewFinance')).toBe(false)
    expect(hasCapability([...routeManager], 'viewFinance')).toBe(false)
    expect(hasCapability([...worker], 'viewFinance')).toBe(false)
  })

  it('only accountant/admin approve taxi expenses', () => {
    expect(hasCapability([...accountant], 'approveTaxiExpense')).toBe(true)
    expect(hasCapability([...admin], 'approveTaxiExpense')).toBe(true)
    expect(hasCapability([...crewLead], 'approveTaxiExpense')).toBe(false)
    expect(hasCapability([...driverLead], 'approveTaxiExpense')).toBe(false)
  })

  // Releasing a member's money mirrors PayoutLineController's RoleAuthorize: a lead
  // confirms the sheet but never settles cash.
  it('only accountant/admin settle a member payout', () => {
    expect(hasCapability([...accountant], 'payMember')).toBe(true)
    expect(hasCapability([...admin], 'payMember')).toBe(true)
    expect(hasCapability([...crewLead], 'payMember')).toBe(false)
    expect(hasCapability([...driverLead], 'payMember')).toBe(false)
    expect(hasCapability([...routeManager], 'payMember')).toBe(false)
  })
})
