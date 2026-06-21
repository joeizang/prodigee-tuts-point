import { BookOpen } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getJson, type MilestoneDetail } from '../api'
import { AsyncState } from '../components/AsyncState'
import { Page } from '../components/Page'
import { useActiveLearning } from '../state/ActiveLearningContext'

export function Lessons() {
  const { activeTrack, primaryProjectDetail } = useActiveLearning()
  const [groups, setGroups] = useState<MilestoneDetail[]>([])
  const [error, setError] = useState<Error | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  useEffect(() => {
    if (!primaryProjectDetail) {
      setGroups([])
      setError(null)
      setIsLoading(false)
      return
    }

    const controller = new AbortController()
    setGroups([])
    setError(null)
    setIsLoading(true)

    Promise.all(
      primaryProjectDetail.milestones.map((milestone) =>
        getJson<MilestoneDetail>(
          `/api/curriculum/projects/${encodeURIComponent(primaryProjectDetail.id)}/milestones/${encodeURIComponent(milestone.id)}`,
          controller.signal,
        ),
      ),
    )
      .then((milestones) => {
        setGroups(milestones.filter((milestone) => milestone.lessons.length > 0))
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
  }, [primaryProjectDetail])

  return (
    <Page title="Lessons">
      <p className="body-copy">
        {activeTrack
          ? `${activeTrack.title} lessons, ordered by the current learning path.`
          : 'Lessons for the active track, ordered by the current learning path.'}
      </p>
      <AsyncState error={error} isLoading={isLoading} />
      <div className="lesson-group-list section-gap">
        {groups.map((group) => (
          <section className="lesson-group" key={group.id}>
            <div className="lesson-group-heading">
              <div>
                <h3>{group.title}</h3>
                <p>{group.summary}</p>
              </div>
              <span>{group.lessons.length} lesson{group.lessons.length === 1 ? '' : 's'}</span>
            </div>
            <div className="item-list">
              {group.lessons.map((lesson) => (
                <Link className="list-row" key={lesson.id} to={`/lessons/${lesson.id}`}>
                  <BookOpen size={18} />
                  <span>
                    <strong>{lesson.title}</strong>
                    <small>{lesson.summary}</small>
                  </span>
                  <small className="lesson-card-action">Start</small>
                </Link>
              ))}
            </div>
          </section>
        ))}
      </div>
    </Page>
  )
}
