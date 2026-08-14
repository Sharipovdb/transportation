import { jsPDF } from 'jspdf'
import autoTable from 'jspdf-autotable'

import type { PayoutReport } from './payout-report'
import { transportModeLabels } from '@/lib/domain-types'
import { formatCurrency, formatKm, formatMonthLabel } from '@/lib/format'

const margin = 14
const ink = '#0f172a'
const muted = '#64748b'
const accent = '#0284c7'

function weekday(isoDate: string) {
  return new Date(`${isoDate}T00:00:00`).toLocaleDateString('en-GB', {
    weekday: 'short',
  })
}

function dayOfMonth(isoDate: string) {
  return new Date(`${isoDate}T00:00:00`).toLocaleDateString('en-GB', {
    day: '2-digit',
    month: 'short',
  })
}

function fileName(report: PayoutReport) {
  const employee = report.fullname.trim().replace(/\s+/g, '-').toLowerCase()
  const month = String(report.month).padStart(2, '0')

  return `transport-report-${employee || 'employee'}-${report.year}-${month}.pdf`
}

/** Renders one member's month as a printable, signable A4 sheet and downloads it. */
export function downloadPayoutReportPdf(report: PayoutReport) {
  const doc = new jsPDF({ unit: 'mm', format: 'a4' })
  const pageWidth = doc.internal.pageSize.getWidth()

  let cursor = drawHeader(doc, report, pageWidth)
  cursor = drawDailyTable(doc, report, cursor)
  cursor = drawSummary(doc, report, pageWidth, cursor)

  drawSignatures(doc, pageWidth, cursor)

  doc.save(fileName(report))
}

function drawHeader(doc: jsPDF, report: PayoutReport, pageWidth: number) {
  doc.setFont('helvetica', 'bold').setFontSize(16).setTextColor(ink)
  doc.text('Employee Transportation Report', margin, 20)

  doc.setFont('helvetica', 'normal').setFontSize(10).setTextColor(muted)
  doc.text(formatMonthLabel(report.year, report.month), margin, 27)

  doc.setDrawColor(accent).setLineWidth(0.6)
  doc.line(margin, 31, pageWidth - margin, 31)

  const facts: [string, string][] = [
    ['Employee', report.fullname],
    ['Crew', report.crewName],
    ['Period', formatMonthLabel(report.year, report.month)],
    ['Payment status', report.isPaid ? 'Paid' : 'Not paid yet'],
  ]

  facts.forEach(([label, value], index) => {
    const column = index % 2
    const row = Math.floor(index / 2)
    const x = margin + column * ((pageWidth - margin * 2) / 2)
    const y = 39 + row * 6

    doc.setFont('helvetica', 'normal').setFontSize(9).setTextColor(muted)
    doc.text(`${label}:`, x, y)

    doc.setFont('helvetica', 'bold').setTextColor(ink)
    doc.text(value, x + 26, y)
  })

  return 52
}

function drawDailyTable(doc: jsPDF, report: PayoutReport, startY: number) {
  const body = report.rows.map((row) => [
    dayOfMonth(row.date),
    weekday(row.date),
    transportModeLabels[row.morningMode],
    row.afternoonMode ? transportModeLabels[row.afternoonMode] : '—',
    row.drivenKm > 0 ? formatKm(row.drivenKm) : '—',
    row.extraBusinessKm > 0 ? formatKm(row.extraBusinessKm) : '—',
    row.taxiAmount > 0 ? formatCurrency(row.taxiAmount) : '—',
  ])

  autoTable(doc, {
    startY,
    head: [
      [
        'Date',
        'Day',
        'Morning',
        'Afternoon',
        'Driven km',
        'Extra business km',
        'Taxi fare',
      ],
    ],
    body:
      body.length > 0
        ? body
        : [
            [
              {
                content: 'No confirmed travel recorded for this month.',
                colSpan: 7,
              },
            ],
          ],
    foot: body.length > 0 ? [buildTotalsRow(report)] : undefined,
    theme: 'grid',
    styles: {
      font: 'helvetica',
      fontSize: 9,
      cellPadding: 2,
      textColor: ink,
      lineColor: '#cbd5e1',
    },
    headStyles: { fillColor: '#e0f2fe', textColor: ink, fontStyle: 'bold' },
    footStyles: { fillColor: '#f1f5f9', textColor: ink, fontStyle: 'bold' },
    columnStyles: {
      4: { halign: 'right' },
      5: { halign: 'right' },
      6: { halign: 'right' },
    },
    margin: { left: margin, right: margin },
  })

  return getTableEndY(doc) + 10
}

function buildTotalsRow(report: PayoutReport) {
  return [
    { content: 'Total', colSpan: 4 },
    formatKm(report.totalDrivenKm),
    formatKm(report.totalExtraBusinessKm),
    formatCurrency(report.totalTaxiAmount),
  ]
}

function drawSummary(
  doc: jsPDF,
  report: PayoutReport,
  pageWidth: number,
  startY: number,
) {
  const lines: [string, string][] = [
    ['Distance driven in own car', formatKm(report.totalDrivenKm)],
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

  doc.setDrawColor('#cbd5e1').setLineWidth(0.2)
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
    'Distance is reported for the record and settled separately; only taxi fares are payable on this sheet.',
    margin,
    y,
  )

  return y + 14
}

function drawSignatures(doc: jsPDF, pageWidth: number, startY: number) {
  const columnWidth = (pageWidth - margin * 2 - 10) / 2
  const labels = ['Employee signature', 'Approved by']

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
  return (
    (doc as jsPDF & { lastAutoTable?: { finalY: number } }).lastAutoTable
      ?.finalY ?? 0
  )
}
