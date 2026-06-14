import { Marked, type RendererObject, type Tokens } from 'marked'
import { useMemo } from 'react'

const markdownRenderer: RendererObject<string, string> = {
  code({ text, lang }: Tokens.Code) {
    const language = lang?.trim() ?? ''
    return `<pre class="code-block"><code>${highlightCode(text, language)}</code></pre>`
  },
  codespan({ text }: Tokens.Codespan) {
    return `<code class="inline-code">${escapeHtml(text)}</code>`
  },
  html() {
    return ''
  },
  link({ href, title, tokens }: Tokens.Link) {
    const safeHref = sanitizeHref(href)
    if (!safeHref) {
      return this.parser.parseInline(tokens)
    }

    const safeTitle = title ? ` title="${escapeHtml(title)}"` : ''
    return `<a href="${escapeHtml(safeHref)}"${safeTitle}>${this.parser.parseInline(tokens)}</a>`
  },
}

const marked = new Marked({
  async: false,
  breaks: false,
  gfm: true,
  renderer: markdownRenderer,
})

export function MarkdownText({
  markdown,
  omitFirstHeading = false,
}: {
  markdown: string
  omitFirstHeading?: boolean
}) {
  const html = useMemo(() => {
    const source = omitFirstHeading ? removeFirstHeading(markdown) : markdown
    return marked.parse(source) as string
  }, [markdown, omitFirstHeading])

  return <div className="markdown-body" dangerouslySetInnerHTML={{ __html: html }} />
}

function removeFirstHeading(markdown: string) {
  const lines = markdown.replace(/\r\n/g, '\n').split('\n')
  const firstMeaningfulLine = lines.findIndex((line) => line.trim().length > 0)
  if (firstMeaningfulLine === -1 || !lines[firstMeaningfulLine].startsWith('# ')) {
    return markdown
  }

  lines.splice(firstMeaningfulLine, 1)
  return lines.join('\n')
}

function highlightCode(code: string, language: string) {
  if (language.toLowerCase() !== 'csharp' && language.toLowerCase() !== 'cs') {
    return escapeHtml(code)
  }

  const tokenPattern =
    /(\/\/.*|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\b(?:public|private|protected|internal|static|sealed|class|record|struct|interface|namespace|using|return|throw|new|var|const|readonly|required|if|else|for|foreach|while|switch|case|default|break|continue|true|false|null|async|await|void|string|int|bool|char|double|decimal|object)\b|\b[A-Z][A-Za-z0-9_]*\b|\b\d+(?:\.\d+)?\b)/g
  let html = ''
  let cursor = 0

  for (const match of code.matchAll(tokenPattern)) {
    const index = match.index ?? 0
    const value = match[0]

    if (index > cursor) {
      html += escapeHtml(code.slice(cursor, index))
    }

    html += `<span class="token ${codeTokenClass(value)}">${escapeHtml(value)}</span>`
    cursor = index + value.length
  }

  if (cursor < code.length) {
    html += escapeHtml(code.slice(cursor))
  }

  return html
}

function codeTokenClass(token: string) {
  if (token.startsWith('//')) {
    return 'comment'
  }

  if (token.startsWith('"') || token.startsWith("'")) {
    return 'string'
  }

  if (/^\d/.test(token)) {
    return 'number'
  }

  if (/^[A-Z]/.test(token)) {
    return 'type'
  }

  return 'keyword'
}

function sanitizeHref(href: string) {
  const trimmed = href.trim()
  if (
    trimmed.startsWith('/')
    || trimmed.startsWith('#')
    || trimmed.startsWith('https://')
    || trimmed.startsWith('http://')
  ) {
    return trimmed
  }

  return null
}

function escapeHtml(value: string) {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;')
}
