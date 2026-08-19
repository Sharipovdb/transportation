import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useMemo } from 'react'
import type { ReactNode } from 'react'

import { apiClient } from '@/lib/api-client'
import type { MonthlySheet } from '@/lib/domain-types'
import { nestedLargePage } from '@/lib/pagination'

// The API shape matches MonthlyTransportSheetDto one-to-one; only the date is narrowed
// to the yyyy-mm-dd the rest of the app uses.
interface MonthlySheetDayApiDto {
  transportDayId: number
  date: string
  drivenKm: number
  extraBusinessKm: number
  taxiAmount: number
}

interface MonthlySheetApiDto {
  id: number
  crewId: number
  year: number
  month: number
  recipientId: number
  recipientFullname: string
  isConfirmed: boolean
  isPaid: boolean
  paidAt: string | null
  days: MonthlySheetDayApiDto[]
  totalDrivenKm: number
  totalExtraBusinessKm: number
  totalTaxiAmount: number
}

interface PaginatedResult<T> {
  items: T[]
  totalCount: number
}

const MONTHLY_SHEETS_QUERY_KEY = ['monthly-sheets']

function toMonthlySheet(dto: MonthlySheetApiDto): MonthlySheet {
  return {
    ...dto,
    days: dto.days.map((day) => ({ ...day, date: day.date.slice(0, 10) })),
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

async function fetchPreview(period: SheetPeriod) {
  const response = await apiClient.post<MonthlySheetApiDto>(
    '/api/MonthlyTransportSheets/GetPreview',
    period,
  )

  return toMonthlySheet(response.data)
}

/**
 * What generating this period would produce, computed server-side and saved nowhere.
 * Passing `null` disables it — a month that already has a saved sheet reads that sheet
 * instead, and the record must never be second-guessed by a recalculation.
 *
 * It is keyed under the sheets key so generating, confirming or deleting a sheet
 * refreshes the preview with everything else.
 */
export function useMonthlySheetPreview(period: SheetPeriod | null) {
  return useQuery({
    queryKey: [...MONTHLY_SHEETS_QUERY_KEY, 'preview', period],
    queryFn: () => fetchPreview(period!),
    enabled: period !== null,
    retry: false,
  })
}

interface MonthlySheetsContextValue {
  sheets: MonthlySheet[]
  isLoading: boolean
  getSheet: (period: SheetPeriod) => MonthlySheet | undefined
  generateSheet: (period: SheetPeriod) => Promise<MonthlySheet>
  confirmSheet: (sheetId: number) => Promise<void>
  unconfirmSheet: (sheetId: number) => Promise<void>
  markSheetPaid: (sheetId: number) => Promise<void>
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

  // Paying a sheet moves the taxi fares behind it on to Paid, so the expense list is
  // stale afterwards too.
  function invalidate() {
    return Promise.all([
      queryClient.invalidateQueries({ queryKey: MONTHLY_SHEETS_QUERY_KEY }),
      queryClient.invalidateQueries({ queryKey: ['taxi-expenses'] }),
    ])
  }

  const generateMutation = useMutation({
    mutationFn: async (period: SheetPeriod) => {
      const response = await apiClient.post<MonthlySheetApiDto>(
        '/api/MonthlyTransportSheets/Generate',
        period,
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

  // Confirming is reversible right up to payment, so the month can be reopened, fixed
  // and recalculated instead of being deleted and rebuilt from scratch.
  const unconfirmMutation = useMutation({
    mutationFn: (sheetId: number) =>
      apiClient.put(`/api/MonthlyTransportSheets/Unconfirm/${sheetId}`),
    onSuccess: invalidate,
  })

  // Settling a crew closes its month; the backend refuses a second call, so the
  // refetched sheet is the source of truth for the button's state.
  const markPaidMutation = useMutation({
    mutationFn: (sheetId: number) =>
      apiClient.put(`/api/MonthlyTransportSheets/MarkPaid/${sheetId}`),
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
      generateSheet: (period) => generateMutation.mutateAsync(period),
      confirmSheet: async (sheetId) => {
        await confirmMutation.mutateAsync(sheetId)
      },
      unconfirmSheet: async (sheetId) => {
        await unconfirmMutation.mutateAsync(sheetId)
      },
      markSheetPaid: async (sheetId) => {
        await markPaidMutation.mutateAsync(sheetId)
      },
      deleteSheet: async (sheetId) => {
        await deleteMutation.mutateAsync(sheetId)
      },
    }),
    [
      sheets,
      isLoading,
      generateMutation,
      confirmMutation,
      unconfirmMutation,
      markPaidMutation,
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
