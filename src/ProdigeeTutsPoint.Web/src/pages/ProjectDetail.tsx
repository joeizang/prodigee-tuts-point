import { CheckCircle2 } from 'lucide-react'
import { useEffect } from 'react'
import { Link, useParams } from 'react-router-dom'
import type { ProjectDetail as ProjectDetailModel } from '../api'
import { AsyncState } from '../components/AsyncState'
import { NotesPanel } from '../components/NotesPanel'
import { Page } from '../components/Page'
import { useApi } from '../hooks/useApi'
import { useStudyTime } from '../hooks/useStudyTime'
import { useActiveLearning } from '../state/ActiveLearningContext'
import type { LocalProfile } from '../types'

export function ProjectDetail({ profile }: { profile: LocalProfile }) {
  const { projectPath, syncTrack } = useActiveLearning()
  const fallbackProjectId = projectPath.startsWith('/projects/') ? projectPath.split('/').at(-1) ?? '' : ''
  const { projectId = fallbackProjectId } = useParams()
  useStudyTime({ profileId: profile.id, targetType: 'project', targetId: projectId })
  const { data: project, error, isLoading } = useApi<ProjectDetailModel>(
    `/api/curriculum/projects/${projectId}`,
  )

  useEffect(() => {
    if (project?.trackId) {
      syncTrack(project.trackId)
    }
  }, [project?.trackId, syncTrack])

  return (
    <Page title={project?.title ?? 'Project'}>
      <AsyncState error={error} isLoading={isLoading} />
      {project && (
        <>
          <p className="body-copy">{project.description}</p>
          <div className="item-list section-gap">
            {project.milestones.map((milestone) => (
              <Link
                className="list-row"
                key={milestone.id}
                to={`/projects/${project.id}/milestones/${milestone.id}`}
              >
                <CheckCircle2 size={18} />
                <span>{milestone.title}</span>
              </Link>
            ))}
          </div>
          <NotesPanel profile={profile} targetId={project.id} targetType="project" />
        </>
      )}
    </Page>
  )
}
