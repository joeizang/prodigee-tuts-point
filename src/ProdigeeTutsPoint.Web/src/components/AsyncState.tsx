export function AsyncState({ error, isLoading }: { error: Error | null; isLoading: boolean }) {
  if (isLoading) {
    return <p className="body-copy">Loading...</p>
  }

  if (error) {
    return <p className="body-copy">Could not load data: {error.message}</p>
  }

  return null
}
