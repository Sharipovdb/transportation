import { createFileRoute } from '@tanstack/react-router'
import { ChevronDown, Wallet } from 'lucide-react'
import { Fragment, useMemo, useState } from 'react'

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
import { useCrews } from '@/features/crews/crews-context'
import { useMonthlySheets } from '@/features/monthly-sheets/monthly-sheets-context'
import { legLabels, taxiExpenseStatusLabels } from '@/lib/domain-types'
import type { PayoutLineTaxiExpense } from '@/lib/domain-types'
import { formatCurrency, formatMonthLabel, getMonthYear } from '@/lib/format'

export const Route = createFileRoute('/payouts')({
  component: PayoutsPage,
})

function todayIsoDate() {
  return new Date().toISOString().slice(0, 10)
}

interface FlatPayoutRow {
  key: string
  crewName: string
  employeeId: string
  employeeName: string
  driverPayment: number
  extraKmPayment: number
  taxiCompensation: number
  totalAmount: number
  taxiExpenses: PayoutLineTaxiExpense[]
}

function PayoutsPage() {
  const { crews } = useCrews()
  const { sheets, isLoading } = useMonthlySheets()

  const [period, setPeriod] = useState<Period>(() => getMonthYear(todayIsoDate()))
  const [expandedRowKey, setExpandedRowKey] = useState<string | null>(null)

  const rows = useMemo<FlatPayoutRow[]>(() => {
    const sheetsInPeriod = sheets.filter((sheet) => sheet.year === period.year && sheet.month === period.month)

    return sheetsInPeriod.flatMap((sheet) => {
      const crewName = crews.find((crew) => crew.id === sheet.crewId)?.name ?? 'Unknown crew'

      return sheet.payoutLines.map((line) => ({
        key: `${sheet.id}-${line.id}`,
        crewName,
        employeeId: line.employeeId,
        employeeName: line.employeeName,
        driverPayment: line.driverPayment,
        extraKmPayment: line.extraKmPayment,
        taxiCompensation: line.taxiCompensation,
        totalAmount: line.totalAmount,
        taxiExpenses: line.taxiExpenses,
      }))
    })
  }, [sheets, crews, period])

  const grandTotal = rows.reduce((total, row) => total + row.totalAmount, 0)

  return (
    <section className="space-y-6">
      <Card>
        <CardHeader className="flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <CardEyebrow>Payouts</CardEyebrow>
            <CardTitle className="mt-2">{formatMonthLabel(period.year, period.month)}</CardTitle>
            <CardDescription className="mt-2">
              A cross-crew view of every generated sheet's computed payout lines. Generate and
              confirm sheets from the Monthly Sheets page — this view is read-only.
            </CardDescription>
          </div>

          <MonthPicker value={period} onChange={setPeriod} />
        </CardHeader>
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
                <TableHead className="text-right">Driver payment</TableHead>
                <TableHead className="text-right">Extra km payment</TableHead>
                <TableHead className="text-right">Taxi compensation</TableHead>
                <TableHead className="text-right">Total</TableHead>
                <TableHead />
              </TableRow>
            </TableHeader>
            <TableBody>
              {rows.map((row) => (
                <Fragment key={row.key}>
                  <TableRow>
                    <TableCell className="font-medium text-slate-900">{row.employeeName}</TableCell>
                    <TableCell>{row.crewName}</TableCell>
                    <TableCell className="text-right">{formatCurrency(row.driverPayment)}</TableCell>
                    <TableCell className="text-right">{formatCurrency(row.extraKmPayment)}</TableCell>
                    <TableCell className="text-right">{formatCurrency(row.taxiCompensation)}</TableCell>
                    <TableCell className="text-right font-semibold text-slate-900">
                      {formatCurrency(row.totalAmount)}
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
                      <TableCell colSpan={7} className="bg-sky-50/40">
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
                <TableCell />
              </TableRow>
            </TableFooter>
          </Table>
        )}
      </Card>
    </section>
  )
}
