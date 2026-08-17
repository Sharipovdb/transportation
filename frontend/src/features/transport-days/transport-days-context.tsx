import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useMemo } from 'react'
import type { ReactNode } from 'react'

import { apiClient } from '@/lib/api-client'
import type {
  Leg,
  TaxiFare,
  TransportDay,
  TransportMode,
  TaxiExpenseStatus,
} from '@/lib/domain-types'
import { getMonthYear } from '@/lib/format'
import { nestedLargePage } from '@/lib/pagination'

import { useAuth } from '@/features/auth/auth-context'
import type { AuthSession } from '@/lib/app-types'

// TransportMode has no explicit int values on the backend (Driven=0, Taxi=1, None=2 by
// ordinal) — request bodies want the number. Response DTOs give back the enum's own
// PascalCase name, which already matches this union, so no reverse map is needed.
const modeToApiValue: Record<TransportMode, number> = {
  Driven: 0,
  Taxi: 1,
  None: 2,
}

// Leg does have explicit backend values (Morning=1, Afternoon=2).
const legToApiValue: Record<Leg, number> = { Morning: 1, Afternoon: 2 }

interface TaxiExpenseApiDto {
  id: number
  transportDayId: number
  leg: Leg
  amount: number
  paidById: number
  taxiExpenseStatus: TaxiExpenseStatus
}

interface TransportDayApiDto {
  id: number
  crewId: number
  date: string
  morningMode: TransportMode
  afternoonMode: TransportMode | null
  driverId: number | null
  commuteKmPerLeg: number
  drivenCommuteKm: number
  extraBusinessKm: number
  notes: string | null
  loggedBy: number
  loggedAt: string
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
// Taxi must come with its fare, which is what turns it into a taxi expense.
export interface TransportDayDraft {
  crewId: number
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
    id: dto.id,
    crewId: dto.crewId,
    date: dto.date.slice(0, 10),
    morningMode: dto.morningMode,
    afternoonMode: dto.afternoonMode,
    driverId: dto.driverId,
    totalCommuteKm: dto.commuteKmPerLeg,
    drivenKm: dto.drivenCommuteKm,
    extraBusinessKm: dto.extraBusinessKm,
    notes: dto.notes ?? '',
    loggedBy: dto.loggedBy,
    loggedAt: dto.loggedAt,
    confirmed: dto.confirmed,
    taxiExpenses: dto.taxiExpenses.map((expense) => ({
      id: expense.id,
      transportDayId: expense.transportDayId,
      paidById: expense.paidById,
      leg: expense.leg,
      amount: expense.amount,
      taxiExpenseStatus: expense.taxiExpenseStatus,
    })),
  }
}

function toTaxiFarePayload(fares: TaxiFare[]) {
  return fares.map((fare) => ({
    leg: legToApiValue[fare.leg],
    amount: fare.amount,
    paidById: fare.paidById,
  }))
}

async function fetchTransportDays(session: AuthSession | null) {
  const isAdmin =
    session?.roles.includes('Admin') ||
    session?.roles.includes('Accountant') ||
    session?.roles.includes('RouteManager')

  const endpoint = isAdmin
    ? '/api/TransportDays/GetAll'
    : '/api/TransportDays/GetByByCurrentUserId'

  const response = await apiClient.get<PaginatedResult<TransportDayApiDto>>(
    endpoint,
    {
      params: nestedLargePage,
    },
  )

  return response.data.items.map(toTransportDay)
}

interface TransportDaysContextValue {
  transportDays: TransportDay[]
  isLoading: boolean
  getDayById: (dayId: number | null | undefined) => TransportDay | undefined
  getDaysForCrewMonth: (
    crewId: number,
    year: number,
    month: number,
  ) => TransportDay[]
  addTransportDay: (draft: TransportDayDraft) => Promise<void>
  updateTransportDay: (
    dayId: number,
    draft: Omit<TransportDayDraft, 'crewId' | 'date'>,
  ) => Promise<void>
  deleteTransportDay: (dayId: number) => Promise<void>
  confirmTransportDay: (dayId: number) => Promise<void>
  unconfirmTransportDay: (dayId: number) => Promise<void>
}

const TransportDaysContext = createContext<TransportDaysContextValue | null>(
  null,
)

export function TransportDaysProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()

  const { session } = useAuth()

  const { data: transportDays = [], isLoading } = useQuery({
    queryKey: [TRANSPORT_DAYS_QUERY_KEY, session],
    queryFn: () => fetchTransportDays(session),
  })

  // Taxi expenses are written through the transport day, so anything that touches a day
  // can change them too — both caches are refreshed together.
  function invalidate() {
    return Promise.all([
      queryClient.invalidateQueries({
        queryKey: [TRANSPORT_DAYS_QUERY_KEY, session],
      }),
      queryClient.invalidateQueries({ queryKey: ['taxi-expenses'] }),
      queryClient.invalidateQueries({ queryKey: ['monthly-sheets'] }),
    ])
  }

  const addMutation = useMutation({
    mutationFn: (draft: TransportDayDraft) =>
      apiClient.post('/api/TransportDays/Create', {
        crewId: draft.crewId,
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
      dayId: number
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
    mutationFn: (dayId: number) =>
      apiClient.delete(`/api/TransportDays/Delete/${dayId}`),
    onSuccess: invalidate,
  })

  const confirmMutation = useMutation({
    mutationFn: (dayId: number) =>
      apiClient.post(`/api/TransportDays/Confirm/${dayId}/confirm`),
    onSuccess: invalidate,
  })

  const unconfirmMutation = useMutation({
    mutationFn: (dayId: number) =>
      apiClient.post(`/api/TransportDays/UnConfirm/${dayId}/unconfirm`),
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
      addTransportDay: async (draft) => {
        await addMutation.mutateAsync(draft)
      },
      updateTransportDay: async (dayId, draft) => {
        await updateMutation.mutateAsync({ dayId, draft })
      },
      deleteTransportDay: async (dayId) => {
        await deleteMutation.mutateAsync(dayId)
      },
      confirmTransportDay: async (dayId) => {
        await confirmMutation.mutateAsync(dayId)
      },
      unconfirmTransportDay: async (dayId) => {
        await unconfirmMutation.mutateAsync(dayId)
      },
    }),
    [
      transportDays,
      isLoading,
      addMutation,
      updateMutation,
      deleteMutation,
      confirmMutation,
      unconfirmMutation,
    ],
  )

  return (
    <TransportDaysContext.Provider value={value}>
      {children}
    </TransportDaysContext.Provider>
  )
}

export function useTransportDays() {
  const context = useContext(TransportDaysContext)

  if (!context) {
    throw new Error(
      'useTransportDays must be used inside TransportDaysProvider',
    )
  }

  return context
}
