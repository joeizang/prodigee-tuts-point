import { useEffect } from 'react'
import { useParams } from 'react-router-dom'
import type { LessonDetail as LessonDetailModel } from '../api'
import { AsyncState } from '../components/AsyncState'
import { MarkdownText } from '../components/MarkdownText'
import { NotesPanel } from '../components/NotesPanel'
import { Page } from '../components/Page'
import { SourceList } from '../components/PrimitiveLists'
import { SoftLockNotice } from '../components/SoftLockNotice'
import { useApi } from '../hooks/useApi'
import { useStudyTime } from '../hooks/useStudyTime'
import { useActiveLearning } from '../state/ActiveLearningContext'
import type { LocalProfile } from '../types'

export function LessonDetail({ profile }: { profile: LocalProfile }) {
  const { lessonPath, syncTrack } = useActiveLearning()
  const fallbackLessonId = lessonPath.startsWith('/lessons/') ? lessonPath.split('/').at(-1) ?? '' : ''
  const { lessonId = fallbackLessonId } = useParams()
  const { data: lesson, error, isLoading } = useApi<LessonDetailModel>(
    `/api/curriculum/lessons/${lessonId}`,
  )
  useStudyTime({ profileId: profile.id, targetType: 'lesson', targetId: lessonId })

  useEffect(() => {
    if (lesson?.trackId) {
      syncTrack(lesson.trackId)
    }
  }, [lesson?.trackId, syncTrack])

  return (
    <Page title={lesson?.title ?? 'Lesson'}>
      <AsyncState error={error} isLoading={isLoading} />
      {lesson && (
        <div className="content-stack">
          <SoftLockNotice locks={lesson.softLocks} title="Recommended primer" />
          <MarkdownText markdown={lesson.markdown} omitFirstHeading />
          <SourceList sources={lesson.sources} />
          <NotesPanel profile={profile} targetId={lesson.id} targetType="lesson" />
        </div>
      )}
    </Page>
  )
}
