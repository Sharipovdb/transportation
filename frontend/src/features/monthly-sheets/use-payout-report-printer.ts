import { useCallback } from 'react'

import { buildPayoutReport } from './payout-report'
import { useCrews } from '@/features/crews/crews-context'
import { useTaxiExpenses } from '@/features/taxi-expenses/taxi-expenses-context'
import { useTransportDays } from '@/features/transport-days/transport-days-context'
import type { MonthlySheet, PayoutLine } from '@/lib/domain-types'

/**
 * Gives a screen one function: print this member's month. Collecting the transport days
 * and taxi expenses behind a payout line needs three contexts, and that wiring belongs
 * here rather than in every table that grows a print button.
 */
export function usePayoutReportPrinter() {
  const { getCrewName } = useCrews()
  const { getDaysForCrewMonth } = useTransportDays()
  const { taxiExpenses } = useTaxiExpenses()

  return useCallback(
    async (sheet: MonthlySheet, line: PayoutLine) => {
      const report = buildPayoutReport({
        line,
        crewName: getCrewName(sheet.crewId),
        year: sheet.year,
        month: sheet.month,
        days: getDaysForCrewMonth(sheet.crewId, sheet.year, sheet.month),
        taxiExpenses,
      })

      // The PDF engine is far heavier than the page that offers the button, so it is
      // fetched on the first print rather than on every visit to Monthly Sheets.
      const { downloadPayoutReportPdf } = await import('./payout-report-pdf')

      downloadPayoutReportPdf(report)
    },
    [getCrewName, getDaysForCrewMonth, taxiExpenses],
  )
}
