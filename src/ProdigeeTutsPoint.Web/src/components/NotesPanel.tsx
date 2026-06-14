import { Save } from 'lucide-react'
import { useRef, useState } from 'react'
import { putJson } from '../api'
import type { PersonalNote } from '../api'
import { useApi } from '../hooks/useApi'
import type { LocalProfile } from '../types'

export function NotesPanel({
  compact = false,
  profile,
  targetId,
  targetType,
}: {
  compact?: boolean
  profile: LocalProfile
  targetId: string
  targetType: string
}) {
  const noteUrl = `/api/learner/notes?profileId=${encodeURIComponent(profile.id)}&targetType=${encodeURIComponent(targetType)}&targetId=${encodeURIComponent(targetId)}`
  const { data: note } = useApi<PersonalNote | null>(noteUrl)
  const noteRef = useRef<HTMLTextAreaElement | null>(null)
  const [savedAt, setSavedAt] = useState<string | null>(null)
  const [isSaving, setIsSaving] = useState(false)

  const saveNote = async () => {
    setIsSaving(true)
    try {
      const saved = await putJson<PersonalNote>('/api/learner/notes', {
        profileId: profile.id,
        targetType,
        targetId,
        body: noteRef.current?.value ?? '',
      })
      setSavedAt(new Date(saved.updatedAt).toLocaleTimeString())
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <section className={compact ? 'notes-panel compact' : 'notes-panel'}>
      <div className="notes-header">
        <h3>Personal Notes</h3>
        {savedAt && <span>Saved {savedAt}</span>}
      </div>
      <textarea
        key={note?.id ?? `${targetType}-${targetId}`}
        ref={noteRef}
        aria-label="Personal notes"
        defaultValue={note?.body ?? ''}
        placeholder="Capture your own explanation, doubts, examples, or source cross-checks."
      />
      <button className="secondary-action" disabled={isSaving} type="button" onClick={saveNote}>
        <Save size={16} />
        <span>{isSaving ? 'Saving...' : 'Save note'}</span>
      </button>
    </section>
  )
}
