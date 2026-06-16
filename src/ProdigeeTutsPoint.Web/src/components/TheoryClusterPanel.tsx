import { BookOpen, Library } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { TheoryCluster } from '../api'
import { Panel } from './Page'

export function TheoryClusterPanel({ cluster }: { cluster: TheoryCluster }) {
  return (
    <Panel title="Study Links">
      <div className="theory-cluster-list">
        {cluster.items.map((item, index) => (
          <article className="theory-cluster-row" key={item.lessonId}>
            <Link className="theory-lesson-link" to={`/lessons/${item.lessonId}`}>
              <span>{String(index + 1).padStart(2, '0')}</span>
              <strong>{item.title}</strong>
              <small>{item.summary}</small>
            </Link>
            {item.sources.length > 0 && (
              <div className="theory-source-links" aria-label={`Source anchors for ${item.title}`}>
                {item.sources.map((source) => (
                  <Link key={source.id} to={`/sources#${source.id}`}>
                    <Library size={13} />
                    {source.bookTitle}
                    {source.chapter ? ` - ${source.chapter}` : ''}
                  </Link>
                ))}
              </div>
            )}
          </article>
        ))}
      </div>
    </Panel>
  )
}

export function ExerciseFirstLoopPanel({
  exercises,
  lessons,
}: {
  exercises: Array<{ id: string; title: string; summary: string }>
  lessons: Array<{ id: string; title: string; summary: string }>
}) {
  const lessonWindowSize = Math.max(1, Math.ceil(lessons.length / Math.max(1, exercises.length)))

  return (
    <Panel title="Exercise-First Loop">
      <div className="exercise-loop-list">
        {exercises.map((exercise, index) => {
          const linkedLessons = lessons.slice(
            Math.min(index * lessonWindowSize, Math.max(0, lessons.length - 1)),
            Math.min((index + 1) * lessonWindowSize, lessons.length),
          )
          const studyLinks = linkedLessons.length > 0 ? linkedLessons : lessons.slice(-1)

          return (
            <article className="exercise-loop-row" key={exercise.id}>
              <div className="exercise-loop-index">{String(index + 1).padStart(2, '0')}</div>
              <div>
                <Link className="exercise-loop-title" to={`/exercises/${exercise.id}`}>
                  <BookOpen size={16} />
                  {exercise.title}
                </Link>
                <p>{exercise.summary}</p>
                <div className="exercise-loop-study-links" aria-label={`Study before ${exercise.title}`}>
                  {studyLinks.map((lesson) => (
                    <Link key={lesson.id} to={`/lessons/${lesson.id}`}>
                      {lesson.title}
                    </Link>
                  ))}
                </div>
              </div>
            </article>
          )
        })}
      </div>
    </Panel>
  )
}
