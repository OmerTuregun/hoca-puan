import { useEffect, useState } from 'react'
import { loadBannedWordPatterns, mayContainBannedContent } from '../utils/contentModeration'

export function useCommentModerationHint(comment: string, debounceMs = 500) {
  const [showHint, setShowHint] = useState(false)

  useEffect(() => {
    const trimmed = comment.trim()
    if (!trimmed) {
      setShowHint(false)
      return
    }

    const timer = window.setTimeout(async () => {
      const patterns = await loadBannedWordPatterns()
      setShowHint(mayContainBannedContent(trimmed, patterns))
    }, debounceMs)

    return () => window.clearTimeout(timer)
  }, [comment, debounceMs])

  return showHint
}
