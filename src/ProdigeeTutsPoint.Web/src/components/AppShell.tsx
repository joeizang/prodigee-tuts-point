import {
  BookOpen,
  CheckCircle2,
  Code2,
  Command,
  FileSearch,
  Library,
  Moon,
  NotebookPen,
  Play,
  Search,
  Settings,
  Sun,
  TerminalSquare,
} from 'lucide-react'
import { useMemo } from 'react'
import { Link, NavLink, useLocation, useNavigate } from 'react-router-dom'
import type { NavigationItem } from '../api'
import type { LocalProfile, Theme } from '../types'

type CommandItem = {
  kind: string
  label: string
  path: string
  summary: string
  icon: React.ComponentType<{ size?: number }>
}

const navItems = [
  { label: 'Dashboard', path: '/', icon: TerminalSquare },
  { label: 'Tracks', path: '/tracks', icon: BookOpen },
  { label: 'Projects', path: '/projects/wordfreq-csharp', icon: Play },
  { label: 'Review', path: '/review', icon: NotebookPen },
  { label: 'Search', path: '/search', icon: Search },
  { label: 'Sources', path: '/sources', icon: Library },
]

export function Sidebar({
  onCommandOpen,
  onThemeChange,
  theme,
}: {
  onCommandOpen: () => void
  onThemeChange: (theme: Theme) => void
  theme: Theme
}) {
  return (
    <aside className="sidebar" aria-label="Primary navigation">
      <Link className="brand" to="/">
        <span className="brand-mark">P</span>
        <span>
          <strong>Prodigee</strong>
          <small>Tuts Point</small>
        </span>
      </Link>

      <nav className="nav-list">
        {navItems.map((item) => (
          <NavLink
            className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
            end={item.path === '/'}
            key={item.path}
            to={item.path}
          >
            <item.icon size={18} />
            <span>{item.label}</span>
          </NavLink>
        ))}
      </nav>

      <div className="sidebar-footer">
        <button className="command-button" type="button" onClick={onCommandOpen}>
          <Command size={18} />
          <span>Command</span>
        </button>
        <ThemeSwitcher theme={theme} onThemeChange={onThemeChange} />
      </div>
    </aside>
  )
}

export function Header({
  onCommandOpen,
  profile,
}: {
  onCommandOpen: () => void
  profile: LocalProfile
}) {
  const location = useLocation()
  const title =
    location.pathname === '/'
      ? 'Dashboard'
      : location.pathname
          .split('/')
          .filter(Boolean)
          .map(formatRouteSegment)
          .join(' / ')

  return (
    <header className="topbar">
      <div>
        <h1>{title}</h1>
      </div>
      <div className="topbar-actions">
        <span className="profile-chip">{profile.displayName}</span>
        <button className="icon-text-button" type="button" onClick={onCommandOpen}>
          <Command size={18} />
          <span>Open</span>
        </button>
      </div>
    </header>
  )
}

export function ThemeSwitcher({
  onThemeChange,
  theme,
}: {
  onThemeChange: (theme: Theme) => void
  theme: Theme
}) {
  return (
    <div className="theme-switcher" aria-label="Theme">
      <button
        className={theme === 'light' ? 'selected' : ''}
        title="Light"
        type="button"
        onClick={() => onThemeChange('light')}
      >
        <Sun size={16} />
      </button>
      <button
        className={theme === 'neutral' ? 'selected' : ''}
        title="Neutral"
        type="button"
        onClick={() => onThemeChange('neutral')}
      >
        <FileSearch size={16} />
      </button>
      <button
        className={theme === 'dark' ? 'selected' : ''}
        title="Dark"
        type="button"
        onClick={() => onThemeChange('dark')}
      >
        <Moon size={16} />
      </button>
    </div>
  )
}

export function CommandPalette({
  items,
  onClose,
  onQueryChange,
  query,
}: {
  items: NavigationItem[]
  onClose: () => void
  onQueryChange: (value: string) => void
  query: string
}) {
  const navigate = useNavigate()
  const commandItems = useMemo(
    () =>
      items.map((item) => ({
        ...item,
        icon: iconForCommandKind(item.kind),
      })),
    [items],
  )
  const filteredItems = useMemo(
    () =>
      commandItems.filter((item) =>
        `${item.label} ${item.kind} ${item.summary}`.toLowerCase().includes(query.trim().toLowerCase()),
      ),
    [commandItems, query],
  )

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <section
        aria-label="Command palette"
        className="command-palette"
        role="dialog"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <div className="command-search">
          <Search size={18} />
          <input
            autoFocus
            aria-label="Search commands"
            value={query}
            onChange={(event) => onQueryChange(event.target.value)}
          />
        </div>
        <div className="command-results">
          {filteredItems.map((item) => (
            <button
              key={item.path}
              type="button"
              onClick={() => {
                navigate(item.path)
                onClose()
                onQueryChange('')
              }}
            >
              <item.icon size={17} />
              <span>
                {item.label}
                <small>{item.kind}</small>
              </span>
            </button>
          ))}
        </div>
      </section>
    </div>
  )
}

function iconForCommandKind(kind: string): CommandItem['icon'] {
  switch (kind) {
    case 'Dashboard':
      return TerminalSquare
    case 'Track':
    case 'Tracks':
    case 'Lesson':
      return BookOpen
    case 'Project':
    case 'Milestone':
      return Play
    case 'Exercise':
      return Code2
    case 'Concept':
      return CheckCircle2
    case 'Review':
      return NotebookPen
    case 'Search':
      return Search
    case 'Sources':
      return Library
    case 'Settings':
      return Settings
    default:
      return FileSearch
  }
}

function formatRouteSegment(segment: string) {
  const knownNames: Record<string, string> = {
    csharp: 'C#',
    'pure-word-counting-core': 'Pure Word Counting Core',
    'wordfreq-csharp': 'wordfreq-csharp',
  }

  if (knownNames[segment]) {
    return knownNames[segment]
  }

  return segment
    .split('-')
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(' ')
}
