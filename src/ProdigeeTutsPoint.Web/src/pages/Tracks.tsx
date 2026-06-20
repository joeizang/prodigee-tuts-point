import { BookOpen } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { TrackSummary } from '../api'
import { AsyncState } from '../components/AsyncState'
import { Page } from '../components/Page'
import { useApi } from '../hooks/useApi'

export function Tracks({ selectedTrackId }: { selectedTrackId: string }) {
  const { data: tracks, error, isLoading } = useApi<TrackSummary[]>('/api/curriculum/tracks')

  return (
    <Page title="Tracks">
      <AsyncState error={error} isLoading={isLoading} />
      <div className="item-list">
        {(tracks ?? []).map((track) => {
          const isActive = track.id === selectedTrackId

          return (
            <Link
              aria-current={isActive ? 'true' : undefined}
              className={isActive ? 'list-row track-row active-track-row' : 'list-row track-row'}
              key={track.id}
              to={`/tracks/${track.id}`}
            >
              <BookOpen size={18} />
              <span className="track-row-copy">
                <strong>{track.title}</strong>
                <small>{track.description}</small>
              </span>
              {isActive && <span className="active-track-token">Active</span>}
            </Link>
          )
        })}
      </div>
    </Page>
  )
}
