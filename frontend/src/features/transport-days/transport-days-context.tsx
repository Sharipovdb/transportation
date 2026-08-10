import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useMemo } from 'react'
import type { ReactNode } from 'react'

import { apiClient } from '@/lib/api-client'
import type {
  TransportDay,
  TransportMode,
  TaxiExpense,
} from '@/lib/domain-types'
import { getMonthYear } from '@/lib/format'
import { nestedLargePage } from '@/lib/pagination'

// TransportMode has no explicit int values on the backend (Driven=0, Taxi=1, None=2 by
// ordinal) — request bodies want the number, response DTOs give back the string name.
const modeToApiValue: Record<TransportMode, number> = {
  Driven: 0,
  Taxi: 1,
  None: 2,
}
const modeFromApiValue: Record<string, TransportMode> = {
  Driven: 'Driven',
  Taxi: 'Taxi',
  None: 'None',
}

interface TransportDayApiDto {
  id: number
  crewId: number
  date: string
  morningMode: string
  afternoonMode: string | null
  driverId: number | null
  totalCommuteKm: number
  extraBusinessKm: number
  notes: string | null
  loggedBy: number
  loggedAt: string
  confirmed: boolean
  taxiExpenses: TaxiExpense[]
}

interface PaginatedResult<T> {
  items: T[]
  totalCount: number
}

// The driver is derived server-side from the crew's Driver-Lead at creation time —
// there's nothing to send for it (see Backend's CreateTransportDayCommandHandler).
export interface TransportDayDraft {
  crewId: number
  date: string
  morningMode: TransportMode
  afternoonMode: TransportMode | null
  extraBusinessKm: number | null
  notes: string | null
}

const TRANSPORT_DAYS_QUERY_KEY = ['transport-days']

function toTransportDay(dto: TransportDayApiDto): TransportDay {
  return {
    id: dto.id,
    crewId: dto.crewId,
    date: dto.date.slice(0, 10),
    morningMode: modeFromApiValue[dto.morningMode] ?? 'none',
    afternoonMode: dto.afternoonMode
      ? (modeFromApiValue[dto.afternoonMode] ?? 'None')
      : 'None',
    driverId: dto.driverId === null ? null : dto.driverId,
    totalCommuteKm: dto.totalCommuteKm,
    extraBusinessKm: dto.extraBusinessKm,
    notes: dto.notes ?? '',
    loggedBy: dto.loggedBy,
    loggedAt: dto.loggedAt,
    confirmed: dto.confirmed,
    taxiExpenses: dto.taxiExpenses,
  }
}

async function fetchTransportDays() {
  const response = await apiClient.get<PaginatedResult<TransportDayApiDto>>(
    '/api/TransportDays/GetAll',
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

  const { data: transportDays = [], isLoading } = useQuery({
    queryKey: TRANSPORT_DAYS_QUERY_KEY,
    queryFn: fetchTransportDays,
  })

  function invalidate() {
    return queryClient.invalidateQueries({ queryKey: TRANSPORT_DAYS_QUERY_KEY })
  }

  const addMutation = useMutation({
    mutationFn: (draft: TransportDayDraft) =>
      apiClient.post('/api/TransportDays/Create', {
        crewId: Number(draft.crewId),
        date: draft.date,
        morningMode: modeToApiValue[draft.morningMode],
        afternoonMode: modeToApiValue[draft.afternoonMode ?? 'None'],
        extraBusinessCm: draft.extraBusinessKm,
        notes: draft.notes,
      }),
    onSuccess: invalidate,
  })

  const updateMutation = useMutation({
    // Update can't move a day to a different crew/date, and the driver stays
    // derived from the crew — only the modes, extra km, and notes are editable here.
    mutationFn: ({
      dayId,
      draft,
    }: {
      dayId: number
      draft: Omit<TransportDayDraft, 'crewId' | 'date'>
    }) =>
      apiClient.put(`/api/TransportDays/Update/${dayId}`, {
        morningMode: modeToApiValue[draft.morningMode],
        afternoonMode: modeToApiValue[draft.afternoonMode ?? 'None'],
        extraBusinessKm: draft.extraBusinessKm,
        notes: draft.notes,
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
