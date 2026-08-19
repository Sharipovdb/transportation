import { jsPDF } from 'jspdf'
import autoTable from 'jspdf-autotable'

import { hasTravel } from './monthly-report'
import type { MonthlyReport } from './monthly-report'
import { formatCurrency, formatKm, formatMonthLabel } from '@/lib/format'

const margin = 14
const ink = '#0f172a'
const muted = '#64748b'
const accent = '#0284c7'
const travelledFill = '#e0f2fe'
const gridLine = '#cbd5e1'

const weekdays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']

function fileName(report: MonthlyReport) {
  const crew = report.crewName.trim().replace(/\s+/g, '-').toLowerCase()
  const month = String(report.month).padStart(2, '0')

  return `transport-report-${crew || 'crew'}-${report.year}-${month}.pdf`
}

/**
 * Renders one crew's month as a printable, signable A4 sheet.
 *
 * The month is a calendar: one cell is one day, marked with the distance the crew drove
 * that day or the taxi fare it cost. Legs are not broken out — a day is the unit the
 * accountant prices — and neither is who fronted a fare, since the whole amount is
 * handed to the lead to distribute.
 *
 * Returning the document rather than saving it is what lets the layout be rendered and
 * inspected outside a browser.
 */
export function renderMonthlyReportPdf(report: MonthlyReport) {
  const doc = new jsPDF({ unit: 'mm', format: 'a4' })
  const pageWidth = doc.internal.pageSize.getWidth()

  let cursor = drawHeader(doc, report, pageWidth)
  cursor = drawCalendar(doc, report, pageWidth, cursor)
  cursor = drawSummary(doc, report, pageWidth, cursor)

  drawSignatures(doc, pageWidth, cursor)

  return doc
}

/** Renders the report and hands it to the browser as a download. */
export function downloadMonthlyReportPdf(report: MonthlyReport) {
  renderMonthlyReportPdf(report).save(fileName(report))
}

function statusLabel(report: MonthlyReport) {
  if (report.isPaid) return 'Paid'

  return report.isConfirmed ? 'Confirmed — not paid yet' : 'Draft'
}

function drawHeader(doc: jsPDF, report: MonthlyReport, pageWidth: number) {
  doc.setFont('helvetica', 'bold').setFontSize(16).setTextColor(ink)
  doc.text('Crew Transport Report', margin, 20)

  doc.setFont('helvetica', 'normal').setFontSize(10).setTextColor(muted)
  doc.text(formatMonthLabel(report.year, report.month), margin, 27)

  doc.setDrawColor(accent).setLineWidth(0.6)
  doc.line(margin, 31, pageWidth - margin, 31)

  const facts: [string, string][] = [
    ['Crew', report.crewName],
    ['Paid to', report.recipientFullname],
    ['Period', formatMonthLabel(report.year, report.month)],
    ['Status', statusLabel(report)],
  ]

  facts.forEach(([label, value], index) => {
    const column = index % 2
    const row = Math.floor(index / 2)
    const x = margin + column * ((pageWidth - margin * 2) / 2)
    const y = 39 + row * 6

    doc.setFont('helvetica', 'normal').setFontSize(9).setTextColor(muted)
    doc.text(`${label}:`, x, y)

    doc.setFont('helvetica', 'bold').setTextColor(ink)
    doc.text(value, x + 22, y)
  })

  return 52
}

/** Day number on top, then what the day is worth — distance, money, or both. */
function cellText(cell: MonthlyReport['weeks'][number][number]) {
  if (cell === null) return ''

  const lines = [String(cell.dayOfMonth)]

  if (cell.drivenKm > 0) lines.push(formatKm(cell.drivenKm))
  if (cell.extraBusinessKm > 0) lines.push(`+${formatKm(cell.extraBusinessKm)}`)
  if (cell.taxiAmount > 0) lines.push(formatCurrency(cell.taxiAmount))

  return lines.join('\n')
}

function drawCalendar(
  doc: jsPDF,
  report: MonthlyReport,
  pageWidth: number,
  startY: number,
) {
  const columnWidth = (pageWidth - margin * 2) / weekdays.length

  autoTable(doc, {
    startY,
    head: [weekdays],
    body: report.weeks.map((week) => week.map(cellText)),
    theme: 'grid',
    styles: {
      font: 'helvetica',
      fontSize: 8,
      cellPadding: 1.5,
      minCellHeight: 15,
      valign: 'top',
      textColor: ink,
      lineColor: gridLine,
      cellWidth: columnWidth,
    },
    headStyles: {
      fillColor: '#f1f5f9',
      textColor: muted,
      fontStyle: 'bold',
      halign: 'center',
      minCellHeight: 7,
    },
    // A day the crew travelled is shaded, so the month reads at a glance as the set of
    // marked cells the report is actually about.
    didParseCell: ({ section, row, column, cell }) => {
      if (section !== 'body') return

      const day = report.weeks[row.index][column.index]

      if (day !== null && hasTravel(day)) {
        cell.styles.fillColor = travelledFill
        cell.styles.fontStyle = 'bold'
      }
    },
    margin: { left: margin, right: margin },
  })

  return getTableEndY(doc) + 10
}

function drawSummary(
  doc: jsPDF,
  report: MonthlyReport,
  pageWidth: number,
  startY: number,
) {
  const lines: [string, string][] = [
    ['Days travelled', String(report.travelledDayCount)],
    ['Distance driven (round trip)', formatKm(report.totalDrivenKm)],
    ['Extra business distance', formatKm(report.totalExtraBusinessKm)],
    ['Taxi fares to reimburse', formatCurrency(report.totalTaxiAmount)],
  ]

  doc.setFont('helvetica', 'bold').setFontSize(11).setTextColor(ink)
  doc.text('Monthly summary', margin, startY)

  let y = startY + 7

  lines.forEach(([label, value]) => {
    doc.setFont('helvetica', 'normal').setFontSize(10).setTextColor(muted)
    doc.text(label, margin, y)

    doc.setFont('helvetica', 'bold').setTextColor(ink)
    doc.text(value, pageWidth - margin, y, { align: 'right' })

    y += 6
  })

  doc.setDrawColor(gridLine).setLineWidth(0.2)
  doc.line(margin, y, pageWidth - margin, y)
  y += 6

  doc.setFont('helvetica', 'bold').setFontSize(11).setTextColor(ink)
  doc.text('Amount payable', margin, y)
  doc.setTextColor(accent)
  doc.text(formatCurrency(report.totalTaxiAmount), pageWidth - margin, y, {
    align: 'right',
  })

  y += 6
  doc.setFont('helvetica', 'italic').setFontSize(8).setTextColor(muted)
  doc.text(
    'Distance is reported for the accountant to price; only taxi fares are payable on this sheet.',
    margin,
    y,
  )

  return y + 14
}

function drawSignatures(doc: jsPDF, pageWidth: number, startY: number) {
  const columnWidth = (pageWidth - margin * 2 - 10) / 2
  const labels = ['Crew lead signature', 'Approved by']

  labels.forEach((label, index) => {
    const x = margin + index * (columnWidth + 10)

    doc.setDrawColor('#94a3b8').setLineWidth(0.3)
    doc.line(x, startY, x + columnWidth, startY)

    doc.setFont('helvetica', 'normal').setFontSize(9).setTextColor(muted)
    doc.text(label, x, startY + 5)
  })

  const dateY = startY + 18
  doc.setDrawColor('#94a3b8')
  doc.line(margin, dateY, margin + columnWidth, dateY)
  doc.text('Date', margin, dateY + 5)
}

// autoTable records where it stopped on the document; typings expose it as an optional
// field, so it is read through one helper instead of casting at every call site.
function getTableEndY(doc: jsPDF) {
  return (doc as jsPDF & { lastAutoTable?: { finalY: number } }).lastAutoTable?.finalY ?? 0
}
