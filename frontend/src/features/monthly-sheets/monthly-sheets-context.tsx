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
  1: 'Morning',
  2: 'Afternoon',
}
const statusFromNumericValue: Record<number, TaxiExpenseStatus> = {
  1: 'Pending',
  2: 'Approved',
  3: 'Rejected',
  4: 'Paid',
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
  fullname: string
  driverPayment: number
  extraKmPayment: number
  taxiCompensation: number
  totalAmount: number
  taxiExpenses: TaxiExpenseSummaryApiDto[]
}

interface MonthlySheetApiDto {
  id: number
  crewId: number
  year: number
  month: number
  isConfirmed: boolean
  payoutLines: PayoutLineApiDto[]
  totalAmount: number
}

interface PaginatedResult<T> {
  items: T[]
  totalCount: number
}

const MONTHLY_SHEETS_QUERY_KEY = ['monthly-sheets']

function toPayoutLine(dto: PayoutLineApiDto): PayoutLine {
  return {
    id: dto.id,
    userId: dto.userId,
    fullname: dto.fullname,
    driverPayment: dto.driverPayment,
    extraKmPayment: dto.extraKmPayment,
    taxiCompensation: dto.taxiCompensation,
    totalAmount: dto.totalAmount,
    taxiExpenses: dto.taxiExpenses.map((expense) => ({
      id: expense.id,
      amount: expense.amount,
      leg: legFromNumericValue[expense.leg] ?? 'Morning',
      taxiExpenseStatus:
        statusFromNumericValue[expense.taxiExpenseStatus] ?? 'Pending',
    })),
  }
}

function toMonthlySheet(dto: MonthlySheetApiDto): MonthlySheet {
  return {
    id: dto.id,
    crewId: dto.crewId,
    year: dto.year,
    month: dto.month,
    isConfirmed: dto.isConfirmed,
    payoutLines: dto.payoutLines.map(toPayoutLine),
    totalAmount: dto.totalAmount,
  }
}

export interface SheetPeriod {
  crewId: number
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
  confirmSheet: (sheetId: number) => Promise<void>
  deleteSheet: (sheetId: number) => Promise<void>
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
          crewId: period.crewId,
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
          crewId: period.crewId,
          year: period.year,
          month: period.month,
        },
      )

      return toMonthlySheet(response.data)
    },
    onSuccess: invalidate,
  })

  const confirmMutation = useMutation({
    mutationFn: (sheetId: number) =>
      apiClient.put(`/api/MonthlyTransportSheets/Confirm/${sheetId}`),
    onSuccess: invalidate,
  })

  const deleteMutation = useMutation({
    mutationFn: (sheetId: number) =>
      apiClient.delete(`/api/MonthlyTransportSheets/Delete/${sheetId}`),
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
    }),
    [
      sheets,
      isLoading,
      previewMutation,
      generateMutation,
      confirmMutation,
      deleteMutation,
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
