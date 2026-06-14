import { useEffect, useState } from 'react'
import { getJson } from '../api'

export function useApi<T>(url: string | null) {
  const [data, setData] = useState<T | null>(null)
  const [error, setError] = useState<Error | null>(null)
  const [isLoading, setIsLoading] = useState(url !== null)

  useEffect(() => {
    if (url === null) {
      return
    }

    const controller = new AbortController()

    getJson<T>(url, controller.signal)
      .then((response) => {
        setData(response)
        setError(null)
      })
      .catch((requestError: unknown) => {
        if (!controller.signal.aborted) {
          setError(requestError instanceof Error ? requestError : new Error('Unknown error'))
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setIsLoading(false)
        }
      })

    return () => controller.abort()
  }, [url])

  return { data, error, isLoading }
}
