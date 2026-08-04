import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useMemo } from 'react'
import type { ReactNode } from 'react'

import { apiClient } from '@/lib/api-client'
import { nestedLargePage } from '@/lib/pagination'
import type { CrewMembership } from '@/lib/domain-types'

interface CrewMembershipApiDto {
  id: number
  crewId: number
  userId: number
  activeFrom: string
  activeTo: string | null
}

interface PaginatedResult<T> {
  items: T[]
  totalCount: number
}

const CREW_MEMBERSHIPS_QUERY_KEY = ['crew-memberships']

// The backend has no `isActive` flag — it's derived from ActiveTo the same way the
// UI reasons about it: no end date, or an end date still in the future.
function isMembershipActive(activeTo: string | null) {
  return activeTo === null || new Date(activeTo) > new Date()
}

function toCrewMembership(dto: CrewMembershipApiDto): CrewMembership {
  return {
    id: String(dto.id),
    crewId: String(dto.crewId),
    employeeId: String(dto.userId),
    activeFrom: dto.activeFrom,
    activeTo: dto.activeTo,
    isActive: isMembershipActive(dto.activeTo),
  }
}

async function fetchCrewMemberships() {
  const response = await apiClient.get<PaginatedResult<CrewMembershipApiDto>>(
    '/api/CrewMembership/GetAll',
    {
      params: nestedLargePage,
    },
  )

  return response.data.items.map(toCrewMembership)
}

interface CrewMembershipsContextValue {
  memberships: CrewMembership[]
  isLoading: boolean
  getActiveMembersForCrew: (crewId: number) => CrewMembership[]
  getActiveMembershipForEmployee: (
    employeeId: string,
  ) => CrewMembership | undefined
  getMembershipHistoryForCrew: (crewId: string) => CrewMembership[]
  assignMember: (crewId: number, employeeId: number) => Promise<boolean>
  removeMember: (membershipId: number, activeTo: string) => Promise<void>
  transferMember: (
    employeeId: number,
    fromCrewId: string,
    toCrewId: string,
  ) => Promise<void>
}

const CrewMembershipsContext =
  createContext<CrewMembershipsContextValue | null>(null)

export function CrewMembershipsProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()

  const { data: memberships = [], isLoading } = useQuery({
    queryKey: CREW_MEMBERSHIPS_QUERY_KEY,
    queryFn: fetchCrewMemberships,
  })

  function invalidate() {
    return queryClient.invalidateQueries({
      queryKey: CREW_MEMBERSHIPS_QUERY_KEY,
    })
  }

  const createMutation = useMutation({
    mutationFn: ({
      crewId,
      employeeId,
    }: {
      crewId: string
      employeeId: string
    }) =>
      apiClient.post('/api/CrewMembership/Create', {
        crewId: Number(crewId),
        userId: Number(employeeId),
      }),
    onSuccess: invalidate,
  })

  const updateMutation = useMutation({
    mutationFn: ({
      membershipId,
      activeTo,
    }: {
      membershipId: string
      activeTo: string
    }) =>
      apiClient.put(`/api/CrewMembership/Update/${membershipId}`, { activeTo }),
    onSuccess: invalidate,
  })

  // The backend's Transfer hardcodes both cutover dates to "now" server-side — no
  // backdating or scheduling a future transfer, so it takes no date argument.
  const transferMutation = useMutation({
    mutationFn: ({
      employeeId,
      fromCrewId,
      toCrewId,
    }: {
      employeeId: string
      fromCrewId: string
      toCrewId: string
    }) =>
      apiClient.put('/api/CrewMembership/Transfer', {
        userId: Number(employeeId),
        oldCrewId: Number(fromCrewId),
        newCrewId: Number(toCrewId),
      }),
    onSuccess: invalidate,
  })

  const value = useMemo<CrewMembershipsContextValue>(
    () => ({
      memberships,
      isLoading,
      getActiveMembersForCrew: (crewId) =>
        memberships.filter(
          (membership) => membership.crewId === crewId && membership.isActive,
        ),
      getActiveMembershipForEmployee: (employeeId) =>
        memberships.find(
          (membership) =>
            membership.employeeId === employeeId && membership.isActive,
        ),
      getMembershipHistoryForCrew: (crewId) =>
        memberships.filter((membership) => membership.crewId === crewId),
      assignMember: async (crewId, employeeId) => {
        const alreadyActiveElsewhere = memberships.some(
          (membership) =>
            membership.employeeId === employeeId && membership.isActive,
        )

        if (alreadyActiveElsewhere) {
          return false
        }

        await createMutation.mutateAsync({ crewId, employeeId })
        return true
      },
      removeMember: async (membershipId, activeTo) => {
        await updateMutation.mutateAsync({ membershipId, activeTo })
      },
      transferMember: async (employeeId, fromCrewId, toCrewId) => {
        await transferMutation.mutateAsync({ employeeId, fromCrewId, toCrewId })
      },
    }),
    [memberships, isLoading, createMutation, updateMutation, transferMutation],
  )

  return (
    <CrewMembershipsContext.Provider value={value}>
      {children}
    </CrewMembershipsContext.Provider>
  )
}

export function useCrewMemberships() {
  const context = useContext(CrewMembershipsContext)

  if (!context) {
    throw new Error(
      'useCrewMemberships must be used inside CrewMembershipsProvider',
    )
  }

  return context
}
