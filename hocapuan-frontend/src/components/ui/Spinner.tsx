import clsx from 'clsx'

export default function Spinner({ className }: { className?: string }) {
  return (
    <div className={clsx('flex items-center justify-center py-12', className)}>
      <div className="w-8 h-8 border-2 border-surface-border border-t-primary rounded-full animate-spin" />
    </div>
  )
}
