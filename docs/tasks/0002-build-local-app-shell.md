# 0002 Build the local app shell

## Status

Completed

## Type

AFK

## What to build

Build the first usable local app shell with explicit React routes, navigation, local learner profile, theme switcher, keyboard shortcuts, and basic command palette.

## Acceptance criteria

- [x] App has explicit routes for dashboard, tracks, projects, lessons, exercises, review, search, sources, and settings.
- [x] A default local learner profile is created automatically with no login.
- [x] Light, dark, and neutral themes are available.
- [x] Theme preference persists per local learner profile.
- [x] Basic command palette opens tracks, lessons, exercises, projects, sources, review, search, and settings.
- [x] Keyboard shortcuts work: `Cmd/Ctrl+K`, `Cmd/Ctrl+Enter`, `Cmd/Ctrl+S`, `Esc`.
- [x] Built React assets can be served by ASP.NET Core for the usable local build.

## Remediation notes

- `Cmd/Ctrl+S` dispatches an app-level save command handled by the exercise workspace.
- `Cmd/Ctrl+Enter` dispatches an app-level run command handled by the exercise workspace.
- Theme persistence is keyed by local profile id, with the old global key retained only as a migration fallback.
- Light mode now has an explicit `[data-theme='light']` token set instead of relying only on `:root` fallback variables.
- The command palette is backed by `/api/curriculum/navigation`, so tracks, projects, milestones, lessons, exercises, concepts, and static app destinations come from indexed content.

## Blocked by

- 0001 Scaffold the .NET 10 and React app
