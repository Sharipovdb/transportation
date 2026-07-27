import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useMemo } from 'react'
import type { ReactNode } from 'react'

import { apiClient } from '@/lib/api-client'
import type { Leg, TaxiExpense, TaxiExpenseStatus } from '@/lib/domain-types'
import { nestedLargePage } from '@/lib/pagination'

// Leg/TaxiExpenseStatus have explicit backend int values (Leg: Morning=1, Afternoon=2;
// TaxiExpenseStatus: Pending=1, Approved=2, Rejected=3, Paid=4). Requests want the
// number, the list/detail DTO gives back the string name.
const legToApiValue: Record<Leg, number> = { morning: 1, afternoon: 2 }
const legFromApiValue: Record<string, Leg> = { Morning: 'morning', Afternoon: 'afternoon' }

const statusToApiValue: Record<TaxiExpenseStatus, number> = {
  pending: 1,
  approved: 2,
  rejected: 3,
  paid: 4,
}
const statusFromApiValue: Record<string, TaxiExpenseStatus> = {
  Pending: 'pending',
  Approved: 'approved',
  Rejected: 'rejected',
  Paid: 'paid',
}

interface TaxiExpenseApiDto {
  id: number
  transportDayId: number
  paidById: number
  leg: string
  amount: number
  taxiExpenseStatus: string
}

interface PaginatedResult<T> {
  items: T[]
  totalCount: number
}

export interface TaxiExpenseDraft {
  transportDayId: string
  leg: Leg
  amount: number
  paidById: string
  status: TaxiExpenseStatus
}

const TAXI_EXPENSES_QUERY_KEY = ['taxi-expenses']

function toTaxiExpense(dto: TaxiExpenseApiDto): TaxiExpense {
  return {
    id: String(dto.id),
    transportDayId: String(dto.transportDayId),
    leg: legFromApiValue[dto.leg] ?? 'morning',
    amount: dto.amount,
    paidById: String(dto.paidById),
    status: statusFromApiValue[dto.taxiExpenseStatus] ?? 'pending',
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
    transportDayId: Number(draft.transportDayId),
    leg: legToApiValue[draft.leg],
    amount: draft.amount,
    paidById: Number(draft.paidById),
    taxiExpenseStatus: statusToApiValue[draft.status],
  }
}

interface TaxiExpensesContextValue {
  taxiExpenses: TaxiExpense[]
  isLoading: boolean
  getExpensesForDay: (transportDayId: string) => TaxiExpense[]
  getExpensesForDays: (transportDayIds: string[]) => TaxiExpense[]
  addTaxiExpense: (draft: TaxiExpenseDraft) => Promise<void>
  updateTaxiExpense: (expenseId: string, draft: TaxiExpenseDraft) => Promise<void>
  deleteTaxiExpense: (expenseId: string) => Promise<void>
  approveTaxiExpense: (expenseId: string) => Promise<void>
  rejectTaxiExpense: (expenseId: string) => Promise<void>
}

const TaxiExpensesContext = createContext<TaxiExpensesContextValue | null>(null)

export function TaxiExpensesProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()

  const { data: taxiExpenses = [], isLoading } = useQuery({
    queryKey: TAXI_EXPENSES_QUERY_KEY,
    queryFn: fetchTaxiExpenses,
  })

  function invalidate() {
    return queryClient.invalidateQueries({ queryKey: TAXI_EXPENSES_QUERY_KEY })
  }

  const addMutation = useMutation({
    mutationFn: (draft: TaxiExpenseDraft) => apiClient.post('/api/TaxiExpense/Create', toApiPayload(draft)),
    onSuccess: invalidate,
  })

  const updateMutation = useMutation({
    // UpdateTaxiExpenseRequest is bound [FromQuery] on the backend despite being a
    // PUT — send the full payload as query params, not a JSON body.
    mutationFn: ({ expenseId, draft }: { expenseId: string; draft: TaxiExpenseDraft }) =>
      apiClient.put(`/api/TaxiExpense/Update/${expenseId}`, undefined, { params: toApiPayload(draft) }),
    onSuccess: invalidate,
  })

  const deleteMutation = useMutation({
    mutationFn: (expenseId: string) => apiClient.delete(`/api/TaxiExpense/Delete/${expenseId}`),
    onSuccess: invalidate,
  })

  const approveMutation = useMutation({
    mutationFn: (expenseId: string) => apiClient.put(`/api/TaxiExpense/Approve/${expenseId}`),
    onSuccess: invalidate,
  })

  const rejectMutation = useMutation({
    mutationFn: (expenseId: string) => apiClient.put(`/api/TaxiExpense/Reject/${expenseId}`),
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
      addTaxiExpense: async (draft) => { await addMutation.mutateAsync(draft) },
      updateTaxiExpense: async (expenseId, draft) => { await updateMutation.mutateAsync({ expenseId, draft }) },
      deleteTaxiExpense: async (expenseId) => { await deleteMutation.mutateAsync(expenseId) },
      approveTaxiExpense: async (expenseId) => { await approveMutation.mutateAsync(expenseId) },
      rejectTaxiExpense: async (expenseId) => { await rejectMutation.mutateAsync(expenseId) },
    }),
    [taxiExpenses, isLoading, addMutation, updateMutation, deleteMutation, approveMutation, rejectMutation],
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
