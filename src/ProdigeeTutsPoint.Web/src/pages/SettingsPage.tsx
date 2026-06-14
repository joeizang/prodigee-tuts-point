import { ThemeSwitcher } from '../components/AppShell'
import { Page } from '../components/Page'
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
  return (
    <Page title="Settings">
      <p className="body-copy">Local learner profile: {profile.displayName}</p>
      <ThemeSwitcher theme={theme} onThemeChange={onThemeChange} />
    </Page>
  )
}
