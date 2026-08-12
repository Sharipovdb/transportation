import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useMemo } from 'react'
import type { ReactNode } from 'react'

import { apiClient } from '@/lib/api-client'
import type { Leg, TaxiExpense, TaxiExpenseStatus } from '@/lib/domain-types'
import { nestedLargePage } from '@/lib/pagination'

// Leg/TaxiExpenseStatus have explicit backend int values (Leg: Morning=1, Afternoon=2;
// TaxiExpenseStatus: Pending=1, Approved=2, Rejected=3, Paid=4) — requests want the
// number. The list/detail DTO gives back the enum's own PascalCase name, which already
// matches these unions, so no reverse map is needed on the read side.
const legToApiValue: Record<Leg, number> = { Morning: 1, Afternoon: 2 }

const statusToApiValue: Record<TaxiExpenseStatus, number> = {
  Pending: 1,
  Approved: 2,
  Rejected: 3,
  Paid: 4,
}

interface TaxiExpenseApiDto {
  id: number
  transportDayId: number
  paidById: number
  leg: Leg
  amount: number
  taxiExpenseStatus: TaxiExpenseStatus
}

interface PaginatedResult<T> {
  items: T[]
  totalCount: number
}

// paidById stays a string here: it comes straight from a <select> element's raw form
// value and is only turned into a number at the API boundary, in toApiPayload.
export interface TaxiExpenseDraft {
  transportDayId: number
  leg: Leg
  amount: number
  paidById: string
  taxiExpenseStatus: TaxiExpenseStatus
}

const TAXI_EXPENSES_QUERY_KEY = ['taxi-expenses']

function toTaxiExpense(dto: TaxiExpenseApiDto): TaxiExpense {
  return {
    id: dto.id,
    transportDayId: dto.transportDayId,
    leg: dto.leg,
    amount: dto.amount,
    paidById: dto.paidById,
    taxiExpenseStatus: dto.taxiExpenseStatus,
  }
}

async function fetchTaxiExpenses() {
  const response = await apiClient.get<PaginatedResult<TaxiExpenseApiDto>>('/api/TaxiExpense/GetAll', {
    params: nestedLargePage,
  })

  return response.data.items.map(toTaxiExpense)
}

function toApiPayload(draft: TaxiExpenseDraft) {
  return {
    transportDayId: draft.transportDayId,
    leg: legToApiValue[draft.leg],
    amount: draft.amount,
    paidById: Number(draft.paidById),
    taxiExpenseStatus: statusToApiValue[draft.taxiExpenseStatus],
  }
}

// Creating an expense is not part of this contract on purpose: a taxi ride is recorded
// on the transport day it belongs to (see features/transport-days), which is what keeps
// a taxi leg from ever existing without a reimbursable expense behind it.
interface TaxiExpensesContextValue {
  taxiExpenses: TaxiExpense[]
  isLoading: boolean
  getExpensesForDay: (transportDayId: number) => TaxiExpense[]
  getExpensesForDays: (transportDayIds: number[]) => TaxiExpense[]
  updateTaxiExpense: (expenseId: number, draft: TaxiExpenseDraft) => Promise<void>
  deleteTaxiExpense: (expenseId: number) => Promise<void>
  approveTaxiExpense: (expenseId: number) => Promise<void>
  rejectTaxiExpense: (expenseId: number) => Promise<void>
}

const TaxiExpensesContext = createContext<TaxiExpensesContextValue | null>(null)

export function TaxiExpensesProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()

  const { data: taxiExpenses = [], isLoading } = useQuery({
    queryKey: TAXI_EXPENSES_QUERY_KEY,
    queryFn: fetchTaxiExpenses,
  })

  // An expense belongs to a transport day and feeds a monthly sheet, so changing one
  // invalidates all three views rather than leaving the other two showing stale money.
  function invalidate() {
    return Promise.all([
      queryClient.invalidateQueries({ queryKey: TAXI_EXPENSES_QUERY_KEY }),
      queryClient.invalidateQueries({ queryKey: ['transport-days'] }),
      queryClient.invalidateQueries({ queryKey: ['monthly-sheets'] }),
    ])
  }

  const updateMutation = useMutation({
    // UpdateTaxiExpenseRequest is bound [FromQuery] on the backend despite being a
    // PUT — send the full payload as query params, not a JSON body.
    mutationFn: ({ expenseId, draft }: { expenseId: number; draft: TaxiExpenseDraft }) =>
      apiClient.put(`/api/TaxiExpense/Update/${expenseId}`, undefined, { params: toApiPayload(draft) }),
    onSuccess: invalidate,
  })

  const deleteMutation = useMutation({
    mutationFn: (expenseId: number) => apiClient.delete(`/api/TaxiExpense/Delete/${expenseId}`),
    onSuccess: invalidate,
  })

  const approveMutation = useMutation({
    mutationFn: (expenseId: number) => apiClient.put(`/api/TaxiExpense/Approve/${expenseId}`),
    onSuccess: invalidate,
  })

  const rejectMutation = useMutation({
    mutationFn: (expenseId: number) => apiClient.put(`/api/TaxiExpense/Reject/${expenseId}`),
    onSuccess: invalidate,
  })

  const value = useMemo<TaxiExpensesContextValue>(
    () => ({
      taxiExpenses,
      isLoading,
      getExpensesForDay: (transportDayId) =>
        taxiExpenses.filter((expense) => expense.transportDayId === transportDayId),
      getExpensesForDays: (transportDayIds) => {
        const dayIdSet = new Set(transportDayIds)
        return taxiExpenses.filter((expense) => dayIdSet.has(expense.transportDayId))
      },
      updateTaxiExpense: async (expenseId, draft) => { await updateMutation.mutateAsync({ expenseId, draft }) },
      deleteTaxiExpense: async (expenseId) => { await deleteMutation.mutateAsync(expenseId) },
      approveTaxiExpense: async (expenseId) => { await approveMutation.mutateAsync(expenseId) },
      rejectTaxiExpense: async (expenseId) => { await rejectMutation.mutateAsync(expenseId) },
    }),
    [taxiExpenses, isLoading, updateMutation, deleteMutation, approveMutation, rejectMutation],
  )

  return <TaxiExpensesContext.Provider value={value}>{children}</TaxiExpensesContext.Provider>
}

export function useTaxiExpenses() {
  const context = useContext(TaxiExpensesContext)

  if (!context) {
    throw new Error('useTaxiExpenses must be used inside TaxiExpensesProvider')
  }

  return context
}
