import { useEffect } from 'react'
import { Play } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import type { TrackDetail as TrackDetailModel } from '../api'
import { AsyncState } from '../components/AsyncState'
import { Page } from '../components/Page'
import { useApi } from '../hooks/useApi'

export function TrackDetail({ onTrackSelected }: { onTrackSelected?: (trackId: string) => void }) {
  const { trackId = 'csharp' } = useParams()
  const { data: track, error, isLoading } = useApi<TrackDetailModel>(
    `/api/curriculum/tracks/${trackId}`,
  )

  useEffect(() => {
    if (track?.id) {
      onTrackSelected?.(track.id)
    }
  }, [onTrackSelected, track?.id])

  return (
    <Page title={track?.title ?? 'Track'}>
      <AsyncState error={error} isLoading={isLoading} />
      {track && (
        <>
          <p className="body-copy">{track.description}</p>
          <div className="active-track-banner" aria-label={`Current active track is ${track.title}`}>
            <span>Active track</span>
            <strong>{track.title}</strong>
          </div>
          <div className="item-list section-gap">
            {track.projects.map((project) => (
              <Link className="list-row" key={project.id} to={`/projects/${project.id}`}>
                <Play size={18} />
                <span>{project.title}</span>
              </Link>
            ))}
          </div>
        </>
      )}
    </Page>
  )
}
