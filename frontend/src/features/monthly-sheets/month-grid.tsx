import { hasTravel } from './monthly-report'
import type { MonthlyReport } from './monthly-report'
import { formatCurrency, formatKm } from '@/lib/format'

const weekdays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']

/**
 * The month exactly as the printed report shows it: one cell is one day, marked with
 * the distance the crew drove or the taxi fare it cost. Both read the same
 * `MonthlyReport`, so the screen can never disagree with the PDF.
 */
export function MonthGrid({ report }: { report: MonthlyReport }) {
  return (
    <div className="overflow-x-auto">
      <div className="grid min-w-[640px] grid-cols-7 gap-1">
        {weekdays.map((weekday) => (
          <div
            key={weekday}
            className="px-2 py-1 text-center text-xs font-semibold uppercase tracking-wide text-slate-400"
          >
            {weekday}
          </div>
        ))}

        {report.weeks.flat().map((day, index) =>
          day === null ? (
            <div key={`blank-${index}`} />
          ) : (
            <div
              key={day.dayOfMonth}
              className={
                hasTravel(day)
                  ? 'min-h-16 rounded-xl border border-sky-200 bg-sky-50 px-2 py-1.5'
                  : 'min-h-16 rounded-xl border border-slate-100 px-2 py-1.5'
              }
            >
              <span
                className={
                  hasTravel(day)
                    ? 'text-xs font-semibold text-slate-900'
                    : 'text-xs text-slate-300'
                }
              >
                {day.dayOfMonth}
              </span>

              {day.drivenKm > 0 && (
                <p className="mt-0.5 text-xs font-medium text-slate-700">
                  {formatKm(day.drivenKm)}
                </p>
              )}
              {day.extraBusinessKm > 0 && (
                <p className="text-xs text-slate-500">
                  +{formatKm(day.extraBusinessKm)}
                </p>
              )}
              {day.taxiAmount > 0 && (
                <p className="mt-0.5 text-xs font-medium text-amber-700">
                  {formatCurrency(day.taxiAmount)}
                </p>
              )}
            </div>
          ),
        )}
      </div>
    </div>
  )
}
