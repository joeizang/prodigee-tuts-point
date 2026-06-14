import { Search } from 'lucide-react'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import type { SearchResult } from '../api'
import { AsyncState } from '../components/AsyncState'
import { Page } from '../components/Page'
import { useApi } from '../hooks/useApi'

export function SearchPage() {
  const [query, setQuery] = useState('')
  const searchUrl = query.trim()
    ? `/api/curriculum/search?q=${encodeURIComponent(query.trim())}`
    : null
  const { data: results, error, isLoading } = useApi<SearchResult[]>(searchUrl)

  return (
    <Page title="Search">
      <div className="search-field">
        <Search size={18} />
        <input
          aria-label="Search curriculum"
          placeholder="Search lessons, exercises, concepts, projects, or source anchors"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
        />
      </div>
      {searchUrl && <AsyncState error={error} isLoading={isLoading} />}
      <div className="item-list section-gap">
        {(results ?? []).map((result) => (
          <Link className="list-row" key={`${result.kind}-${result.id}`} to={result.path}>
            <Search size={18} />
            <span>
              <strong>
                {result.kind}: {result.title}
              </strong>
              <small>
                {result.summary}
                {result.metadata ? ` - ${result.metadata}` : ''}
              </small>
            </span>
          </Link>
        ))}
      </div>
    </Page>
  )
}
