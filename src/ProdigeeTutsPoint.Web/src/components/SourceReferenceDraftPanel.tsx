import { useEffect, useMemo, useState } from 'react'
import { Save } from 'lucide-react'
import type { SourceBook } from '../api'
import { Panel } from './Page'

const storageKey = 'prodigee.sourceReferenceDrafts'

type SourceReferenceDraft = {
  book: string
  chapter: string
  topic: string
  usage: string
  ownerId: string
  ownerType: 'lesson' | 'milestone'
}

const emptyDraft: SourceReferenceDraft = {
  book: '',
  chapter: '',
  topic: '',
  usage: 'QualityAnchor',
  ownerId: '',
  ownerType: 'lesson',
}

export function SourceReferenceDraftPanel({ books }: { books: SourceBook[] }) {
  const [draft, setDraft] = useState<SourceReferenceDraft>(() => ({
    ...emptyDraft,
    book: books[0]?.id ?? '',
  }))
  const [savedDrafts, setSavedDrafts] = useState<SourceReferenceDraft[]>(readDrafts)
  const yaml = useMemo(() => buildYaml(draft), [draft])

  useEffect(() => {
    if (books.length === 0) {
      return
    }

    if (!books.some((book) => book.id === draft.book)) {
      setDraft((current) => ({ ...current, book: books[0]?.id ?? '' }))
    }
  }, [books, draft.book])

  const saveDraft = () => {
    const next = [{ ...draft }, ...savedDrafts].slice(0, 8)
    setSavedDrafts(next)
    localStorage.setItem(storageKey, JSON.stringify(next))
  }

  return (
    <Panel title="Source Reference Drafts">
      <div className="source-draft-grid">
        <label>
          Book
          <select
            value={draft.book}
            onChange={(event) => setDraft({ ...draft, book: event.target.value })}
          >
            {books.map((book) => (
              <option key={book.id} value={book.id}>
                {book.title}
              </option>
            ))}
          </select>
        </label>
        <label>
          Target type
          <select
            value={draft.ownerType}
            onChange={(event) =>
              setDraft({ ...draft, ownerType: event.target.value as SourceReferenceDraft['ownerType'] })
            }
          >
            <option value="lesson">Lesson</option>
            <option value="milestone">Milestone</option>
          </select>
        </label>
        <label>
          Target id
          <input
            value={draft.ownerId}
            onChange={(event) => setDraft({ ...draft, ownerId: event.target.value })}
            placeholder="lesson-or-milestone-id"
          />
        </label>
        <label>
          Chapter
          <input
            value={draft.chapter}
            onChange={(event) => setDraft({ ...draft, chapter: event.target.value })}
            placeholder="Chapter or section"
          />
        </label>
        <label className="source-draft-wide">
          Topic
          <input
            value={draft.topic}
            onChange={(event) => setDraft({ ...draft, topic: event.target.value })}
            placeholder="Precise study topic, not copied book text"
          />
        </label>
      </div>
      <pre className="source-draft-yaml">{yaml}</pre>
      <button className="secondary-action" type="button" onClick={saveDraft}>
        <Save size={16} />
        Save draft locally
      </button>
      {savedDrafts.length > 0 && (
        <div className="source-draft-history">
          {savedDrafts.map((item, index) => (
            <small key={`${item.ownerId}-${item.topic}-${index}`}>
              {item.ownerType}:{item.ownerId || 'unassigned'} - {item.book} - {item.topic || 'untitled'}
            </small>
          ))}
        </div>
      )}
    </Panel>
  )
}

function buildYaml(draft: SourceReferenceDraft) {
  return `# Add under ${draft.ownerType === 'lesson' ? 'lesson.sourceReferences' : 'milestone.sourceReferences'} for ${draft.ownerId || '<target-id>'}
- book: ${draft.book || '<book-id>'}
  chapter: ${draft.chapter || '<chapter-or-section>'}
  topic: ${draft.topic || '<specific-study-topic>'}
  usage: ${draft.usage}`
}

function readDrafts() {
  try {
    const raw = localStorage.getItem(storageKey)
    return raw ? (JSON.parse(raw) as SourceReferenceDraft[]) : []
  } catch {
    return []
  }
}
