import { BookOpen, CheckCircle2, Library } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { SourceReference } from '../api'
import { Panel } from './Page'

export function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div className="metric">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  )
}

export function List({ items }: { items: string[] }) {
  return (
    <ul className="plain-list">
      {items.map((item) => (
        <li key={item}>
          <CheckCircle2 size={17} />
          <span>{item}</span>
        </li>
      ))}
    </ul>
  )
}

export function LinkedList({
  items,
  pathPrefix,
}: {
  items: Array<{ id: string; title: string; summary: string }>
  pathPrefix: string
}) {
  return (
    <div className="item-list">
      {items.map((item) => (
        <Link className="list-row" key={item.id} to={`${pathPrefix}/${item.id}`}>
          <BookOpen size={18} />
          <span>
            <strong>{item.title}</strong>
            <small>{item.summary}</small>
          </span>
        </Link>
      ))}
    </div>
  )
}

export function SourceList({
  sources,
  title = 'Further reading',
}: {
  sources: SourceReference[]
  title?: string
}) {
  if (sources.length === 0) {
    return null
  }

  return (
    <Panel title={title}>
      <ul className="plain-list">
        {sources.map((source) => (
          <li key={source.id}>
            <Library size={17} />
            <span>
              {source.bookTitle}
              {source.chapter ? ` - ${source.chapter}` : ''}: {source.topic}
            </span>
          </li>
        ))}
      </ul>
    </Panel>
  )
}
