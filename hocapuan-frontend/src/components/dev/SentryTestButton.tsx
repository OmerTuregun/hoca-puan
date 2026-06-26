export default function SentryTestButton() {
  if (!import.meta.env.DEV) return null

  return (
    <button
      type="button"
      onClick={() => {
        throw new Error('Sentry test')
      }}
      className="fixed bottom-4 right-4 z-50 rounded bg-red-600 px-3 py-1.5 text-xs font-medium text-white shadow hover:bg-red-700"
      title="Sentry entegrasyonunu test et (yalnızca development)"
    >
      Sentry Test
    </button>
  )
}
