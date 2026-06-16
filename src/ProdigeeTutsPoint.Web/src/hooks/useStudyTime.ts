import { useEffect, useRef } from 'react'
import { postJson } from '../api'

const idleTimeoutMs = 90_000
const flushEveryMs = 60_000
const maxFlushSeconds = 15 * 60

export function useStudyTime({
  profileId,
  targetType,
  targetId,
}: {
  profileId: string
  targetType: string
  targetId: string
}) {
  const lastActivityRef = useRef(0)
  const activeSecondsRef = useRef(0)
  const startedAtRef = useRef('')

  useEffect(() => {
    lastActivityRef.current = Date.now()
    activeSecondsRef.current = 0
    startedAtRef.current = new Date().toISOString()

    const markActive = () => {
      lastActivityRef.current = Date.now()
    }
    const flush = async () => {
      const activeSeconds = Math.min(activeSecondsRef.current, maxFlushSeconds)
      if (activeSeconds < 5) {
        return
      }

      activeSecondsRef.current = 0
      const endedAt = new Date().toISOString()
      const startedAt = startedAtRef.current
      startedAtRef.current = endedAt
      await postJson('/api/learner/study-time', {
        profileId,
        targetType,
        targetId,
        activeSeconds,
        startedAt,
        endedAt,
      }).catch(() => undefined)
    }

    const interval = window.setInterval(() => {
      if (Date.now() - lastActivityRef.current <= idleTimeoutMs) {
        activeSecondsRef.current += 1
      }
    }, 1000)
    const flushInterval = window.setInterval(() => {
      void flush()
    }, flushEveryMs)

    window.addEventListener('pointerdown', markActive)
    window.addEventListener('keydown', markActive)
    window.addEventListener('scroll', markActive, { passive: true })

    return () => {
      window.clearInterval(interval)
      window.clearInterval(flushInterval)
      window.removeEventListener('pointerdown', markActive)
      window.removeEventListener('keydown', markActive)
      window.removeEventListener('scroll', markActive)
      void flush()
    }
  }, [profileId, targetId, targetType])
}
