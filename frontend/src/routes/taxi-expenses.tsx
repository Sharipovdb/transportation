import { zodResolver } from '@hookform/resolvers/zod'
import { createFileRoute } from '@tanstack/react-router'
import { Check, PencilLine, Plus, Receipt, Trash2, X } from 'lucide-react'
import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'

import { ConfirmDialog } from '@/components/confirm-dialog'
import type { Period } from '@/components/month-picker'
import { MonthPicker } from '@/components/month-picker'
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
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { usePermissions } from '@/features/auth/use-permissions'
import { useCrews } from '@/features/crews/crews-context'
import { useEmployees } from '@/features/employees/employees-context'
import type { TaxiExpenseDraft } from '@/features/taxi-expenses/taxi-expenses-context'
import { useTaxiExpenses } from '@/features/taxi-expenses/taxi-expenses-context'
import { useTransportDays } from '@/features/transport-days/transport-days-context'
import { getErrorMessage } from '@/lib/api-error'
import { legLabels, legs, taxiExpenseStatusLabels } from '@/lib/domain-types'
import type { TaxiExpenseStatus } from '@/lib/domain-types'
import {
  formatCurrency,
  formatDayLabel,
  formatMonthLabel,
  getMonthYear,
} from '@/lib/format'

export const Route = createFileRoute('/taxi-expenses')({
  component: TaxiExpensesPage,
})

const expenseFormSchema = z.object({
  crewId: z.number().min(1, 'Select a crew.'),
  transportDayId: z.number().min(1, 'Select a transport day.'),
  leg: z.enum(legs),
  amount: z.number().positive('Amount must be greater than 0.'),
  paidById: z.number().min(1, 'Select who paid.'),
})

type ExpenseFormInput = z.input<typeof expenseFormSchema>
type ExpenseFormValues = z.output<typeof expenseFormSchema>

function todayIsoDate() {
  return new Date().toISOString().slice(0, 10)
}

function defaultValues(): ExpenseFormInput {
  return {
    crewId: 0,
    transportDayId: 0,
    leg: 'Morning',
    amount: 0,
    paidById: 0,
  }
}

const statusBadgeVariant: Record<
  TaxiExpenseStatus,
  'warning' | 'success' | 'destructive' | 'secondary'
> = {
  Pending: 'warning',
  Approved: 'success',
  Rejected: 'destructive',
  Paid: 'secondary',
}

function TaxiExpensesPage() {
  const { can } = usePermissions()
  const { crews } = useCrews()
  const { employees, getEmployeeById } = useEmployees()
  const { transportDays, getDayById } = useTransportDays()
  const {
    taxiExpenses,
    isLoading,
    addTaxiExpense,
    updateTaxiExpense,
    deleteTaxiExpense,
    approveTaxiExpense,
    rejectTaxiExpense,
  } = useTaxiExpenses()

  const [period, setPeriod] = useState<Period>(() =>
    getMonthYear(todayIsoDate()),
  )
  const [crewFilter, setCrewFilter] = useState<number | null>(null)
  const [editingExpenseId, setEditingExpenseId] = useState<number | null>(null)
  const [formError, setFormError] = useState('')
  const [deletingExpenseId, setDeletingExpenseId] = useState<number | null>(
    null,
  )

  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<ExpenseFormInput, any, ExpenseFormValues>({
    resolver: zodResolver(expenseFormSchema),
    defaultValues: defaultValues(),
  })

  const formCrewId = watch('crewId')

  const dayOptions = useMemo(
    () =>
      transportDays
        .filter((day) => day.crewId === formCrewId)
        .sort((first, second) => second.date.localeCompare(first.date)),
    [transportDays, formCrewId],
  )

  const visibleExpenses = useMemo(() => {
    return taxiExpenses
      .filter((expense) => {
        const day = getDayById(expense.transportDayId)

        if (!day) {
          return false
        }

        const inPeriod =
          getMonthYear(day.date).year === period.year &&
          getMonthYear(day.date).month === period.month
        const inCrew = !crewFilter || day.crewId === crewFilter

        return inPeriod && inCrew
      })
      .sort((first, second) =>
        (getDayById(second.transportDayId)?.date ?? '').localeCompare(
          getDayById(first.transportDayId)?.date ?? '',
        ),
      )
  }, [taxiExpenses, period, crewFilter, getDayById])

  function resetForm() {
    setEditingExpenseId(null)
    setFormError('')
    reset(defaultValues())
  }

  function startEdit(expenseId: number) {
    const expense = taxiExpenses.find((candidate) => candidate.id === expenseId)
    const day = expense ? getDayById(expense.transportDayId) : null

    if (!expense || !day) {
      return
    }

    setEditingExpenseId(expenseId)
    setFormError('')
    reset({
      crewId: day.crewId,
      transportDayId: expense.transportDayId,
      leg: expense.leg,
      amount: expense.amount,
      paidById: expense.paidById,
    })
  }

  const onSubmit = handleSubmit(async (values: ExpenseFormValues) => {
    setFormError('')

    const draft: TaxiExpenseDraft = {
      transportDayId: values.transportDayId,
      leg: values.leg,
      amount: values.amount,
      paidById: values.paidById,
      status: 'Pending',
    }

    try {
      if (editingExpenseId) {
        await updateTaxiExpense(editingExpenseId, draft)
      } else {
        await addTaxiExpense(draft)
      }

      resetForm()
    } catch (error) {
      setFormError(getErrorMessage(error, 'Could not save this taxi expense.'))
    }
  })

  async function removeExpense(expenseId: number) {
    if (editingExpenseId === expenseId) {
      resetForm()
    }

    try {
      await deleteTaxiExpense(expenseId)
    } catch (error) {
      setFormError(getErrorMessage(error, 'Could not remove this expense.'))
    }
  }

  async function handleApprove(expenseId: number) {
    try {
      await approveTaxiExpense(expenseId)
    } catch (error) {
      setFormError(getErrorMessage(error, 'Could not approve this expense.'))
    }
  }

  async function handleReject(expenseId: number) {
    try {
      await rejectTaxiExpense(expenseId)
    } catch (error) {
      setFormError(getErrorMessage(error, 'Could not reject this expense.'))
    }
  }

  function crewName(crewId: number) {
    return crews.find((crew) => crew.id === crewId)?.name ?? 'Unknown crew'
  }

  function payerName(employeeId: number) {
    const employee = getEmployeeById(employeeId)
    return employee ? employee.fullname : 'Unknown'
  }

  return (
    <section className="grid gap-6 xl:grid-cols-[380px_minmax(0,1fr)]">
      <Card>
        <CardHeader className="flex-row items-start justify-between gap-4">
          <div>
            <CardEyebrow>Reimbursements</CardEyebrow>
            <CardTitle className="mt-2">
              {editingExpenseId ? 'Edit Taxi Expense' : 'Record Taxi Expense'}
            </CardTitle>
            <CardDescription className="mt-2">
              Every taxi ride is recorded against a specific day and leg. New
              expenses start Pending until approved.
            </CardDescription>
          </div>

          <div className="flex size-11 shrink-0 items-center justify-center rounded-2xl bg-sky-100 text-sky-700">
            <Receipt className="size-5" />
          </div>
        </CardHeader>

        <form className="mt-2 space-y-5" onSubmit={onSubmit}>
          <Field label="Crew" htmlFor="crewId" error={errors.crewId?.message}>
            <Select
              id="crewId"
              disabled={!!editingExpenseId}
              {...register('crewId', { valueAsNumber: true })}
            >
              <option value="0">Select crew…</option>
              {crews.map((crew) => (
                <option key={crew.id} value={crew.id}>
                  {crew.name}
                </option>
              ))}
            </Select>
          </Field>

          <Field
            label="Transport Day"
            htmlFor="transportDayId"
            error={errors.transportDayId?.message}
          >
            <Select
              id="transportDayId"
              disabled={!!editingExpenseId || !formCrewId}
              {...register('transportDayId', { valueAsNumber: true })}
            >
              <option value="0">Select day…</option>
              {dayOptions.map((day) => (
                <option key={day.id} value={day.id}>
                  {formatDayLabel(day.date)}
                </option>
              ))}
            </Select>
            {formCrewId > 0 && dayOptions.length === 0 && (
              <p className="text-xs text-amber-600">
                No transport days logged for this crew yet.
              </p>
            )}
          </Field>

          <Field label="Leg" htmlFor="leg" error={errors.leg?.message}>
            <Select id="leg" {...register('leg')}>
              {legs.map((leg) => (
                <option key={leg} value={leg}>
                  {legLabels[leg]}
                </option>
              ))}
            </Select>
          </Field>

          <Field
            label="Amount (TJS)"
            htmlFor="amount"
            error={errors.amount?.message}
          >
            <Input
              id="amount"
              type="number"
              min="0"
              step="1"
              {...register('amount', { valueAsNumber: true })}
            />
          </Field>

          <Field
            label="Paid By"
            htmlFor="paidById"
            error={errors.paidById?.message}
          >
            <Select
              id="paidById"
              {...register('paidById', { valueAsNumber: true })}
            >
              <option value="0">Select employee…</option>
              {employees.map((employee) => (
                <option key={employee.id} value={employee.id}>
                  {employee.fullname}
                </option>
              ))}
            </Select>
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
              {editingExpenseId ? (
                <PencilLine className="size-4" />
              ) : (
                <Plus className="size-4" />
              )}
              {editingExpenseId ? 'Save Changes' : 'Record Expense'}
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
            <CardEyebrow>Taxi expenses</CardEyebrow>
            <CardTitle className="mt-2">
              {formatMonthLabel(period.year, period.month)}
            </CardTitle>
            <CardDescription className="mt-2">
              {isLoading
                ? 'Loading…'
                : `${visibleExpenses.length} expense(s) shown.`}
            </CardDescription>
          </div>

          <div className="flex flex-wrap items-end gap-3">
            <Select
              value={crewFilter ?? ''}
              onChange={(event) => setCrewFilter(Number(event.target.value))}
              className="w-48"
            >
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
                <TableHead>Leg</TableHead>
                <TableHead className="text-right">Amount</TableHead>
                <TableHead>Paid By</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {visibleExpenses.map((expense) => {
                const day = getDayById(expense.transportDayId)
                const isPending = expense.taxiExpenseStatus === 'Pending'

                return (
                  <TableRow key={expense.id}>
                    <TableCell className="font-medium text-slate-900">
                      {day ? formatDayLabel(day.date) : '—'}
                    </TableCell>
                    <TableCell>{day ? crewName(day.crewId) : '—'}</TableCell>
                    <TableCell>{legLabels[expense.leg]}</TableCell>
                    <TableCell className="text-right">
                      {formatCurrency(expense.amount)}
                    </TableCell>
                    <TableCell>{payerName(expense.paidById)}</TableCell>
                    <TableCell>
                      <Badge
                        variant={statusBadgeVariant[expense.taxiExpenseStatus]}
                      >
                        {taxiExpenseStatusLabels[expense.taxiExpenseStatus]}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        {isPending && can('approveTaxiExpense') && (
                          <>
                            <Button
                              type="button"
                              variant="outline"
                              size="icon-sm"
                              className="rounded-full border-emerald-200 text-emerald-700"
                              onClick={() => handleApprove(expense.id)}
                              title="Approve"
                            >
                              <Check className="size-3.5" />
                            </Button>
                            <Button
                              type="button"
                              variant="outline"
                              size="icon-sm"
                              className="rounded-full border-red-200 text-red-600"
                              onClick={() => handleReject(expense.id)}
                              title="Reject"
                            >
                              <X className="size-3.5" />
                            </Button>
                          </>
                        )}
                        {isPending && (
                          <Button
                            type="button"
                            variant="outline"
                            size="icon-sm"
                            className="rounded-full border-sky-100 text-sky-700"
                            onClick={() => startEdit(expense.id)}
                          >
                            <PencilLine className="size-3.5" />
                          </Button>
                        )}
                        <Button
                          type="button"
                          variant="destructive"
                          size="icon-sm"
                          className="rounded-full"
                          onClick={() => setDeletingExpenseId(expense.id)}
                        >
                          <Trash2 className="size-3.5" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                )
              })}

              {!isLoading && visibleExpenses.length === 0 && (
                <TableRow>
                  <TableCell
                    colSpan={7}
                    className="py-10 text-center text-slate-400"
                  >
                    No taxi expenses recorded for this period.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </div>
      </Card>

      <ConfirmDialog
        open={deletingExpenseId !== null}
        onOpenChange={(open) => !open && setDeletingExpenseId(null)}
        title="Delete this taxi expense?"
        description="This action cannot be undone."
        onConfirm={() => {
          if (deletingExpenseId !== null) {
            removeExpense(deletingExpenseId)
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
