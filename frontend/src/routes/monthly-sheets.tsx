import { createFileRoute } from '@tanstack/react-router'
import {
  CheckCircle2,
  ChevronDown,
  Eye,
  Loader2,
  Sparkles,
  Trash2,
} from 'lucide-react'
import { Fragment, useState } from 'react'

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
import { Select } from '@/components/ui/select'
import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { usePermissions } from '@/features/auth/use-permissions'
import { useCrews } from '@/features/crews/crews-context'
import { useMonthlySheets } from '@/features/monthly-sheets/monthly-sheets-context'
import { getErrorMessage } from '@/lib/api-error'
import { legLabels, taxiExpenseStatusLabels } from '@/lib/domain-types'
import type { MonthlySheet } from '@/lib/domain-types'
import { formatCurrency, formatMonthLabel, getMonthYear } from '@/lib/format'

export const Route = createFileRoute('/monthly-sheets')({
  component: MonthlySheetsPage,
})

function todayIsoDate() {
  return new Date().toISOString().slice(0, 10)
}

function MonthlySheetsPage() {
  const { can } = usePermissions()
  const { crews } = useCrews()
  const {
    isLoading,
    getSheet,
    previewSheet,
    generateSheet,
    confirmSheet,
    deleteSheet,
  } = useMonthlySheets()

  const [period, setPeriod] = useState<Period>(() =>
    getMonthYear(todayIsoDate()),
  )
  const [crewId, setCrewId] = useState('')
  const [preview, setPreview] = useState<MonthlySheet | null>(null)
  const [isBusy, setIsBusy] = useState(false)
  const [actionError, setActionError] = useState('')
  const [expandedEmployeeId, setExpandedEmployeeId] = useState<string | null>(
    null,
  )

  const activeCrewId = crews.some((crew) => crew.id === crewId)
    ? crewId
    : (crews[0]?.id ?? '')
  const crew = crews.find((candidate) => candidate.id === activeCrewId)
  const sheet = crew
    ? getSheet({ crewId: crew.id, year: period.year, month: period.month })
    : undefined

  function resetTransientState() {
    setPreview(null)
    setActionError('')
    setExpandedEmployeeId(null)
  }

  function selectCrew(nextCrewId: string) {
    setCrewId(nextCrewId)
    resetTransientState()
  }

  function selectPeriod(nextPeriod: Period) {
    setPeriod(nextPeriod)
    resetTransientState()
  }

  async function handlePreview() {
    if (!crew) return

    setIsBusy(true)
    setActionError('')

    try {
      const result = await previewSheet({
        crewId: crew.id,
        year: period.year,
        month: period.month,
      })
      setPreview(result)
    } catch (error) {
      setActionError(
        getErrorMessage(error, 'Could not compute a preview for this period.'),
      )
    } finally {
      setIsBusy(false)
    }
  }

  async function handleGenerate() {
    if (!crew) return

    setIsBusy(true)
    setActionError('')

    try {
      await generateSheet({
        crewId: crew.id,
        year: period.year,
        month: period.month,
      })
      setPreview(null)
    } catch (error) {
      setActionError(getErrorMessage(error, 'Could not generate the sheet.'))
    } finally {
      setIsBusy(false)
    }
  }

  async function handleConfirm() {
    if (!sheet?.id) return

    setIsBusy(true)
    setActionError('')

    try {
      await confirmSheet(sheet.id)
    } catch (error) {
      setActionError(getErrorMessage(error, 'Could not confirm the sheet.'))
    } finally {
      setIsBusy(false)
    }
  }

  async function handleDelete() {
    if (!sheet?.id) return

    setIsBusy(true)
    setActionError('')

    try {
      await deleteSheet(sheet.id)
    } catch (error) {
      setActionError(getErrorMessage(error, 'Could not delete the sheet.'))
    } finally {
      setIsBusy(false)
    }
  }

  const displayedSheet = sheet ?? preview

  return (
    <section className="space-y-6">
      <Card>
        <CardHeader className="flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <CardEyebrow>Monthly Sheets</CardEyebrow>
            <CardTitle className="mt-2">
              {formatMonthLabel(period.year, period.month)}
            </CardTitle>
            <CardDescription className="mt-2">
              Driver km, extra km, and taxi compensation are computed by the
              backend from logged transport days and approved taxi expenses —
              nothing here is entered by hand.
            </CardDescription>
          </div>

          <div className="flex flex-wrap items-end gap-3">
            <Select
              value={activeCrewId}
              onChange={(event) => selectCrew(event.target.value)}
              className="w-56"
            >
              {crews.length === 0 && <option value="">No crews yet</option>}
              {crews.map((crewOption) => (
                <option key={crewOption.id} value={crewOption.id}>
                  {crewOption.name}
                </option>
              ))}
            </Select>
            <MonthPicker value={period} onChange={selectPeriod} />
          </div>
        </CardHeader>
      </Card>

      {!crew ? (
        <Card>
          <p className="py-10 text-center text-slate-400">
            Add a crew first on the Crews page.
          </p>
        </Card>
      ) : isLoading ? (
        <Card>
          <p className="flex items-center justify-center gap-2 py-10 text-slate-400">
            <Loader2 className="size-4 animate-spin" /> Loading…
          </p>
        </Card>
      ) : (
        <Card>
          <CardHeader className="flex-col gap-4 border-b border-sky-100 pb-5 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <CardEyebrow>{crew.name}</CardEyebrow>
              <CardTitle className="mt-2 flex items-center gap-3">
                {sheet
                  ? 'Generated Sheet'
                  : preview
                    ? 'Preview (not saved)'
                    : 'No sheet yet'}
                {sheet?.isConfirmed && (
                  <Badge variant="success">Confirmed</Badge>
                )}
                {sheet && !sheet.isConfirmed && (
                  <Badge variant="warning">Draft</Badge>
                )}
                {!sheet && preview && (
                  <Badge variant="secondary">Unsaved</Badge>
                )}
              </CardTitle>
            </div>

            <div className="flex flex-wrap gap-2">
              {!sheet && (
                <>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    className="rounded-full border-sky-100 text-sky-700"
                    disabled={isBusy}
                    onClick={handlePreview}
                  >
                    <Eye className="size-3.5" />
                    Preview
                  </Button>
                  {can('generateSheet') && (
                    <Button
                      type="button"
                      size="sm"
                      className="rounded-full bg-sky-600 text-white hover:bg-sky-700"
                      disabled={isBusy}
                      onClick={handleGenerate}
                    >
                      <Sparkles className="size-3.5" />
                      Generate &amp; Save
                    </Button>
                  )}
                </>
              )}

              {sheet && !sheet.isConfirmed && can('confirmSheet') && (
                <Button
                  type="button"
                  size="sm"
                  className="rounded-full bg-emerald-600 text-white hover:bg-emerald-700"
                  disabled={isBusy}
                  onClick={handleConfirm}
                >
                  <CheckCircle2 className="size-3.5" />
                  Confirm sheet
                </Button>
              )}

              {sheet && can('deleteSheet') && (
                <Button
                  type="button"
                  variant="destructive"
                  size="sm"
                  className="rounded-full"
                  disabled={isBusy}
                  onClick={handleDelete}
                >
                  <Trash2 className="size-3.5" />
                  Delete
                </Button>
              )}
            </div>
          </CardHeader>

          {actionError && (
            <p className="mt-4 text-sm font-medium text-red-500">
              {actionError}
            </p>
          )}

          <div className="mt-4">
            {!displayedSheet ? (
              <p className="py-10 text-center text-slate-400">
                Nothing generated for this period yet — try Preview first.
              </p>
            ) : displayedSheet.payoutLines.length === 0 ? (
              <p className="py-10 text-center text-slate-400">
                No payout lines — this crew has no logged transport days for
                this period.
              </p>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Employee</TableHead>
                    <TableHead className="text-right">Driver payment</TableHead>
                    <TableHead className="text-right">
                      Extra km payment
                    </TableHead>
                    <TableHead className="text-right">
                      Taxi compensation
                    </TableHead>
                    <TableHead className="text-right">Total</TableHead>
                    <TableHead />
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {displayedSheet.payoutLines.map((line) => (
                    <Fragment key={line.id}>
                      <TableRow>
                        <TableCell className="font-medium text-slate-900">
                          {line.employeeName}
                        </TableCell>
                        <TableCell className="text-right">
                          {formatCurrency(line.driverPayment)}
                        </TableCell>
                        <TableCell className="text-right">
                          {formatCurrency(line.extraKmPayment)}
                        </TableCell>
                        <TableCell className="text-right">
                          {formatCurrency(line.taxiCompensation)}
                        </TableCell>
                        <TableCell className="text-right font-semibold text-slate-900">
                          {formatCurrency(line.totalAmount)}
                        </TableCell>
                        <TableCell className="text-right">
                          {line.taxiExpenses.length > 0 && (
                            <Button
                              type="button"
                              variant="ghost"
                              size="icon-sm"
                              className="rounded-full text-slate-400"
                              onClick={() =>
                                setExpandedEmployeeId((current) =>
                                  current === line.employeeId
                                    ? null
                                    : line.employeeId,
                                )
                              }
                            >
                              <ChevronDown
                                className={
                                  expandedEmployeeId === line.employeeId
                                    ? 'rotate-180 transition-transform'
                                    : 'transition-transform'
                                }
                              />
                            </Button>
                          )}
                        </TableCell>
                      </TableRow>

                      {expandedEmployeeId === line.employeeId &&
                        line.taxiExpenses.length > 0 && (
                          <TableRow className="hover:bg-transparent">
                            <TableCell colSpan={6} className="bg-sky-50/40">
                              <div className="space-y-1.5 py-2">
                                {line.taxiExpenses.map((expense) => (
                                  <div
                                    key={expense.id}
                                    className="flex items-center justify-between rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm"
                                  >
                                    <span className="text-slate-600">
                                      {legLabels[expense.leg]} leg
                                    </span>
                                    <span className="text-slate-600">
                                      {formatCurrency(expense.amount)}
                                    </span>
                                    <Badge
                                      variant={
                                        expense.status === 'approved'
                                          ? 'success'
                                          : 'secondary'
                                      }
                                    >
                                      {taxiExpenseStatusLabels[expense.status]}
                                    </Badge>
                                  </div>
                                ))}
                              </div>
                            </TableCell>
                          </TableRow>
                        )}
                    </Fragment>
                  ))}
                </TableBody>
                <TableFooter>
                  <TableRow>
                    <TableCell
                      className="font-semibold text-slate-900"
                      colSpan={4}
                    >
                      Total payout — {crew.name}
                    </TableCell>
                    <TableCell className="text-right text-base font-semibold text-sky-700">
                      {formatCurrency(displayedSheet.totalAmount)}
                    </TableCell>
                    <TableCell />
                  </TableRow>
                </TableFooter>
              </Table>
            )}
          </div>
        </Card>
      )}
    </section>
  )
}
