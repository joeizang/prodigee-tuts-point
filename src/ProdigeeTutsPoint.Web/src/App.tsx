import { useEffect, useState } from 'react'
import { Route, Routes } from 'react-router-dom'
import { CommandPalette, Header, Sidebar } from './components/AppShell'
import { Dashboard } from './pages/Dashboard'
import { ConceptDetail } from './pages/ConceptDetail'
import { ExerciseDetail } from './pages/ExerciseDetail'
import { LessonDetail } from './pages/LessonDetail'
import { MilestoneDetail } from './pages/MilestoneDetail'
import { ProjectDetail } from './pages/ProjectDetail'
import { Review } from './pages/Review'
import { SearchPage } from './pages/SearchPage'
import { SettingsPage } from './pages/SettingsPage'
import { Sources } from './pages/Sources'
import { TrackDetail } from './pages/TrackDetail'
import { Tracks } from './pages/Tracks'
import { useApi } from './hooks/useApi'
import type { NavigationItem } from './api'
import type { LocalProfile, Theme } from './types'
import './App.css'

const themeKey = 'prodigee.theme'
const profileKey = 'prodigee.localProfile'
const defaultProfile: LocalProfile = {
  id: 'default-profile',
  displayName: 'Default Profile',
}

function App() {
  const [profile] = useState<LocalProfile>(() => {
    const storedProfile = localStorage.getItem(profileKey)
    if (storedProfile) {
      try {
        return JSON.parse(storedProfile) as LocalProfile
      } catch {
        localStorage.removeItem(profileKey)
      }
    }

    localStorage.setItem(profileKey, JSON.stringify(defaultProfile))
    return defaultProfile
  })
  const [theme, setTheme] = useState<Theme>(() => {
    const storedTheme = localStorage.getItem(profileThemeKey(profile.id)) ?? localStorage.getItem(themeKey)
    return storedTheme === 'light' || storedTheme === 'neutral' || storedTheme === 'dark'
      ? storedTheme
      : 'neutral'
  })
  const [isCommandOpen, setIsCommandOpen] = useState(false)
  const [query, setQuery] = useState('')
  const { data: navigationItems } = useApi<NavigationItem[]>('/api/curriculum/navigation')

  useEffect(() => {
    document.documentElement.dataset.theme = theme
    localStorage.setItem(profileThemeKey(profile.id), theme)
  }, [profile.id, theme])

  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      const commandOrControl = event.metaKey || event.ctrlKey

      if (commandOrControl && event.key.toLowerCase() === 'k') {
        event.preventDefault()
        setIsCommandOpen(true)
      }

      if (commandOrControl && event.key.toLowerCase() === 's') {
        event.preventDefault()
        window.dispatchEvent(new CustomEvent('prodigee:save'))
      }

      if (commandOrControl && event.key === 'Enter') {
        event.preventDefault()
        window.dispatchEvent(new CustomEvent('prodigee:run'))
      }

      if (event.key === 'Escape') {
        setIsCommandOpen(false)
      }
    }

    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [])

  return (
    <div className="app-shell">
      <Sidebar
        theme={theme}
        onCommandOpen={() => setIsCommandOpen(true)}
        onThemeChange={setTheme}
      />

      <main className="content-shell">
        <Header profile={profile} onCommandOpen={() => setIsCommandOpen(true)} />
        <Routes>
          <Route path="/" element={<Dashboard profile={profile} />} />
          <Route path="/tracks" element={<Tracks />} />
          <Route path="/tracks/:trackId" element={<TrackDetail />} />
          <Route path="/projects/:projectId" element={<ProjectDetail profile={profile} />} />
          <Route
            path="/projects/:projectId/milestones/:milestoneId"
            element={<MilestoneDetail profile={profile} />}
          />
          <Route path="/lessons/:lessonId" element={<LessonDetail profile={profile} />} />
          <Route path="/concepts/:conceptId" element={<ConceptDetail profile={profile} />} />
          <Route
            path="/exercises/:exerciseId"
            element={<ExerciseDetail profile={profile} theme={theme} />}
          />
          <Route path="/review" element={<Review profile={profile} />} />
          <Route path="/search" element={<SearchPage />} />
          <Route path="/sources" element={<Sources profile={profile} />} />
          <Route
            path="/settings"
            element={<SettingsPage profile={profile} theme={theme} onThemeChange={setTheme} />}
          />
        </Routes>
      </main>

      {isCommandOpen && (
        <CommandPalette
          items={navigationItems ?? fallbackNavigationItems}
          query={query}
          onClose={() => setIsCommandOpen(false)}
          onQueryChange={setQuery}
        />
      )}
    </div>
  )
}

export default App

function profileThemeKey(profileId: string) {
  return `${themeKey}.${profileId}`
}

const fallbackNavigationItems: NavigationItem[] = [
  { kind: 'Dashboard', label: 'Dashboard', path: '/', summary: 'Study overview' },
  { kind: 'Tracks', label: 'Tracks', path: '/tracks', summary: 'All curriculum tracks' },
  { kind: 'Review', label: 'Review', path: '/review', summary: 'Review queue' },
  { kind: 'Search', label: 'Search', path: '/search', summary: 'Search curriculum' },
  { kind: 'Sources', label: 'Sources', path: '/sources', summary: 'Source library' },
  { kind: 'Settings', label: 'Settings', path: '/settings', summary: 'Local settings' },
]
