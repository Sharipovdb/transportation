import { createFileRoute } from '@tanstack/react-router'
import { BadgeCheck, ChevronDown, Wallet } from 'lucide-react'
import { Fragment, useMemo, useState } from 'react'

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
import type { PayoutLineTaxiExpense } from '@/lib/domain-types'
import {
  formatCurrency,
  formatKm,
  formatMonthLabel,
  getMonthYear,
} from '@/lib/format'

export const Route = createFileRoute('/payouts')({
  component: PayoutsPage,
})

function todayIsoDate() {
  return new Date().toISOString().slice(0, 10)
}

interface FlatPayoutRow {
  key: string
  payoutLineId: string
  crewName: string
  employeeId: string
  employeeName: string
  driverKm: number
  extraBusinessKm: number
  taxiCompensation: number
  totalAmount: number
  isPaid: boolean
  taxiExpenses: PayoutLineTaxiExpense[]
}

function PayoutsPage() {
  const { can } = usePermissions()
  const { getCrewName } = useCrews()
  const { sheets, isLoading, markPayoutLinePaid } = useMonthlySheets()

  const [period, setPeriod] = useState<Period>(() => getMonthYear(todayIsoDate()))
  const [expandedRowKey, setExpandedRowKey] = useState<string | null>(null)
  const [payingRow, setPayingRow] = useState<FlatPayoutRow | null>(null)
  const [actionError, setActionError] = useState('')

  const rows = useMemo<FlatPayoutRow[]>(() => {
    const sheetsInPeriod = sheets.filter((sheet) => sheet.year === period.year && sheet.month === period.month)

    return sheetsInPeriod.flatMap((sheet) => {
      const crewName = getCrewName(sheet.crewId)

      return sheet.payoutLines.map((line) => ({
        key: `${sheet.id}-${line.id}`,
        payoutLineId: line.id,
        crewName,
        employeeId: line.employeeId,
        employeeName: line.employeeName,
        driverKm: line.driverKm,
        extraBusinessKm: line.extraBusinessKm,
        taxiCompensation: line.taxiCompensation,
        totalAmount: line.totalAmount,
        isPaid: line.isPaid,
        taxiExpenses: line.taxiExpenses,
      }))
    })
  }, [sheets, getCrewName, period])

  // Money already released is excluded from what still has to be paid out.
  const grandTotal = rows.reduce((total, row) => total + row.totalAmount, 0)
  const outstandingTotal = rows.reduce(
    (total, row) => (row.isPaid ? total : total + row.totalAmount),
    0,
  )

  async function handlePay(row: FlatPayoutRow) {
    setActionError('')

    try {
      await markPayoutLinePaid(row.payoutLineId)
    } catch (error) {
      setActionError(
        getErrorMessage(error, `Could not mark ${row.employeeName} as paid.`),
      )
    }
  }

  return (
    <section className="space-y-6">
      <Card>
        <CardHeader className="flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <CardEyebrow>Payouts</CardEyebrow>
            <CardTitle className="mt-2">{formatMonthLabel(period.year, period.month)}</CardTitle>
            <CardDescription className="mt-2">
              A cross-crew view of every generated sheet's computed payout lines. Kilometres
              are reported for reference only — the amount released is the taxi money a member
              fronted. Each member is settled once per month.
            </CardDescription>
          </div>

          <MonthPicker value={period} onChange={setPeriod} />
        </CardHeader>

        {actionError && <p className="mt-4 text-sm font-medium text-red-500">{actionError}</p>}
      </Card>

      <Card>
        {isLoading ? (
          <p className="py-10 text-center text-slate-400">Loading…</p>
        ) : rows.length === 0 ? (
          <div className="flex flex-col items-center gap-3 py-12 text-center">
            <Wallet className="size-8 text-sky-300" />
            <p className="text-slate-500">
              No generated sheets for this period yet — generate one on the{' '}
              <span className="font-medium text-slate-700">Monthly Sheets</span> page.
            </p>
          </div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Employee</TableHead>
                <TableHead>Crew</TableHead>
                <TableHead className="text-right">Driven km</TableHead>
                <TableHead className="text-right">Extra business km</TableHead>
                <TableHead className="text-right">Taxi compensation</TableHead>
                <TableHead className="text-right">Total payout</TableHead>
                <TableHead className="text-right">Payment</TableHead>
                <TableHead />
              </TableRow>
            </TableHeader>
            <TableBody>
              {rows.map((row) => (
                <Fragment key={row.key}>
                  <TableRow>
                    <TableCell className="font-medium text-slate-900">{row.employeeName}</TableCell>
                    <TableCell>{row.crewName}</TableCell>
                    <TableCell className="text-right">
                      {row.driverKm > 0 ? (
                        formatKm(row.driverKm)
                      ) : (
                        <span className="text-slate-300">—</span>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      {row.extraBusinessKm > 0 ? (
                        formatKm(row.extraBusinessKm)
                      ) : (
                        <span className="text-slate-300">—</span>
                      )}
                    </TableCell>
                    <TableCell className="text-right">{formatCurrency(row.taxiCompensation)}</TableCell>
                    <TableCell className="text-right font-semibold text-slate-900">
                      {formatCurrency(row.totalAmount)}
                    </TableCell>
                    <TableCell className="text-right">
                      {row.isPaid ? (
                        <Badge variant="success">Paid</Badge>
                      ) : row.totalAmount <= 0 ? (
                        <span className="text-xs text-slate-300">Nothing due</span>
                      ) : can('payMember') ? (
                        <Button
                          type="button"
                          size="sm"
                          className="rounded-full bg-emerald-600 text-white hover:bg-emerald-700"
                          onClick={() => setPayingRow(row)}
                        >
                          <BadgeCheck className="size-3.5" />
                          Paid
                        </Button>
                      ) : (
                        <Badge variant="warning">Unpaid</Badge>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      {row.taxiExpenses.length > 0 && (
                        <Button
                          type="button"
                          variant="ghost"
                          size="icon-sm"
                          className="rounded-full text-slate-400"
                          onClick={() => setExpandedRowKey((current) => (current === row.key ? null : row.key))}
                        >
                          <ChevronDown
                            className={
                              expandedRowKey === row.key ? 'rotate-180 transition-transform' : 'transition-transform'
                            }
                          />
                        </Button>
                      )}
                    </TableCell>
                  </TableRow>

                  {expandedRowKey === row.key && row.taxiExpenses.length > 0 && (
                    <TableRow className="hover:bg-transparent">
                      <TableCell colSpan={8} className="bg-sky-50/40">
                        <div className="space-y-1.5 py-2">
                          {row.taxiExpenses.map((expense) => (
                            <div
                              key={expense.id}
                              className="flex items-center justify-between rounded-xl border border-sky-100 bg-white px-3 py-2 text-sm"
                            >
                              <span className="text-slate-600">{legLabels[expense.leg]} leg</span>
                              <span className="text-slate-600">{formatCurrency(expense.amount)}</span>
                              <Badge variant={expense.status === 'approved' ? 'success' : 'secondary'}>
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
                <TableCell className="font-semibold text-slate-900" colSpan={5}>
                  Grand total
                </TableCell>
                <TableCell className="text-right text-base font-semibold text-sky-700">
                  {formatCurrency(grandTotal)}
                </TableCell>
                <TableCell className="text-right text-xs font-normal text-slate-500" colSpan={2}>
                  Still to pay:{' '}
                  <span className="font-medium text-slate-700">{formatCurrency(outstandingTotal)}</span>
                </TableCell>
              </TableRow>
            </TableFooter>
          </Table>
        )}
      </Card>

      <ConfirmDialog
        open={payingRow !== null}
        onOpenChange={(open) => !open && setPayingRow(null)}
        title={payingRow ? `Pay ${payingRow.employeeName}?` : 'Pay this member?'}
        tone="positive"
        confirmLabel="Mark as paid"
        description={
          payingRow
            ? `This releases ${formatCurrency(payingRow.totalAmount)} for ${formatMonthLabel(
                period.year,
                period.month,
              )}. They cannot be paid again for this month.`
            : ''
        }
        onConfirm={() => {
          if (payingRow) {
            handlePay(payingRow)
          }
        }}
      />
    </section>
  )
}
