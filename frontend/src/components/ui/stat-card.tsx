import type { LucideIcon } from 'lucide-react'

import { Card } from '@/components/ui/card'

interface StatCardProps {
  label: string
  value: string | number
  icon: LucideIcon
  hint?: string
}

export function StatCard({ label, value, icon: Icon, hint }: StatCardProps) {
  return (
    <Card className="p-5">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="text-sm text-slate-500">{label}</p>
          <p className="mt-2 truncate text-2xl font-semibold text-slate-900">{value}</p>
          {hint && <p className="mt-1 text-xs text-slate-400">{hint}</p>}
        </div>

        <span className="flex size-11 shrink-0 items-center justify-center rounded-2xl bg-sky-100 text-sky-700">
          <Icon className="size-5" />
        </span>
      </div>
    </Card>
  )
}
