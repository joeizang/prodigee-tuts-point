export type Theme = 'light' | 'neutral' | 'dark'

export type LocalProfile = {
  id: string
  displayName: string
}

export type MarkdownBlock =
  | { kind: 'heading'; level: 1 | 2 | 3; text: string }
  | { kind: 'paragraph'; text: string }
  | { kind: 'list'; items: string[] }
  | { kind: 'code'; language: string; code: string }
