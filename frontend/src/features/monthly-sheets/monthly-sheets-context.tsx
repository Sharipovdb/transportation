import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useMemo } from 'react'
import type { ReactNode } from 'react'

import { apiClient } from '@/lib/api-client'
import type {
  Leg,
  MonthlySheet,
  PayoutLine,
  TaxiExpenseStatus,
} from '@/lib/domain-types'
import { nestedLargePage } from '@/lib/pagination'

// Matches the numeric Leg/TaxiExpenseStatus enum values as they appear nested inside
// PayoutLineDto.TaxiExpenses (see TaxiExpenseSummaryDto.cs) — numeric here even though
// the standalone TaxiExpenseController's DTO stringifies the same enums.
const legFromNumericValue: Record<number, Leg> = {
  1: 'morning',
  2: 'afternoon',
}
const statusFromNumericValue: Record<number, TaxiExpenseStatus> = {
  1: 'pending',
  2: 'approved',
  3: 'rejected',
  4: 'paid',
}

interface TaxiExpenseSummaryApiDto {
  id: number
  amount: number
  leg: number
  taxiExpenseStatus: number
}

interface PayoutLineApiDto {
  id: number
  userId: number
  firstName: string
  lastName: string
  driverKm: number
  extraBusinessKm: number
  taxiCompensation: number
  isPaid: boolean
  paidAt: string | null
  totalAmount: number
  taxiExpenses: TaxiExpenseSummaryApiDto[]
}

interface MonthlySheetApiDto {
  id?: number
  crewId: number
  year: number
  month: number
  isConfirmed: boolean
  payoutLines: PayoutLineApiDto[]
  totalDriverKm: number
  totalExtraBusinessKm: number
  totalTaxiAmount: number
  totalAmount: number
}

interface PaginatedResult<T> {
  items: T[]
  totalCount: number
}

const MONTHLY_SHEETS_QUERY_KEY = ['monthly-sheets']

function toPayoutLine(dto: PayoutLineApiDto): PayoutLine {
  return {
    id: String(dto.id),
    employeeId: String(dto.userId),
    employeeName: `${dto.firstName} ${dto.lastName}`.trim(),
    driverKm: dto.driverKm,
    extraBusinessKm: dto.extraBusinessKm,
    taxiCompensation: dto.taxiCompensation,
    isPaid: dto.isPaid,
    paidAt: dto.paidAt,
    totalAmount: dto.totalAmount,
    taxiExpenses: dto.taxiExpenses.map((expense) => ({
      id: String(expense.id),
      amount: expense.amount,
      leg: legFromNumericValue[expense.leg] ?? 'morning',
      status: statusFromNumericValue[expense.taxiExpenseStatus] ?? 'pending',
    })),
  }
}

function toMonthlySheet(dto: MonthlySheetApiDto): MonthlySheet {
  return {
    id: dto.id === undefined ? null : String(dto.id),
    crewId: String(dto.crewId),
    year: dto.year,
    month: dto.month,
    isConfirmed: dto.isConfirmed,
    payoutLines: dto.payoutLines.map(toPayoutLine),
    totalDriverKm: dto.totalDriverKm,
    totalExtraBusinessKm: dto.totalExtraBusinessKm,
    totalTaxiAmount: dto.totalTaxiAmount,
    totalAmount: dto.totalAmount,
  }
}

export interface SheetPeriod {
  crewId: string
  year: number
  month: number
}

async function fetchSheets() {
  const response = await apiClient.get<PaginatedResult<MonthlySheetApiDto>>(
    '/api/MonthlyTransportSheets/GetAll',
    { params: nestedLargePage },
  )

  return response.data.items.map(toMonthlySheet)
}

interface MonthlySheetsContextValue {
  sheets: MonthlySheet[]
  isLoading: boolean
  getSheet: (period: SheetPeriod) => MonthlySheet | undefined
  previewSheet: (period: SheetPeriod) => Promise<MonthlySheet>
  generateSheet: (period: SheetPeriod) => Promise<MonthlySheet>
  confirmSheet: (sheetId: string) => Promise<void>
  deleteSheet: (sheetId: string) => Promise<void>
  markPayoutLinePaid: (payoutLineId: string) => Promise<void>
}

const MonthlySheetsContext = createContext<MonthlySheetsContextValue | null>(
  null,
)

function isSamePeriod(sheet: MonthlySheet, period: SheetPeriod) {
  return (
    sheet.crewId === period.crewId &&
    sheet.year === period.year &&
    sheet.month === period.month
  )
}

export function MonthlySheetsProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()

  const { data: sheets = [], isLoading } = useQuery({
    queryKey: MONTHLY_SHEETS_QUERY_KEY,
    queryFn: fetchSheets,
  })

  function invalidate() {
    return queryClient.invalidateQueries({ queryKey: MONTHLY_SHEETS_QUERY_KEY })
  }

  const previewMutation = useMutation({
    mutationFn: async (period: SheetPeriod) => {
      const response = await apiClient.post<MonthlySheetApiDto>(
        '/api/MonthlyTransportSheets/GetPreview',
        {
          crewId: Number(period.crewId),
          year: period.year,
          month: period.month,
        },
      )

      return toMonthlySheet(response.data)
    },
  })

  const generateMutation = useMutation({
    mutationFn: async (period: SheetPeriod) => {
      const response = await apiClient.post<MonthlySheetApiDto>(
        '/api/MonthlyTransportSheets/Generate',
        {
          crewId: Number(period.crewId),
          year: period.year,
          month: period.month,
        },
      )

      return toMonthlySheet(response.data)
    },
    onSuccess: invalidate,
  })

  const confirmMutation = useMutation({
    mutationFn: (sheetId: string) =>
      apiClient.put(`/api/MonthlyTransportSheets/Confirm/${sheetId}`),
    onSuccess: invalidate,
  })

  const deleteMutation = useMutation({
    mutationFn: (sheetId: string) =>
      apiClient.delete(`/api/MonthlyTransportSheets/Delete/${sheetId}`),
    onSuccess: invalidate,
  })

  // Settling a member closes their line for the month; the backend refuses a second
  // call, so the refetched sheet is the source of truth for the button's state.
  const markPaidMutation = useMutation({
    mutationFn: (payoutLineId: string) =>
      apiClient.put(`/api/PayoutLine/MarkPaid/${payoutLineId}`),
    onSuccess: invalidate,
  })

  const value = useMemo<MonthlySheetsContextValue>(
    () => ({
      sheets,
      isLoading,
      getSheet: (period) => sheets.find((sheet) => isSamePeriod(sheet, period)),
      previewSheet: (period) => previewMutation.mutateAsync(period),
      generateSheet: (period) => generateMutation.mutateAsync(period),
      confirmSheet: async (sheetId) => {
        await confirmMutation.mutateAsync(sheetId)
      },
      deleteSheet: async (sheetId) => {
        await deleteMutation.mutateAsync(sheetId)
      },
      markPayoutLinePaid: async (payoutLineId) => {
        await markPaidMutation.mutateAsync(payoutLineId)
      },
    }),
    [
      sheets,
      isLoading,
      previewMutation,
      generateMutation,
      confirmMutation,
      deleteMutation,
      markPaidMutation,
    ],
  )

  return (
    <MonthlySheetsContext.Provider value={value}>
      {children}
    </MonthlySheetsContext.Provider>
  )
}

export function useMonthlySheets() {
  const context = useContext(MonthlySheetsContext)

  if (!context) {
    throw new Error(
      'useMonthlySheets must be used inside MonthlySheetsProvider',
    )
  }

  return context
}
