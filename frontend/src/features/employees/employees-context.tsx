import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useMemo } from 'react'
import type { ReactNode } from 'react'

import { apiClient } from '@/lib/api-client'
import type { ApiResponse } from '@/lib/api-client'
import { appRoleToBackendRole, resolveAppRole } from '@/lib/app-types'
import type { Employee, EmployeeRole } from '@/lib/domain-types'

// Raw shape of Transportation.Application.User.Models.UserDto — note the backend's
// own casing (`firstname`, not `firstName`) and that role membership travels
// separately from everything else (assign/remove, not a field on Update).
interface UserApiDto {
  id: number
  email: string
  username: string
  firstname: string
  lastName: string
  phoneNumber: string
  telegramId: string
  roles: string[]
}

export interface EmployeeDraft {
  firstName: string
  lastName: string
  phoneNumber: string
  telegramId: string
  role: EmployeeRole
}

const EMPLOYEES_QUERY_KEY = ['employees']

// Every account created from this page shares one password — there's no invite-email
// flow in this pass (mirrors the seeded demo accounts' convention, see
// Backend/.../Seeders/UserDatabaseSeeder.cs).
const DEFAULT_PASSWORD = 'Passw0rd!'

function toEmployee(dto: UserApiDto): Employee | null {
  const role = resolveAppRole(dto.roles)

  // The technical Admin account (and anyone with no recognized role yet) isn't a
  // business "Employee" per the PRD's role list.
  if (!role || role === 'admin') {
    return null
  }

  return {
    id: String(dto.id),
    firstName: dto.firstname,
    lastName: dto.lastName,
    phoneNumber: dto.phoneNumber,
    telegramId: dto.telegramId,
    role,
  }
}

function syntheticEmail(phoneNumber: string) {
  return `user${phoneNumber.replace(/\D/g, '')}@srp.local`
}

async function fetchEmployees() {
  const response = await apiClient.get<ApiResponse<UserApiDto[]>>('/api/User/GetAll')
  const users = response.data.data ?? []
  const usersById = new Map(users.map((user) => [String(user.id), user]))
  const employees = users.map(toEmployee).filter((employee): employee is Employee => employee !== null)

  return { employees, usersById }
}

interface EmployeesContextValue {
  employees: Employee[]
  isLoading: boolean
  addEmployee: (draft: EmployeeDraft) => Promise<void>
  updateEmployee: (employeeId: string, draft: EmployeeDraft) => Promise<void>
  deleteEmployee: (employeeId: string) => Promise<void>
  getEmployeeById: (employeeId: string | null | undefined) => Employee | undefined
}

const EmployeesContext = createContext<EmployeesContextValue | null>(null)

export function EmployeesProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: EMPLOYEES_QUERY_KEY,
    queryFn: fetchEmployees,
  })

  const employees = data?.employees ?? []
  const usersById = data?.usersById

  function invalidate() {
    return queryClient.invalidateQueries({ queryKey: EMPLOYEES_QUERY_KEY })
  }

  const addMutation = useMutation({
    mutationFn: async (draft: EmployeeDraft) => {
      const phoneNumber = draft.phoneNumber.trim()

      await apiClient.post('/api/User/Create', {
        email: syntheticEmail(phoneNumber),
        username: phoneNumber,
        firstname: draft.firstName,
        lastName: draft.lastName,
        password: DEFAULT_PASSWORD,
        phoneNumber,
        telegramId: draft.telegramId,
      })

      // Create only returns a message, not the new user — look it up by the
      // username we just assigned (the phone number) so we can grant the role. The
      // route is api/User/GetByName + the action's own "by-name/{username}" template.
      const created = await apiClient.get<ApiResponse<UserApiDto>>(
        `/api/User/GetByName/by-name/${encodeURIComponent(phoneNumber)}`,
      )
      const userId = created.data.data?.id

      if (userId) {
        await apiClient.post('/api/roles/assign', {
          roleName: appRoleToBackendRole[draft.role],
          userId,
        })
      }
    },
    onSuccess: invalidate,
  })

  const updateMutation = useMutation({
    mutationFn: async ({ employeeId, draft }: { employeeId: string; draft: EmployeeDraft }) => {
      const existing = usersById?.get(employeeId)
      const previousRole = existing ? resolveAppRole(existing.roles) : null
      const phoneNumber = draft.phoneNumber.trim()

      await apiClient.put('/api/User/Update', {
        id: Number(employeeId),
        email: existing?.email ?? syntheticEmail(phoneNumber),
        username: existing?.username ?? phoneNumber,
        firstname: draft.firstName,
        lastName: draft.lastName,
        phoneNumber,
        telegramId: draft.telegramId,
      })

      if (previousRole && previousRole !== draft.role) {
        await apiClient.post('/api/roles/remove', {
          roleName: appRoleToBackendRole[previousRole],
          userId: Number(employeeId),
        })
        await apiClient.post('/api/roles/assign', {
          roleName: appRoleToBackendRole[draft.role],
          userId: Number(employeeId),
        })
      }
    },
    onSuccess: invalidate,
  })

  const deleteMutation = useMutation({
    mutationFn: (employeeId: string) => apiClient.delete(`/api/User/Delete/${employeeId}`),
    onSuccess: invalidate,
  })

  const value = useMemo<EmployeesContextValue>(
    () => ({
      employees,
      isLoading,
      addEmployee: async (draft) => { await addMutation.mutateAsync(draft) },
      updateEmployee: async (employeeId, draft) => { await updateMutation.mutateAsync({ employeeId, draft }) },
      deleteEmployee: async (employeeId) => { await deleteMutation.mutateAsync(employeeId) },
      getEmployeeById: (employeeId) => employees.find((employee) => employee.id === employeeId),
    }),
    [employees, isLoading, addMutation, updateMutation, deleteMutation],
  )

  return <EmployeesContext.Provider value={value}>{children}</EmployeesContext.Provider>
}

export function useEmployees() {
  const context = useContext(EmployeesContext)

  if (!context) {
    throw new Error('useEmployees must be used inside EmployeesProvider')
  }

  return context
}
