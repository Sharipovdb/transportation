import { createFileRoute } from '@tanstack/react-router'
import {
  BadgeCheck,
  CalendarCheck,
  CheckCircle2,
  FileText,
  Loader2,
  RefreshCw,
  Sparkles,
  Trash2,
  Undo2,
  Wallet,
} from 'lucide-react'
import { useMemo, useState } from 'react'

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
import { Select } from '@/components/ui/select'
import { StatCard } from '@/components/ui/stat-card'
import { usePermissions } from '@/features/auth/use-permissions'
import { useCrews } from '@/features/crews/crews-context'
import { MonthGrid } from '@/features/monthly-sheets/month-grid'
import { buildMonthlyReport } from '@/features/monthly-sheets/monthly-report'
import {
  useMonthlySheetPreview,
  useMonthlySheets,
} from '@/features/monthly-sheets/monthly-sheets-context'
import { PaySheetDialog } from '@/features/monthly-sheets/pay-sheet-dialog'
import {
  isPayable,
  SheetStatusBadge,
} from '@/features/monthly-sheets/sheet-status'
import { useMonthlyReportPrinter } from '@/features/monthly-sheets/use-monthly-report-printer'
import { getErrorMessage } from '@/lib/api-error'
import type { MonthlySheet } from '@/lib/domain-types'
import {
  formatCurrency,
  formatKm,
  formatMonthLabel,
  getMonthYear,
} from '@/lib/format'

export const Route = createFileRoute('/monthly-sheets')({
  component: MonthlySheetsPage,
})

function todayIsoDate() {
  return new Date().toISOString().slice(0, 10)
}

function MonthlySheetsPage() {
  const { can } = usePermissions()
  const { crews, getCrewName } = useCrews()
  const {
    isLoading,
    getSheet,
    generateSheet,
    confirmSheet,
    unconfirmSheet,
    markSheetPaid,
    deleteSheet,
  } = useMonthlySheets()
  const printReport = useMonthlyReportPrinter()

  const [period, setPeriod] = useState<Period>(() =>
    getMonthYear(todayIsoDate()),
  )
  const [crewId, setCrewId] = useState<number | null>(null)
  const [isBusy, setIsBusy] = useState(false)
  const [actionError, setActionError] = useState('')
  const [confirmingDelete, setConfirmingDelete] = useState(false)
  const [payingSheet, setPayingSheet] = useState<MonthlySheet | null>(null)

  const activeCrewId = crews.some((crew) => crew.id === crewId)
    ? crewId
    : (crews[0]?.id ?? null)
  const crew = crews.find((candidate) => candidate.id === activeCrewId)
  const sheet = crew
    ? getSheet({ crewId: crew.id, year: period.year, month: period.month })
    : undefined

  // A month without a saved sheet is still worth reading, so what generating would
  // produce is loaded straight away rather than hidden behind a button. A saved sheet
  // always wins: once the month exists, that is the record and there is nothing to
  // preview.
  const preview = useMonthlySheetPreview(
    crew && !sheet
      ? { crewId: crew.id, year: period.year, month: period.month }
      : null,
  )

  const displayedSheet = sheet ?? preview.data ?? null

  const report = useMemo(
    () =>
      displayedSheet
        ? buildMonthlyReport(displayedSheet, getCrewName(displayedSheet.crewId))
        : null,
    [displayedSheet, getCrewName],
  )

  // Generating a month the crew neither drove nor paid for is refused server-side, so
  // the button only appears once there is something to save.
  const hasSomethingToSave = (displayedSheet?.days.length ?? 0) > 0

  function selectCrew(nextCrewId: number) {
    setCrewId(nextCrewId)
    setActionError('')
  }

  function selectPeriod(nextPeriod: Period) {
    setPeriod(nextPeriod)
    setActionError('')
  }

  // Every action fails the same way — into one message under the header — so each one
  // is just its own call plus a sentence to show if the backend refuses it.
  async function run(action: () => Promise<unknown>, fallbackMessage: string) {
    setIsBusy(true)
    setActionError('')

    try {
      await action()
    } catch (error) {
      setActionError(getErrorMessage(error, fallbackMessage))
    } finally {
      setIsBusy(false)
    }
  }

  const handleGenerate = () =>
    run(async () => {
      if (!crew) return

      await generateSheet({
        crewId: crew.id,
        year: period.year,
        month: period.month,
      })
    }, 'Could not generate the sheet.')

  const handleConfirm = () =>
    run(async () => {
      if (sheet) await confirmSheet(sheet.id)
    }, 'Could not confirm the sheet.')

  const handleUnconfirm = () =>
    run(async () => {
      if (sheet) await unconfirmSheet(sheet.id)
    }, 'Could not reopen the sheet.')

  const handlePay = (paidSheet: MonthlySheet) =>
    run(() => markSheetPaid(paidSheet.id), 'Could not mark this crew as paid.')

  const handleDelete = () =>
    run(async () => {
      if (sheet) await deleteSheet(sheet.id)
    }, 'Could not delete the sheet.')

  const handlePrint = () =>
    run(async () => {
      if (displayedSheet) await printReport(displayedSheet)
    }, 'Could not build the PDF report.')

  const errorMessage =
    actionError ||
    (preview.error
      ? getErrorMessage(
          preview.error,
          'Could not compute this period for the crew.',
        )
      : '')

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
              One sheet per crew per month, computed from its logged transport
              days. Kilometres count as soon as a day is logged; a taxi fare
              counts once it is approved. The whole month is settled with the
              crew's lead, who distributes it inside the team.
            </CardDescription>
          </div>

          <div className="flex flex-wrap items-end gap-3">
            <Select
              value={activeCrewId ?? ''}
              onChange={(event) => selectCrew(Number(event.target.value))}
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
      ) : isLoading || preview.isLoading ? (
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
              <CardTitle className="mt-2 flex flex-wrap items-center gap-3">
                {sheet ? (
                  <>
                    Generated Sheet
                    <SheetStatusBadge sheet={sheet} />
                  </>
                ) : displayedSheet ? (
                  <>
                    Not generated yet
                    <Badge variant="secondary">Unsaved</Badge>
                  </>
                ) : (
                  'No sheet yet'
                )}
              </CardTitle>
              {displayedSheet && (
                <CardDescription className="mt-2">
                  Paid to{' '}
                  <span className="font-medium text-slate-700">
                    {displayedSheet.recipientFullname || 'the crew lead'}
                  </span>
                </CardDescription>
              )}
            </div>

            <div className="flex flex-wrap gap-2">
              {/* Days keep being logged and confirmed after a sheet is first produced,
                  so a draft can be recomputed in place to pick them up. */}
              {(!sheet || !sheet.isConfirmed) &&
                hasSomethingToSave &&
                can('generateSheet') && (
                  <Button
                    type="button"
                    size="sm"
                    className="rounded-full bg-sky-600 text-white hover:bg-sky-700"
                    disabled={isBusy}
                    onClick={handleGenerate}
                  >
                    {sheet ? (
                      <RefreshCw className="size-3.5" />
                    ) : (
                      <Sparkles className="size-3.5" />
                    )}
                    {sheet ? 'Recalculate' : 'Generate & Save'}
                  </Button>
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

              {/* Confirming only freezes the figures, so it can be taken back while the
                  month is still unpaid — the sheet returns to draft and can be
                  recalculated. Paying is what closes it for good. */}
              {sheet &&
                !sheet.isPaid &&
                sheet.isConfirmed &&
                can('confirmSheet') && (
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    className="rounded-full border-sky-100 text-sky-700"
                    disabled={isBusy}
                    onClick={handleUnconfirm}
                  >
                    <Undo2 className="size-3.5" />
                    Go back
                  </Button>
                )}

              {sheet && isPayable(sheet) && can('paySheet') && (
                <Button
                  type="button"
                  size="sm"
                  className="rounded-full bg-emerald-600 text-white hover:bg-emerald-700"
                  disabled={isBusy}
                  onClick={() => setPayingSheet(sheet)}
                >
                  <BadgeCheck className="size-3.5" />
                  Mark as paid
                </Button>
              )}

              {displayedSheet && (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="rounded-full border-sky-100 text-sky-700"
                  disabled={isBusy}
                  onClick={handlePrint}
                >
                  <FileText className="size-3.5" />
                  Print PDF
                </Button>
              )}

              {sheet && !sheet.isConfirmed && can('deleteSheet') && (
                <Button
                  type="button"
                  variant="destructive"
                  size="sm"
                  className="rounded-full"
                  disabled={isBusy}
                  onClick={() => setConfirmingDelete(true)}
                >
                  <Trash2 className="size-3.5" />
                  Delete
                </Button>
              )}
            </div>
          </CardHeader>

          {errorMessage && (
            <p className="mt-4 text-sm font-medium text-red-500">
              {errorMessage}
            </p>
          )}

          {!report ? (
            <p className="py-10 text-center text-slate-400">
              Nothing to show for this period yet.
            </p>
          ) : (
            <div className="mt-5 space-y-6">
              <div className="grid grid-cols-[repeat(auto-fit,minmax(170px,1fr))] gap-4">
                <StatCard
                  label="Days travelled"
                  value={report.travelledDayCount}
                  icon={CalendarCheck}
                />
                <StatCard
                  label="Driven (round trip)"
                  value={formatKm(report.totalDrivenKm)}
                  icon={RefreshCw}
                  hint={
                    report.totalExtraBusinessKm > 0
                      ? `+ ${formatKm(report.totalExtraBusinessKm)} business`
                      : undefined
                  }
                />
                <StatCard
                  label="Taxi to reimburse"
                  value={formatCurrency(report.totalTaxiAmount)}
                  icon={Wallet}
                  hint="The amount payable to the lead"
                />
              </div>

              {report.travelledDayCount === 0 ? (
                <p className="py-10 text-center text-slate-400">
                  No travel logged for this crew in this period.
                </p>
              ) : (
                <MonthGrid report={report} />
              )}
            </div>
          )}
        </Card>
      )}

      <ConfirmDialog
        open={confirmingDelete}
        onOpenChange={setConfirmingDelete}
        title="Delete this monthly sheet?"
        description="This action cannot be undone. It can be generated again from the confirmed transport days."
        onConfirm={handleDelete}
      />

      <PaySheetDialog
        sheet={payingSheet}
        onClose={() => setPayingSheet(null)}
        onConfirm={handlePay}
      />
    </section>
  )
}
