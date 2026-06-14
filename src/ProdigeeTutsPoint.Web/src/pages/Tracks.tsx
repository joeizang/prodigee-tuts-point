import { BookOpen } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { TrackSummary } from '../api'
import { AsyncState } from '../components/AsyncState'
import { Page } from '../components/Page'
import { useApi } from '../hooks/useApi'

export function Tracks() {
  const { data: tracks, error, isLoading } = useApi<TrackSummary[]>('/api/curriculum/tracks')

  return (
    <Page title="Tracks">
      <AsyncState error={error} isLoading={isLoading} />
      <div className="item-list">
        {(tracks ?? []).map((track) => (
          <Link className="list-row" key={track.id} to={`/tracks/${track.id}`}>
            <BookOpen size={18} />
            <span>{track.title}</span>
          </Link>
        ))}
      </div>
    </Page>
  )
}
