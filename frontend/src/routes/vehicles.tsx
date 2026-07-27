import { zodResolver } from '@hookform/resolvers/zod'
import { createFileRoute } from '@tanstack/react-router'
import { AlertTriangle, Car, PencilLine, Plus, Trash2 } from 'lucide-react'
import { useEffect, useMemo, useState  } from 'react'
import type {ReactNode} from 'react';
import { useForm } from 'react-hook-form'
import { z } from 'zod'

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardEyebrow, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Select } from '@/components/ui/select'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useEmployees } from '@/features/employees/employees-context'
import { useVehicles  } from '@/features/vehicles/vehicles-context'
import type {VehicleDraft} from '@/features/vehicles/vehicles-context';
import { getErrorMessage } from '@/lib/api-error'
import { getEmployeeName } from '@/lib/domain-types'

export const Route = createFileRoute('/vehicles')({
  component: VehiclesPage,
})

const vehicleFormSchema = z.object({
  driverId: z.string().min(1, 'Select a driver-lead.'),
  plate: z.string().trim().min(2, 'Plate number must contain at least 2 characters.'),
  seatCount: z.coerce.number().int().positive('Seat count must be at least 1.'),
  amortizationBasis: z.coerce.number().nonnegative('Amortization basis cannot be negative.'),
})

type VehicleFormInput = z.input<typeof vehicleFormSchema>
type VehicleFormValues = z.output<typeof vehicleFormSchema>

const defaultValues: VehicleFormInput = {
  driverId: '',
  plate: '',
  seatCount: 4,
  amortizationBasis: 0,
}

function VehiclesPage() {
  const { employees } = useEmployees()
  const { vehicles, isLoading, addVehicle, updateVehicle, deleteVehicle } = useVehicles()
  const [editingVehicleId, setEditingVehicleId] = useState<string | null>(null)
  const [formError, setFormError] = useState('')

  const driverLeads = useMemo(
    () => employees.filter((employee) => employee.role === 'driverLead'),
    [employees],
  )

  const driverLeadsWithoutVehicle = useMemo(
    () => driverLeads.filter((employee) => !vehicles.some((vehicle) => vehicle.driverId === employee.id)),
    [driverLeads, vehicles],
  )

  const editingVehicle =
    editingVehicleId === null ? null : vehicles.find((vehicle) => vehicle.id === editingVehicleId) ?? null

  const availableDriverOptions = useMemo(
    () =>
      driverLeads.filter(
        (employee) =>
          employee.id === editingVehicle?.driverId ||
          !vehicles.some((vehicle) => vehicle.driverId === employee.id),
      ),
    [driverLeads, vehicles, editingVehicle],
  )

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<VehicleFormInput, any, VehicleFormValues>({
    resolver: zodResolver(vehicleFormSchema),
    defaultValues,
  })

  useEffect(() => {
    if (!editingVehicle) {
      reset(defaultValues)
      return
    }

    reset({
      driverId: editingVehicle.driverId,
      plate: editingVehicle.plate,
      seatCount: editingVehicle.seatCount,
      amortizationBasis: editingVehicle.amortizationBasis,
    })
  }, [editingVehicle, reset])

  function resetForm() {
    setEditingVehicleId(null)
    setFormError('')
    reset(defaultValues)
  }

  const onSubmit = handleSubmit(async (values: VehicleDraft) => {
    setFormError('')

    try {
      if (editingVehicleId) {
        await updateVehicle(editingVehicleId, values)
      } else {
        await addVehicle(values)
      }

      resetForm()
    } catch (error) {
      setFormError(getErrorMessage(error, 'Could not save this vehicle.'))
    }
  })

  async function removeVehicle(vehicleId: string) {
    if (editingVehicleId === vehicleId) {
      resetForm()
    }

    try {
      await deleteVehicle(vehicleId)
    } catch (error) {
      setFormError(getErrorMessage(error, 'Could not remove this vehicle.'))
    }
  }

  function employeeName(employeeId: string) {
    const employee = employees.find((candidate) => candidate.id === employeeId)
    return employee ? getEmployeeName(employee) : 'Unknown'
  }

  return (
    <section className="grid gap-6 xl:grid-cols-[380px_minmax(0,1fr)]">
      <Card>
        <CardHeader className="flex-row items-start justify-between gap-4">
          <div>
            <CardEyebrow>Vehicles</CardEyebrow>
            <CardTitle className="mt-2">{editingVehicle ? 'Edit Vehicle' : 'Add Vehicle'}</CardTitle>
            <CardDescription className="mt-2">
              Every Driver-Lead needs exactly one vehicle with a seat count and amortization basis.
            </CardDescription>
          </div>

          <div className="flex size-11 shrink-0 items-center justify-center rounded-2xl bg-sky-100 text-sky-700">
            <Car className="size-5" />
          </div>
        </CardHeader>

        <form className="mt-2 space-y-5" onSubmit={onSubmit}>
          <Field label="Driver-Lead" htmlFor="driverId" error={errors.driverId?.message}>
            <Select id="driverId" {...register('driverId')}>
              <option value="">Select driver-lead…</option>
              {availableDriverOptions.map((employee) => (
                <option key={employee.id} value={employee.id}>
                  {getEmployeeName(employee)}
                </option>
              ))}
            </Select>
          </Field>

          <Field label="Plate Number" htmlFor="plate" error={errors.plate?.message}>
            <Input id="plate" placeholder="01 T 123 AA" {...register('plate')} />
          </Field>

          <Field label="Seat Count" htmlFor="seatCount" error={errors.seatCount?.message}>
            <Input id="seatCount" type="number" min="1" step="1" {...register('seatCount')} />
          </Field>

          <Field
            label="Amortization Basis (TJS / month)"
            htmlFor="amortizationBasis"
            error={errors.amortizationBasis?.message}
          >
            <Input id="amortizationBasis" type="number" min="0" step="1" {...register('amortizationBasis')} />
          </Field>

          <div className="flex flex-wrap gap-3 pt-2">
            <Button
              type="submit"
              className="h-11 rounded-2xl bg-sky-600 px-5 text-white hover:bg-sky-700"
              disabled={isSubmitting || availableDriverOptions.length === 0}
            >
              {editingVehicle ? <PencilLine className="size-4" /> : <Plus className="size-4" />}
              {editingVehicle ? 'Save Changes' : 'Add Vehicle'}
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

          {availableDriverOptions.length === 0 && !editingVehicle && (
            <p className="text-xs font-medium text-amber-600">
              Every Driver-Lead already has a vehicle assigned.
            </p>
          )}

          {formError && <p className="text-xs font-medium text-red-500">{formError}</p>}
        </form>
      </Card>

      <div className="space-y-6">
        {driverLeadsWithoutVehicle.length > 0 && (
          <Card className="border-amber-200 bg-amber-50/70">
            <div className="flex items-start gap-3">
              <AlertTriangle className="mt-0.5 size-5 shrink-0 text-amber-600" />
              <div>
                <p className="text-sm font-semibold text-amber-800">Driver-Leads without a vehicle</p>
                <p className="mt-1 text-sm text-amber-700">
                  {driverLeadsWithoutVehicle.map((employee) => getEmployeeName(employee)).join(', ')} cannot
                  be assigned as a crew Driver-Lead until a vehicle is added.
                </p>
              </div>
            </div>
          </Card>
        )}

        <Card>
          <CardHeader className="flex-col gap-4 border-b border-sky-100 pb-5 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <CardEyebrow>Vehicles list</CardEyebrow>
              <CardTitle className="mt-2">Fleet</CardTitle>
              <CardDescription className="mt-2">
                {isLoading ? 'Loading…' : `${vehicles.length} vehicles registered.`}
              </CardDescription>
            </div>
          </CardHeader>

          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Driver-Lead</TableHead>
                  <TableHead>Plate</TableHead>
                  <TableHead>Seats</TableHead>
                  <TableHead>Amortization</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {vehicles.map((vehicle) => (
                  <TableRow key={vehicle.id}>
                    <TableCell className="font-medium text-slate-900">
                      {employeeName(vehicle.driverId)}
                    </TableCell>
                    <TableCell>
                      <Badge variant="secondary">{vehicle.plate}</Badge>
                    </TableCell>
                    <TableCell>{vehicle.seatCount}</TableCell>
                    <TableCell>{vehicle.amortizationBasis} TJS / mo</TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          type="button"
                          variant="outline"
                          size="icon"
                          className="rounded-full border-sky-100 text-sky-700"
                          onClick={() => setEditingVehicleId(vehicle.id)}
                        >
                          <PencilLine className="size-4" />
                        </Button>
                        <Button
                          type="button"
                          variant="destructive"
                          size="icon"
                          className="rounded-full"
                          onClick={() => removeVehicle(vehicle.id)}
                        >
                          <Trash2 className="size-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}

                {!isLoading && vehicles.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={5} className="py-10 text-center text-slate-400">
                      No vehicles registered yet.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </div>
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