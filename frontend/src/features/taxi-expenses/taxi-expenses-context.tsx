import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useMemo } from 'react'
import type { ReactNode } from 'react'

import { apiClient } from '@/lib/api-client'
import type { Leg, TaxiExpense, TaxiExpenseStatus } from '@/lib/domain-types'
import { nestedLargePage } from '@/lib/pagination'

// Leg/TaxiExpenseStatus have explicit backend int values (Leg: Morning=1, Afternoon=2;
// TaxiExpenseStatus: Pending=1, Approved=2, Rejected=3, Paid=4). Requests want the
// number, the list/detail DTO gives back the string name.
const legToApiValue: Record<Leg, number> = { Morning: 1, Afternoon: 2 }
const legFromApiValue: Record<string, Leg> = {
  Morning: 'Morning',
  Afternoon: 'Afternoon',
}

const statusToApiValue: Record<TaxiExpenseStatus, number> = {
  Pending: 1,
  Approved: 2,
  Rejected: 3,
  Paid: 4,
}
const statusFromApiValue: Record<string, TaxiExpenseStatus> = {
  Pending: 'Pending',
  Approved: 'Approved',
  Rejected: 'Rejected',
  Paid: 'Paid',
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
  transportDayId: number
  leg: Leg
  amount: number
  paidById: number
  status: TaxiExpenseStatus
}

const TAXI_EXPENSES_QUERY_KEY = ['taxi-expenses']

function toTaxiExpense(dto: TaxiExpenseApiDto): TaxiExpense {
  return {
    id: dto.id,
    transportDayId: dto.transportDayId,
    leg: legFromApiValue[dto.leg] ?? 'Morning',
    amount: dto.amount,
    paidById: dto.paidById,
    taxiExpenseStatus: statusFromApiValue[dto.taxiExpenseStatus] ?? 'Pending',
  }
}

async function fetchTaxiExpenses() {
  const response = await apiClient.get<PaginatedResult<TaxiExpenseApiDto>>(
    '/api/TaxiExpense/GetAll',
    {
      params: nestedLargePage,
    },
  )

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
  getExpensesForDay: (transportDayId: number) => TaxiExpense[]
  getExpensesForDays: (transportDayIds: number[]) => TaxiExpense[]
  addTaxiExpense: (draft: TaxiExpenseDraft) => Promise<void>
  updateTaxiExpense: (
    expenseId: number,
    draft: TaxiExpenseDraft,
  ) => Promise<void>
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

  function invalidate() {
    return queryClient.invalidateQueries({ queryKey: TAXI_EXPENSES_QUERY_KEY })
  }

  const addMutation = useMutation({
    mutationFn: (draft: TaxiExpenseDraft) =>
      apiClient.post('/api/TaxiExpense/Create', toApiPayload(draft)),
    onSuccess: invalidate,
  })

  const updateMutation = useMutation({
    // UpdateTaxiExpenseRequest is bound [FromQuery] on the backend despite being a
    // PUT — send the full payload as query params, not a JSON body.
    mutationFn: ({
      expenseId,
      draft,
    }: {
      expenseId: number
      draft: TaxiExpenseDraft
    }) =>
      apiClient.put(`/api/TaxiExpense/Update/${expenseId}`, undefined, {
        params: toApiPayload(draft),
      }),
    onSuccess: invalidate,
  })

  const deleteMutation = useMutation({
    mutationFn: (expenseId: number) =>
      apiClient.delete(`/api/TaxiExpense/Delete/${expenseId}`),
    onSuccess: invalidate,
  })

  const approveMutation = useMutation({
    mutationFn: (expenseId: number) =>
      apiClient.put(`/api/TaxiExpense/Approve/${expenseId}/approve`),
    onSuccess: invalidate,
  })

  const rejectMutation = useMutation({
    mutationFn: (expenseId: number) =>
      apiClient.put(`/api/TaxiExpense/Reject/${expenseId}/reject`),
    onSuccess: invalidate,
  })

  const value = useMemo<TaxiExpensesContextValue>(
    () => ({
      taxiExpenses,
      isLoading,
      getExpensesForDay: (transportDayId) =>
        taxiExpenses.filter(
          (expense) => expense.transportDayId === transportDayId,
        ),
      getExpensesForDays: (transportDayIds) => {
        const dayIdSet = new Set(transportDayIds)
        return taxiExpenses.filter((expense) =>
          dayIdSet.has(expense.transportDayId),
        )
      },
      addTaxiExpense: async (draft) => {
        await addMutation.mutateAsync(draft)
      },
      updateTaxiExpense: async (expenseId, draft) => {
        await updateMutation.mutateAsync({ expenseId, draft })
      },
      deleteTaxiExpense: async (expenseId) => {
        await deleteMutation.mutateAsync(expenseId)
      },
      approveTaxiExpense: async (expenseId) => {
        await approveMutation.mutateAsync(expenseId)
      },
      rejectTaxiExpense: async (expenseId) => {
        await rejectMutation.mutateAsync(expenseId)
      },
    }),
    [
      taxiExpenses,
      isLoading,
      addMutation,
      updateMutation,
      deleteMutation,
      approveMutation,
      rejectMutation,
    ],
  )

  return (
    <TaxiExpensesContext.Provider value={value}>
      {children}
    </TaxiExpensesContext.Provider>
  )
}

export function useTaxiExpenses() {
  const context = useContext(TaxiExpensesContext)

  if (!context) {
    throw new Error('useTaxiExpenses must be used inside TaxiExpensesProvider')
  }

  return context
}
