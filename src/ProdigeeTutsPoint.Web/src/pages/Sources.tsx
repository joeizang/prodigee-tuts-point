import type { SourceBook } from '../api'
import { AsyncState } from '../components/AsyncState'
import { NotesPanel } from '../components/NotesPanel'
import { Page, Panel } from '../components/Page'
import { useApi } from '../hooks/useApi'
import type { LocalProfile } from '../types'

export function Sources({ profile }: { profile: LocalProfile }) {
  const { data: books, error, isLoading } = useApi<SourceBook[]>('/api/curriculum/sources')

  return (
    <Page title="Sources">
      <AsyncState error={error} isLoading={isLoading} />
      <div className="content-stack">
        {(books ?? []).map((book) => (
          <Panel key={book.id} title={book.title}>
            <p className="body-copy">
              {book.author}
              {book.edition ? ` - ${book.edition} edition` : ''}
              {book.publisher ? ` - ${book.publisher}` : ''} - {book.ownershipStatus}
            </p>
            <div className="source-reference-list">
              {book.references.map((reference) => (
                <section className="source-reference-row" id={reference.id} key={reference.id}>
                  <div>
                    <strong>{reference.topic}</strong>
                    <small>
                      {reference.chapter ? `${reference.chapter} - ` : ''}
                      {reference.usage}
                    </small>
                  </div>
                  <NotesPanel
                    compact
                    profile={profile}
                    targetId={reference.id}
                    targetType="sourcereference"
                  />
                </section>
              ))}
            </div>
          </Panel>
        ))}
      </div>
    </Page>
  )
}
