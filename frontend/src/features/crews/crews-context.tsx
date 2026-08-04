import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useMemo } from 'react'
import type { ReactNode } from 'react'

import { apiClient } from '@/lib/api-client'
import { nestedLargePage } from '@/lib/pagination'
import type { Crew } from '@/lib/domain-types'

// The backend models "driver-lead XOR manager-lead" as two separate nullable FKs on
// Crew, rather than the frontend's single leadType+leadId pair — translated here.
interface CrewApiDto {
  id: number
  name: string
  routeId: number
  driverLeadId: number | null
  crewLeadId: number | null
  seatCapacity: number
}

interface PaginatedResult<T> {
  items: T[]
  totalCount: number
}

export interface CrewDraft {
  name: string
  routeId: number
  driverLeadId: number | null
  crewLeadId: number | null
  seatCapacity: number
}

const CREWS_QUERY_KEY = ['crews']

function toCrew(dto: CrewApiDto): Crew {
  return {
    id: dto.id,
    name: dto.name,
    routeId: dto.routeId,
    driverLeadId: dto.driverLeadId,
    crewLeadId: dto.crewLeadId,
    seatCapacity: dto.seatCapacity,
  }
}

async function fetchCrews() {
  const response = await apiClient.get<PaginatedResult<CrewApiDto>>(
    '/api/Crew/GetAll',
    {
      params: nestedLargePage,
    },
  )

  return response.data.items.map(toCrew)
}

interface CrewsContextValue {
  crews: Crew[]
  isLoading: boolean
  addCrew: (draft: CrewDraft) => Promise<void>
  updateCrew: (crewId: number, draft: CrewDraft) => Promise<void>
  deleteCrew: (crewId: number) => Promise<void>
  getCrewById: (crewId: number | null | undefined) => Crew | undefined
}

const CrewsContext = createContext<CrewsContextValue | null>(null)

export function CrewsProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()

  const { data: crews = [], isLoading } = useQuery({
    queryKey: CREWS_QUERY_KEY,
    queryFn: fetchCrews,
  })

  function invalidate() {
    return queryClient.invalidateQueries({ queryKey: CREWS_QUERY_KEY })
  }

  const addMutation = useMutation({
    mutationFn: (draft: CrewDraft) =>
      apiClient.post('/api/Crew/Create', {
        name: draft.name,
        routeId: Number(draft.routeId),
        driverLeadId: draft.driverLeadId,
        crewLeadId: draft.crewLeadId,
        seatCapacity: draft.seatCapacity,
      }),
    onSuccess: invalidate,
  })

  const updateMutation = useMutation({
    // Note: Update's field name for the driver-lead FK is `driverId`, not
    // `driverLeadId` like Create — an inconsistency in the backend's own commands.
    mutationFn: ({ crewId, draft }: { crewId: number; draft: CrewDraft }) =>
      apiClient.put(`/api/Crew/Update/${crewId}`, {
        name: draft.name,
        routeId: Number(draft.routeId),
        seatCapacity: draft.seatCapacity,
        driverLeadId: draft.driverLeadId,
        crewLeadId: draft.crewLeadId,
      }),
    onSuccess: invalidate,
  })

  const deleteMutation = useMutation({
    mutationFn: (crewId: number) =>
      apiClient.delete(`/api/Crew/Delete/${crewId}`),
    onSuccess: invalidate,
  })

  const value = useMemo<CrewsContextValue>(
    () => ({
      crews,
      isLoading,
      addCrew: async (draft) => {
        await addMutation.mutateAsync(draft)
      },
      updateCrew: async (crewId, draft) => {
        await updateMutation.mutateAsync({ crewId, draft })
      },
      deleteCrew: async (crewId) => {
        await deleteMutation.mutateAsync(crewId)
      },
      getCrewById: (crewId) => crews.find((crew) => crew.id === crewId),
    }),
    [crews, isLoading, addMutation, updateMutation, deleteMutation],
  )

  return <CrewsContext.Provider value={value}>{children}</CrewsContext.Provider>
}

export function useCrews() {
  const context = useContext(CrewsContext)

  if (!context) {
    throw new Error('useCrews must be used inside CrewsProvider')
  }

  return context
}
