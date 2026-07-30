import { zodResolver } from '@hookform/resolvers/zod'
import { createFileRoute } from '@tanstack/react-router'
import {
  AlertTriangle,
  Bus,
  PencilLine,
  Plus,
  Trash2,
  Users,
} from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardDescription,
  CardEyebrow,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Select } from '@/components/ui/select'
import { CrewMembershipDialog } from '@/features/crews/crew-membership-dialog'
import { useCrewMemberships } from '@/features/crews/crew-memberships-context'
import { useCrews } from '@/features/crews/crews-context'
import type { CrewDraft } from '@/features/crews/crews-context'
import { useEmployees } from '@/features/employees/employees-context'
import { useTransportRoutes } from '@/features/routes/routes-context'
import { useVehicles } from '@/features/vehicles/vehicles-context'
import { getErrorMessage } from '@/lib/api-error'
import { getEmployeeName, leadTypes } from '@/lib/domain-types'
import type { LeadType } from '@/lib/domain-types'

export const Route = createFileRoute('/crews')({
  component: CrewsPage,
})

const crewFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(2, 'Crew name must contain at least 2 characters.'),
  routeId: z.string().min(1, 'Select a route.'),
  leadType: z.enum(leadTypes),
  leadId: z.string().min(1, 'Select a crew lead.'),
  seatCapacity: z.coerce
    .number()
    .int()
    .positive('Seat capacity must be at least 1.'),
})

type CrewFormInput = z.input<typeof crewFormSchema>
type CrewFormValues = z.output<typeof crewFormSchema>

const defaultValues: CrewFormInput = {
  name: '',
  routeId: '',
  leadType: 'driver',
  leadId: '',
  seatCapacity: 4,
}

function CrewsPage() {
  const { employees } = useEmployees()
  const { routes } = useTransportRoutes()
  const { getVehicleByDriverId } = useVehicles()
  const { crews, isLoading, addCrew, updateCrew, deleteCrew } = useCrews()
  const { getActiveMembersForCrew } = useCrewMemberships()

  const [editingCrewId, setEditingCrewId] = useState<string | null>(null)
  const [membershipCrewId, setMembershipCrewId] = useState<string | null>(null)
  const [formError, setFormError] = useState('')

  const editingCrew =
    editingCrewId === null
      ? null
      : (crews.find((crew) => crew.id === editingCrewId) ?? null)

  const {
    register,
    handleSubmit,
    reset,
    watch,
    setValue,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<CrewFormInput, any, CrewFormValues>({
    resolver: zodResolver(crewFormSchema),
    defaultValues,
  })

  const leadType = watch('leadType')
  const leadId = watch('leadId')

  const leadOptions = useMemo(
    () =>
      employees.filter((employee) =>
        leadType === 'driver'
          ? employee.role === 'driverLead'
          : employee.role === 'crewLead',
      ),
    [employees, leadType],
  )

  useEffect(() => {
    if (!editingCrew) {
      reset(defaultValues)
      return
    }

    reset({
      name: editingCrew.name,
      routeId: editingCrew.routeId,
      leadType: editingCrew.leadType,
      leadId: editingCrew.leadId,
      seatCapacity: editingCrew.seatCapacity,
    })
  }, [editingCrew, reset])

  useEffect(() => {
    if (leadType !== 'driver' || !leadId) {
      return
    }

    const vehicle = getVehicleByDriverId(leadId)

    if (vehicle) {
      setValue('seatCapacity', vehicle.seatCount)
    }
    // Only re-run when the selected driver-lead (or lead type) changes, not on every render.
  }, [leadType, leadId])

  function resetForm() {
    setEditingCrewId(null)
    setFormError('')
    reset(defaultValues)
  }

  const onSubmit = handleSubmit(async (values: CrewDraft) => {
    if (values.leadType === 'driver' && !getVehicleByDriverId(values.leadId)) {
      setError('leadId', {
        type: 'validate',
        message:
          'This Driver-Lead has no vehicle yet — add one on the Vehicles page first.',
      })
      return
    }

    setFormError('')

    try {
      if (editingCrewId) {
        await updateCrew(editingCrewId, values)
      } else {
        await addCrew(values)
      }

      resetForm()
    } catch (error) {
      setFormError(getErrorMessage(error, 'Could not save this crew.'))
    }
  })

  async function removeCrew(crewId: string) {
    if (editingCrewId === crewId) {
      resetForm()
    }

    try {
      await deleteCrew(crewId)
    } catch (error) {
      setFormError(getErrorMessage(error, 'Could not remove this crew.'))
    }
  }

  function routeName(routeId: string) {
    return routes.find((route) => route.id === routeId)?.name ?? 'Unknown route'
  }

  function employeeName(employeeId: string) {
    const employee = employees.find((candidate) => candidate.id === employeeId)
    return employee ? getEmployeeName(employee) : 'Unknown'
  }

  const membershipCrew = membershipCrewId
    ? (crews.find((crew) => crew.id === membershipCrewId) ?? null)
    : null

  return (
    <section className="space-y-6">
      <div className="grid gap-6 xl:grid-cols-[380px_minmax(0,1fr)]">
        <Card>
          <CardHeader className="flex-row items-start justify-between gap-4">
            <div>
              <CardEyebrow>Crews</CardEyebrow>
              <CardTitle className="mt-2">
                {editingCrew ? 'Edit Crew' : 'Add Crew'}
              </CardTitle>
              <CardDescription className="mt-2">
                A crew has a name, a route, and a lead — either a Driver-Lead or
                a Manager-Lead.
              </CardDescription>
            </div>

            <div className="flex size-11 shrink-0 items-center justify-center rounded-2xl bg-sky-100 text-sky-700">
              <Bus className="size-5" />
            </div>
          </CardHeader>

          <form className="mt-2 space-y-5" onSubmit={onSubmit}>
            <Field
              label="Crew Name"
              htmlFor="name"
              error={errors.name?.message}
            >
              <Input
                id="name"
                placeholder="Karakum Site Crew A"
                {...register('name')}
              />
            </Field>

            <Field
              label="Route"
              htmlFor="routeId"
              error={errors.routeId?.message}
            >
              <Select id="routeId" {...register('routeId')}>
                <option value="">Select route…</option>
                {routes.map((route) => (
                  <option key={route.id} value={route.id}>
                    {route.name} ({route.distanceKm} km)
                  </option>
                ))}
              </Select>
            </Field>

            <div className="space-y-2">
              <p className="text-sm font-medium text-slate-700">Lead Type</p>
              <div className="grid grid-cols-2 gap-2">
                {(['driver', 'manager'] as LeadType[]).map((type) => (
                  <button
                    key={type}
                    type="button"
                    onClick={() => {
                      setValue('leadType', type)
                      setValue('leadId', '')
                    }}
                    className={`rounded-2xl border px-4 py-2.5 text-sm font-medium transition ${
                      leadType === type
                        ? 'border-sky-600 bg-sky-600 text-white'
                        : 'border-sky-100 bg-white text-slate-600 hover:border-sky-200'
                    }`}
                  >
                    {type === 'driver' ? 'Driver-Lead' : 'Manager-Lead'}
                  </button>
                ))}
              </div>
            </div>

            <Field
              label="Crew Lead"
              htmlFor="leadId"
              error={errors.leadId?.message}
            >
              <Select id="leadId" {...register('leadId')}>
                <option value="">Select lead…</option>
                {leadOptions.map((employee) => (
                  <option key={employee.id} value={employee.id}>
                    {getEmployeeName(employee)}
                  </option>
                ))}
              </Select>
              {leadOptions.length === 0 && (
                <p className="text-xs text-amber-600">
                  No {leadType === 'driver' ? 'Driver-Lead' : 'Manager-Lead'}{' '}
                  employees available yet.
                </p>
              )}
            </Field>

            <Field
              label="Seat Capacity"
              htmlFor="seatCapacity"
              error={errors.seatCapacity?.message}
            >
              <Input
                id="seatCapacity"
                type="number"
                min="1"
                step="1"
                {...register('seatCapacity')}
              />
              {leadType === 'driver' && (
                <p className="text-xs text-slate-400">
                  Defaults from the driver's vehicle — still editable.
                </p>
              )}
            </Field>

            {formError && (
              <p className="text-xs font-medium text-red-500">{formError}</p>
            )}

            <div className="flex flex-wrap gap-3 pt-2">
              <Button
                type="submit"
                className="h-11 rounded-2xl bg-sky-600 px-5 text-white hover:bg-sky-700"
                disabled={isSubmitting}
              >
                {editingCrew ? (
                  <PencilLine className="size-4" />
                ) : (
                  <Plus className="size-4" />
                )}
                {editingCrew ? 'Save Changes' : 'Add Crew'}
              </Button>

              <Button
                type="button"
                variant="outline"
                className="h-11 rounded-2xl border-sky-100 px-5 text-slate-700"
                onClick={resetForm}
              >
                Clear Form
              </Button>
            </div>
          </form>
        </Card>

        <div className="grid gap-5 sm:grid-cols-2">
          {crews.map((crew) => {
            const activeMembers = getActiveMembersForCrew(crew.id)
            const isOverflowing = activeMembers.length > crew.seatCapacity

            return (
              <Card key={crew.id} className="flex flex-col">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <Badge
                      variant={
                        crew.leadType === 'driver' ? 'success' : 'default'
                      }
                    >
                      {crew.leadType === 'driver'
                        ? 'Driver-Lead'
                        : 'Manager-Lead'}
                    </Badge>
                    <h3 className="mt-2 text-lg font-semibold text-slate-950">
                      {crew.name}
                    </h3>
                    <p className="text-sm text-slate-500">
                      {routeName(crew.routeId)}
                    </p>
                  </div>

                  <div className="flex gap-1.5">
                    <Button
                      type="button"
                      variant="outline"
                      size="icon-sm"
                      className="rounded-full border-sky-100 text-sky-700"
                      onClick={() => setEditingCrewId(crew.id)}
                    >
                      <PencilLine className="size-3.5" />
                    </Button>
                    <Button
                      type="button"
                      variant="destructive"
                      size="icon-sm"
                      className="rounded-full"
                      onClick={() => removeCrew(crew.id)}
                    >
                      <Trash2 className="size-3.5" />
                    </Button>
                  </div>
                </div>

                <div className="mt-4 space-y-1.5 text-sm text-slate-600">
                  <p>
                    <span className="font-medium text-slate-800">Lead:</span>{' '}
                    {employeeName(crew.leadId)}
                  </p>
                  <p>
                    <span className="font-medium text-slate-800">Seats:</span>{' '}
                    {activeMembers.length} / {crew.seatCapacity}
                  </p>
                </div>

                {isOverflowing && (
                  <Badge variant="warning" className="mt-3 w-fit">
                    <AlertTriangle className="size-3" />
                    Overflow — arrange extra taxi/vehicle
                  </Badge>
                )}

                <Button
                  type="button"
                  variant="outline"
                  className="mt-4 h-10 w-full rounded-2xl border-sky-100 text-sky-700"
                  onClick={() => setMembershipCrewId(crew.id)}
                >
                  <Users className="size-4" />
                  Manage Members
                </Button>
              </Card>
            )
          })}

          {!isLoading && crews.length === 0 && (
            <Card className="sm:col-span-2">
              <p className="py-10 text-center text-slate-400">
                No crews yet — add the first one.
              </p>
            </Card>
          )}
        </div>
      </div>

      {membershipCrew && (
        <CrewMembershipDialog
          crew={membershipCrew}
          open={membershipCrewId !== null}
          onOpenChange={(nextOpen) => {
            if (!nextOpen) {
              setMembershipCrewId(null)
            }
          }}
        />
      )}
    </section>
  )
}

interface FieldProps {
  label: string
  htmlFor: string
  error?: string
  children: ReactNode
}

function Field({ label, htmlFor, error, children }: FieldProps) {
  return (
    <div className="space-y-2">
      <label className="text-sm font-medium text-slate-700" htmlFor={htmlFor}>
        {label}
      </label>
      {children}
      {error && <p className="text-xs font-medium text-red-500">{error}</p>}
    </div>
  )
}
