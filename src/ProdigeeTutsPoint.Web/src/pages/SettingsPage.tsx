import { useState } from 'react'
import { getJson } from '../api'
import type { ImportResult, PortableStateExport, SetupDiagnostics } from '../api'
import { ThemeSwitcher } from '../components/AppShell'
import { Page, Panel } from '../components/Page'
import { useApi } from '../hooks/useApi'
import type { LocalProfile, Theme } from '../types'

export function SettingsPage({
  onThemeChange,
  profile,
  theme,
}: {
  onThemeChange: (theme: Theme) => void
  profile: LocalProfile
  theme: Theme
}) {
  const { data: setup } = useApi<SetupDiagnostics>('/api/setup/diagnostics')
  const [exportStatus, setExportStatus] = useState('')
  const [importText, setImportText] = useState('')
  const [importResult, setImportResult] = useState<ImportResult | null>(null)
  const [importError, setImportError] = useState('')

  const exportProfile = async () => {
    const portableState = await getJson<PortableStateExport>(
      `/api/portable-state/export?profileId=${encodeURIComponent(profile.id)}`,
    )
    const json = JSON.stringify(portableState, null, 2)
    const blob = new Blob([json], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = `prodigee-${profile.id}-${new Date().toISOString().slice(0, 10)}.json`
    anchor.click()
    URL.revokeObjectURL(url)
    setExportStatus(`Exported ${portableState.profileId} without provider secret values.`)
  }

  const importProfile = async () => {
    setImportError('')
    setImportResult(null)
    try {
      const parsed = JSON.parse(importText) as unknown
      const response = await fetch('/api/portable-state/import', {
        body: JSON.stringify(parsed),
        headers: { 'Content-Type': 'application/json' },
        method: 'POST',
      })
      const result = (await response.json()) as ImportResult
      setImportResult(result)
      if (!response.ok) {
        setImportError(result.conflicts.map((conflict) => conflict.message).join(' '))
      }
    } catch (error) {
      setImportError(error instanceof Error ? error.message : 'Import failed.')
    }
  }

  return (
    <Page title="Settings">
      <div className="content-stack">
        <Panel title="Local Profile">
          <p className="body-copy">Local learner profile: {profile.displayName}</p>
          <ThemeSwitcher theme={theme} onThemeChange={onThemeChange} />
        </Panel>

        <Panel title="Export and Import">
          <div className="settings-actions">
            <button className="primary-action" type="button" onClick={() => void exportProfile()}>
              Export learner state
            </button>
          </div>
          {exportStatus && <p className="body-copy">{exportStatus}</p>}
          <label className="settings-field">
            <span>Import JSON</span>
            <textarea
              rows={8}
              value={importText}
              onChange={(event) => setImportText(event.target.value)}
            />
          </label>
          <button
            className="secondary-action"
            disabled={!importText.trim()}
            type="button"
            onClick={() => void importProfile()}
          >
            Import learner state
          </button>
          {importResult && (
            <p className="body-copy">
              Imported {importResult.importedRowCount} row(s). Conflicts:{' '}
              {importResult.conflicts.length}.
            </p>
          )}
          {importError && <p className="body-copy">{importError}</p>}
        </Panel>

        <Panel title="Setup Diagnostics">
          <div className="setup-check-list">
            {(setup?.checks ?? []).map((check) => (
              <article className="setup-check" key={check.id}>
                <strong>{check.id}</strong>
                <span>{check.status}</span>
                <p>{check.message}</p>
              </article>
            ))}
          </div>
        </Panel>
      </div>
    </Page>
  )
}
