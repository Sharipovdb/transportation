import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useMemo } from 'react'
import type { ReactNode } from 'react'

import { apiClient } from '@/lib/api-client'
import type { Leg, TaxiFare, TransportDay, TransportMode } from '@/lib/domain-types'
import { getMonthYear } from '@/lib/format'
import { nestedLargePage } from '@/lib/pagination'

// TransportMode has no explicit int values on the backend (Driven=0, Taxi=1, None=2 by
// ordinal) — request bodies want the number, response DTOs give back the string name.
const modeToApiValue: Record<TransportMode, number> = { driven: 0, taxi: 1, none: 2 }
const modeFromApiValue: Record<string, TransportMode> = { Driven: 'driven', Taxi: 'taxi', None: 'none' }

// Leg does have explicit backend values (Morning=1, Afternoon=2).
const legToApiValue: Record<Leg, number> = { morning: 1, afternoon: 2 }
const legFromApiValue: Record<string, Leg> = { Morning: 'morning', Afternoon: 'afternoon' }

interface TaxiExpenseApiDto {
  id: number
  leg: string
  amount: number
  paidById: number
  taxiExpenseStatus: string
}

interface TransportDayApiDto {
  id: number
  crewId: number
  date: string
  morningMode: string
  afternoonMode: string | null
  driverId: number | null
  commuteKmPerLeg: number
  drivenCommuteKm: number
  extraBusinessKm: number
  notes: string | null
  confirmed: boolean
  taxiExpenses: TaxiExpenseApiDto[]
}

interface PaginatedResult<T> {
  items: T[]
  totalCount: number
}

// The driver and the route distance are derived server-side from the crew at creation
// time — there's nothing to send for them (see CreateTransportDayCommandHandler).
// Taxi fares, on the other hand, only the person logging the day knows: a leg marked
// `taxi` must come with its fare, which is what turns it into a taxi expense.
export interface TransportDayDraft {
  crewId: string
  date: string
  morningMode: TransportMode
  afternoonMode: TransportMode
  extraBusinessKm: number
  notes: string
  taxiFares: TaxiFare[]
}

const TRANSPORT_DAYS_QUERY_KEY = ['transport-days']

function toTransportDay(dto: TransportDayApiDto): TransportDay {
  return {
    id: String(dto.id),
    crewId: String(dto.crewId),
    date: dto.date.slice(0, 10),
    morningMode: modeFromApiValue[dto.morningMode] ?? 'none',
    afternoonMode: dto.afternoonMode ? modeFromApiValue[dto.afternoonMode] ?? 'none' : 'none',
    driverId: dto.driverId === null ? null : String(dto.driverId),
    commuteKmPerLeg: dto.commuteKmPerLeg,
    drivenKm: dto.drivenCommuteKm,
    extraBusinessKm: dto.extraBusinessKm,
    notes: dto.notes ?? '',
    confirmed: dto.confirmed,
    taxiFares: dto.taxiExpenses.map((expense) => ({
      leg: legFromApiValue[expense.leg] ?? 'morning',
      amount: expense.amount,
      paidById: String(expense.paidById),
    })),
  }
}

function toTaxiFarePayload(fares: TaxiFare[]) {
  return fares.map((fare) => ({
    leg: legToApiValue[fare.leg],
    amount: fare.amount,
    paidById: Number(fare.paidById),
  }))
}

async function fetchTransportDays() {
  const response = await apiClient.get<PaginatedResult<TransportDayApiDto>>('/api/TransportDays/GetAll', {
    params: nestedLargePage,
  })

  return response.data.items.map(toTransportDay)
}

interface TransportDaysContextValue {
  transportDays: TransportDay[]
  isLoading: boolean
  getDayById: (dayId: string | null | undefined) => TransportDay | undefined
  getDaysForCrewMonth: (crewId: string, year: number, month: number) => TransportDay[]
  addTransportDay: (draft: TransportDayDraft) => Promise<void>
  updateTransportDay: (dayId: string, draft: Omit<TransportDayDraft, 'crewId' | 'date'>) => Promise<void>
  deleteTransportDay: (dayId: string) => Promise<void>
  confirmTransportDay: (dayId: string) => Promise<void>
  unconfirmTransportDay: (dayId: string) => Promise<void>
}

const TransportDaysContext = createContext<TransportDaysContextValue | null>(null)

export function TransportDaysProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()

  const { data: transportDays = [], isLoading } = useQuery({
    queryKey: TRANSPORT_DAYS_QUERY_KEY,
    queryFn: fetchTransportDays,
  })

  // Taxi expenses are written through the transport day, so anything that touches a day
  // can change them too — both caches are refreshed together.
  function invalidate() {
    return Promise.all([
      queryClient.invalidateQueries({ queryKey: TRANSPORT_DAYS_QUERY_KEY }),
      queryClient.invalidateQueries({ queryKey: ['taxi-expenses'] }),
      queryClient.invalidateQueries({ queryKey: ['monthly-sheets'] }),
    ])
  }

  const addMutation = useMutation({
    mutationFn: (draft: TransportDayDraft) =>
      apiClient.post('/api/TransportDays/Create', {
        crewId: Number(draft.crewId),
        date: draft.date,
        morningMode: modeToApiValue[draft.morningMode],
        afternoonMode: modeToApiValue[draft.afternoonMode],
        extraBusinessKm: draft.extraBusinessKm,
        notes: draft.notes,
        taxiFares: toTaxiFarePayload(draft.taxiFares),
      }),
    onSuccess: invalidate,
  })

  const updateMutation = useMutation({
    // Update can't move a day to a different crew/date, and the driver stays derived
    // from the crew. Taxi fares are sent in full: the day's taxi expenses are replaced
    // by the list, so a leg that is no longer a taxi ride drops its expense.
    mutationFn: ({
      dayId,
      draft,
    }: {
      dayId: string
      draft: Omit<TransportDayDraft, 'crewId' | 'date'>
    }) =>
      apiClient.put(`/api/TransportDays/Update/${dayId}`, {
        morningMode: modeToApiValue[draft.morningMode],
        afternoonMode: modeToApiValue[draft.afternoonMode],
        extraBusinessKm: draft.extraBusinessKm,
        notes: draft.notes,
        taxiFares: toTaxiFarePayload(draft.taxiFares),
      }),
    onSuccess: invalidate,
  })

  const deleteMutation = useMutation({
    mutationFn: (dayId: string) => apiClient.delete(`/api/TransportDays/Delete/${dayId}`),
    onSuccess: invalidate,
  })

  const confirmMutation = useMutation({
    mutationFn: (dayId: string) => apiClient.post(`/api/TransportDays/Confirm/${dayId}/confirm`),
    onSuccess: invalidate,
  })

  const unconfirmMutation = useMutation({
    mutationFn: (dayId: string) => apiClient.post(`/api/TransportDays/UnConfirm/${dayId}/unconfirm`),
    onSuccess: invalidate,
  })

  const value = useMemo<TransportDaysContextValue>(
    () => ({
      transportDays,
      isLoading,
      getDayById: (dayId) => transportDays.find((day) => day.id === dayId),
      getDaysForCrewMonth: (crewId, year, month) =>
        transportDays
          .filter((day) => {
            if (day.crewId !== crewId) {
              return false
            }

            const period = getMonthYear(day.date)
            return period.year === year && period.month === month
          })
          .sort((first, second) => first.date.localeCompare(second.date)),
      addTransportDay: async (draft) => { await addMutation.mutateAsync(draft) },
      updateTransportDay: async (dayId, draft) => { await updateMutation.mutateAsync({ dayId, draft }) },
      deleteTransportDay: async (dayId) => { await deleteMutation.mutateAsync(dayId) },
      confirmTransportDay: async (dayId) => { await confirmMutation.mutateAsync(dayId) },
      unconfirmTransportDay: async (dayId) => { await unconfirmMutation.mutateAsync(dayId) },
    }),
    [transportDays, isLoading, addMutation, updateMutation, deleteMutation, confirmMutation, unconfirmMutation],
  )

  return <TransportDaysContext.Provider value={value}>{children}</TransportDaysContext.Provider>
}

export function useTransportDays() {
  const context = useContext(TransportDaysContext)

  if (!context) {
    throw new Error('useTransportDays must be used inside TransportDaysProvider')
  }

  return context
}
