import { CheckCircle2 } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import type { ProjectDetail as ProjectDetailModel } from '../api'
import { AsyncState } from '../components/AsyncState'
import { NotesPanel } from '../components/NotesPanel'
import { Page } from '../components/Page'
import { useApi } from '../hooks/useApi'
import type { LocalProfile } from '../types'

export function ProjectDetail({ profile }: { profile: LocalProfile }) {
  const { projectId = 'wordfreq-csharp' } = useParams()
  const { data: project, error, isLoading } = useApi<ProjectDetailModel>(
    `/api/curriculum/projects/${projectId}`,
  )

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
