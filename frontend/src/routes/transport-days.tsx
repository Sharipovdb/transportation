import { zodResolver } from '@hookform/resolvers/zod'
import { createFileRoute } from '@tanstack/react-router'
import { CheckCircle2, PencilLine, Plus, Route as RouteIcon, RotateCcw, Trash2 } from 'lucide-react'
import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'

import { ConfirmDialog } from '@/components/confirm-dialog'
import type { Period } from '@/components/month-picker'
import { MonthPicker } from '@/components/month-picker'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardDescription, CardEyebrow, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Select } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useCrews } from '@/features/crews/crews-context'
import { useEmployees } from '@/features/employees/employees-context'
import type { TransportDayDraft } from '@/features/transport-days/transport-days-context'
import { useTransportDays } from '@/features/transport-days/transport-days-context'
import { getErrorMessage } from '@/lib/api-error'
import { getEmployeeName, transportModeLabels, transportModes } from '@/lib/domain-types'
import type { TransportMode } from '@/lib/domain-types'
import { formatDayLabel, formatKm, formatMonthLabel, getMonthYear } from '@/lib/format'

export const Route = createFileRoute('/transport-days')({
  component: TransportDaysPage,
})

const dayFormSchema = z.object({
  crewId: z.string().min(1, 'Select a crew.'),
  date: z.string().min(1, 'Select a date.'),
  morningMode: z.enum(transportModes),
  afternoonMode: z.enum(transportModes),
  extraBusinessKm: z.coerce.number().nonnegative('Extra km cannot be negative.'),
  notes: z.string(),
})

type DayFormInput = z.input<typeof dayFormSchema>
type DayFormValues = z.output<typeof dayFormSchema>

function todayIsoDate() {
  return new Date().toISOString().slice(0, 10)
}

function defaultValues(): DayFormInput {
  return {
    crewId: '',
    date: todayIsoDate(),
    morningMode: 'driven',
    afternoonMode: 'driven',
    extraBusinessKm: 0,
    notes: '',
  }
}

const modeBadgeVariant: Record<TransportMode, 'success' | 'warning' | 'secondary'> = {
  driven: 'success',
  taxi: 'warning',
  none: 'secondary',
}

function TransportDaysPage() {
  const { crews } = useCrews()
  const { getEmployeeById } = useEmployees()
  const {
    transportDays,
    isLoading,
    addTransportDay,
    updateTransportDay,
    deleteTransportDay,
    confirmTransportDay,
    unconfirmTransportDay,
  } = useTransportDays()

  const [period, setPeriod] = useState<Period>(() => getMonthYear(todayIsoDate()))
  const [crewFilter, setCrewFilter] = useState('')
  const [editingDayId, setEditingDayId] = useState<string | null>(null)
  const [formError, setFormError] = useState('')
  const [deletingDayId, setDeletingDayId] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<DayFormInput, any, DayFormValues>({
    resolver: zodResolver(dayFormSchema),
    defaultValues: defaultValues(),
  })

  const editingDay = editingDayId ? transportDays.find((day) => day.id === editingDayId) ?? null : null

  const visibleDays = useMemo(() => {
    return transportDays
      .filter((day) => {
        const inPeriod = getMonthYear(day.date).year === period.year && getMonthYear(day.date).month === period.month
        const inCrew = !crewFilter || day.crewId === crewFilter
        return inPeriod && inCrew
      })
      .sort((first, second) => second.date.localeCompare(first.date))
  }, [transportDays, period, crewFilter])

  function resetForm() {
    setEditingDayId(null)
    setFormError('')
    reset(defaultValues())
  }

  function startEdit(dayId: string) {
    const day = transportDays.find((candidate) => candidate.id === dayId)

    if (!day) {
      return
    }

    setEditingDayId(dayId)
    setFormError('')
    reset({
      crewId: day.crewId,
      date: day.date,
      morningMode: day.morningMode,
      afternoonMode: day.afternoonMode,
      extraBusinessKm: day.extraBusinessKm,
      notes: day.notes,
    })
  }

  const onSubmit = handleSubmit(async (values: TransportDayDraft) => {
    setFormError('')

    try {
      if (editingDayId) {
        await updateTransportDay(editingDayId, {
          morningMode: values.morningMode,
          afternoonMode: values.afternoonMode,
          extraBusinessKm: values.extraBusinessKm,
          notes: values.notes,
        })
      } else {
        await addTransportDay(values)
      }

      resetForm()
    } catch (error) {
      setFormError(getErrorMessage(error, 'Could not save this transport day.'))
    }
  })

  async function removeDay(dayId: string) {
    if (editingDayId === dayId) {
      resetForm()
    }

    try {
      await deleteTransportDay(dayId)
    } catch (error) {
      setFormError(getErrorMessage(error, 'Could not remove this day.'))
    }
  }

  async function toggleConfirm(dayId: string, confirmed: boolean) {
    try {
      if (confirmed) {
        await unconfirmTransportDay(dayId)
      } else {
        await confirmTransportDay(dayId)
      }
    } catch (error) {
      setFormError(getErrorMessage(error, 'Could not update confirmation.'))
    }
  }

  function crewName(crewId: string) {
    return crews.find((crew) => crew.id === crewId)?.name ?? 'Unknown crew'
  }

  function driverName(driverId: string | null) {
    if (!driverId) {
      return '—'
    }

    const employee = getEmployeeById(driverId)
    return employee ? getEmployeeName(employee) : '—'
  }

  return (
    <section className="grid gap-6 xl:grid-cols-[380px_minmax(0,1fr)]">
      <Card>
        <CardHeader className="flex-row items-start justify-between gap-4">
          <div>
            <CardEyebrow>Daily Log</CardEyebrow>
            <CardTitle className="mt-2">{editingDay ? 'Edit Transport Day' : 'Log Transport Day'}</CardTitle>
            <CardDescription className="mt-2">
              One record per crew per working day. The driver and base commute km are derived
              automatically from the crew.
            </CardDescription>
          </div>

          <div className="flex size-11 shrink-0 items-center justify-center rounded-2xl bg-sky-100 text-sky-700">
            <RouteIcon className="size-5" />
          </div>
        </CardHeader>

        <form className="mt-2 space-y-5" onSubmit={onSubmit}>
          <Field label="Crew" htmlFor="crewId" error={errors.crewId?.message}>
            <Select id="crewId" disabled={!!editingDay} {...register('crewId')}>
              <option value="">Select crew…</option>
              {crews.map((crew) => (
                <option key={crew.id} value={crew.id}>
                  {crew.name}
                </option>
              ))}
            </Select>
          </Field>

          <Field label="Date" htmlFor="date" error={errors.date?.message}>
            <Input id="date" type="date" disabled={!!editingDay} {...register('date')} />
          </Field>

          <div className="grid grid-cols-2 gap-4">
            <Field label="Morning" htmlFor="morningMode" error={errors.morningMode?.message}>
              <Select id="morningMode" {...register('morningMode')}>
                {transportModes.map((mode) => (
                  <option key={mode} value={mode}>
                    {transportModeLabels[mode]}
                  </option>
                ))}
              </Select>
            </Field>

            <Field label="Afternoon" htmlFor="afternoonMode" error={errors.afternoonMode?.message}>
              <Select id="afternoonMode" {...register('afternoonMode')}>
                {transportModes.map((mode) => (
                  <option key={mode} value={mode}>
                    {transportModeLabels[mode]}
                  </option>
                ))}
              </Select>
            </Field>
          </div>

          <Field
            label="Extra Business Km"
            htmlFor="extraBusinessKm"
            error={errors.extraBusinessKm?.message}
          >
            <Input id="extraBusinessKm" type="number" min="0" step="0.1" {...register('extraBusinessKm')} />
          </Field>

          <Field label="Notes" htmlFor="notes" error={errors.notes?.message}>
            <Textarea id="notes" rows={3} placeholder="Optional context for this day" {...register('notes')} />
          </Field>

          {formError && <p className="text-xs font-medium text-red-500">{formError}</p>}

          <div className="flex flex-wrap gap-3 pt-2">
            <Button
              type="submit"
              className="h-11 rounded-2xl bg-sky-600 px-5 text-white hover:bg-sky-700"
              disabled={isSubmitting}
            >
              {editingDay ? <PencilLine className="size-4" /> : <Plus className="size-4" />}
              {editingDay ? 'Save Changes' : 'Log Day'}
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

      <Card>
        <CardHeader className="flex-col gap-4 border-b border-sky-100 pb-5 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <CardEyebrow>Transport days</CardEyebrow>
            <CardTitle className="mt-2">{formatMonthLabel(period.year, period.month)}</CardTitle>
            <CardDescription className="mt-2">
              {isLoading ? 'Loading…' : `${visibleDays.length} day(s) shown.`}
            </CardDescription>
          </div>

          <div className="flex flex-wrap items-end gap-3">
            <Select value={crewFilter} onChange={(event) => setCrewFilter(event.target.value)} className="w-48">
              <option value="">All crews</option>
              {crews.map((crew) => (
                <option key={crew.id} value={crew.id}>
                  {crew.name}
                </option>
              ))}
            </Select>
            <MonthPicker value={period} onChange={setPeriod} />
          </div>
        </CardHeader>

        <div className="mt-4">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Day</TableHead>
                <TableHead>Crew</TableHead>
                <TableHead>Morning</TableHead>
                <TableHead>Afternoon</TableHead>
                <TableHead>Driver</TableHead>
                <TableHead className="text-right">Commute</TableHead>
                <TableHead className="text-right">Extra km</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {visibleDays.map((day) => (
                <TableRow key={day.id}>
                  <TableCell className="font-medium text-slate-900">{formatDayLabel(day.date)}</TableCell>
                  <TableCell>{crewName(day.crewId)}</TableCell>
                  <TableCell>
                    <Badge variant={modeBadgeVariant[day.morningMode]}>
                      {transportModeLabels[day.morningMode]}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <Badge variant={modeBadgeVariant[day.afternoonMode]}>
                      {transportModeLabels[day.afternoonMode]}
                    </Badge>
                  </TableCell>
                  <TableCell>{driverName(day.driverId)}</TableCell>
                  <TableCell className="text-right">{formatKm(day.commuteKm)}</TableCell>
                  <TableCell className="text-right">
                    {day.extraBusinessKm > 0 ? formatKm(day.extraBusinessKm) : '—'}
                  </TableCell>
                  <TableCell>
                    {day.confirmed ? <Badge variant="success">Confirmed</Badge> : <Badge variant="warning">Draft</Badge>}
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button
                        type="button"
                        variant="outline"
                        size="icon-sm"
                        className="rounded-full border-sky-100 text-sky-700"
                        onClick={() => toggleConfirm(day.id, day.confirmed)}
                        title={day.confirmed ? 'Unconfirm' : 'Confirm'}
                      >
                        {day.confirmed ? <RotateCcw className="size-3.5" /> : <CheckCircle2 className="size-3.5" />}
                      </Button>
                      <Button
                        type="button"
                        variant="outline"
                        size="icon-sm"
                        className="rounded-full border-sky-100 text-sky-700"
                        onClick={() => startEdit(day.id)}
                      >
                        <PencilLine className="size-3.5" />
                      </Button>
                      <Button
                        type="button"
                        variant="destructive"
                        size="icon-sm"
                        className="rounded-full"
                        onClick={() => setDeletingDayId(day.id)}
                      >
                        <Trash2 className="size-3.5" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}

              {!isLoading && visibleDays.length === 0 && (
                <TableRow>
                  <TableCell colSpan={9} className="py-10 text-center text-slate-400">
                    No transport days logged for this period.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </div>
      </Card>

      <ConfirmDialog
        open={deletingDayId !== null}
        onOpenChange={(open) => !open && setDeletingDayId(null)}
        title="Delete this transport day?"
        description="This action cannot be undone."
        onConfirm={() => {
          if (deletingDayId !== null) {
            removeDay(deletingDayId)
          }
        }}
      />
    </section>
  )
}

interface FieldProps {
  label: string
  htmlFor: string
  error?: string
  children: React.ReactNode
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
