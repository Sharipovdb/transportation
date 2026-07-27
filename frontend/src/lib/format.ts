// Shared formatters so every screen renders money, dates, and month labels the same way.
// Single currency (TJS) per the spec's non-goal on multi-currency.

const monthNames = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
]

export function formatCurrency(amount: number) {
  return `${amount.toLocaleString('en-US', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  })} TJS`
}

export function formatKm(km: number) {
  return `${km.toLocaleString('en-US', { maximumFractionDigits: 1 })} km`
}

// month is 1-12.
export function formatMonthLabel(year: number, month: number) {
  return `${monthNames[month - 1]} ${year}`
}

export function monthName(month: number) {
  return monthNames[month - 1]
}

// Short, human day label from an ISO date, e.g. "Wed, 01 Jul".
export function formatDayLabel(isoDate: string) {
  const date = new Date(`${isoDate}T00:00:00`)

  return date.toLocaleDateString('en-GB', {
    weekday: 'short',
    day: '2-digit',
    month: 'short',
  })
}

export function getMonthYear(isoDate: string) {
  const date = new Date(`${isoDate}T00:00:00`)

  return { year: date.getFullYear(), month: date.getMonth() + 1 }
}
