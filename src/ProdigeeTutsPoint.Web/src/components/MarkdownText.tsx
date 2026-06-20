import { Marked, type RendererObject, type Tokens } from 'marked'
import { useMemo } from 'react'

const markdownRenderer: RendererObject<string, string> = {
  code({ text, lang }: Tokens.Code) {
    const language = resolveCodeLanguage(text, lang?.trim() ?? '')
    const languageClass = language ? ` language-${language}` : ''
    return `<pre class="code-block${languageClass}"><code class="${languageClass.trim()}">${highlightCode(text, language)}</code></pre>`
  },
  codespan({ text }: Tokens.Codespan) {
    return `<code class="inline-code">${escapeHtml(text)}</code>`
  },
  paragraph({ tokens }: Tokens.Paragraph) {
    return `<p>${enrichTermDefinitions(this.parser.parseInline(tokens))}</p>`
  },
  strong({ tokens }: Tokens.Strong) {
    const text = plainText(tokens).trim()
    if (!text.startsWith('Term:')) {
      return `<strong>${this.parser.parseInline(tokens)}</strong>`
    }

    const term = text.slice('Term:'.length).trim()
    if (!term) {
      return `<strong>${this.parser.parseInline(tokens)}</strong>`
    }

    return `<span class="term-keyword" data-term="${escapeHtml(term)}" tabindex="0">${escapeHtml(term)}</span>`
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
  if (language === 'typescript') {
    return highlightTypeScriptCode(code)
  }

  if (language === 'swift') {
    return highlightSwiftCode(code)
  }

  if (language === 'csharp') {
    return highlightCSharpCode(code)
  }

  return escapeHtml(code)
}

function highlightCSharpCode(code: string) {
  const tokenPattern =
    /(\/\/.*|\/\*[\s\S]*?\*\/|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\b(?:public|private|protected|internal|static|sealed|class|record|struct|interface|namespace|using|return|throw|new|var|const|readonly|required|if|else|for|foreach|while|switch|case|default|break|continue|true|false|null|async|await|void|string|int|bool|char|double|decimal|object)\b|\b[A-Z][A-Za-z0-9_]*\b|\b\d+(?:\.\d+)?\b)/g
  return highlightWithPattern(code, tokenPattern)
}

function highlightTypeScriptCode(code: string) {
  const tokenPattern =
    /(\/\/.*|\/\*[\s\S]*?\*\/|`(?:\\.|[^`\\])*`|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\b(?:export|import|from|type|interface|class|extends|implements|function|const|let|var|return|throw|new|if|else|for|of|while|switch|case|default|break|continue|true|false|null|undefined|async|await|string|number|boolean|unknown|never|void|readonly|as|satisfies)\b|\b[A-Z][A-Za-z0-9_]*\b|\b\d+(?:\.\d+)?\b)/g
  return highlightWithPattern(code, tokenPattern)
}

function highlightSwiftCode(code: string) {
  const tokenPattern =
    /(\/\/.*|\/\*[\s\S]*?\*\/|"(?:\\.|[^"\\])*"|@[A-Za-z_][A-Za-z0-9_]*|\b(?:actor|any|as|associatedtype|async|await|break|case|catch|class|continue|default|defer|do|else|enum|extension|false|fileprivate|for|func|guard|if|import|in|init|inout|internal|is|isolated|let|mutating|nil|nonisolated|open|package|private|protocol|public|repeat|required|rethrows|return|self|some|static|struct|subscript|super|switch|throw|throws|true|try|typealias|var|where|while)\b|\b(?:String|Int|Bool|Double|Float|Void|Error|Result|Array|Dictionary|Set|Package|Target|PackageDescription|Equatable|Hashable|Sendable)\b|\b[A-Z][A-Za-z0-9_]*\b|\b\d+(?:\.\d+)?\b)/g
  return highlightWithPattern(code, tokenPattern)
}

function highlightWithPattern(code: string, tokenPattern: RegExp) {
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
  if (token.startsWith('//') || token.startsWith('/*')) {
    return 'comment'
  }

  if (token.startsWith('"') || token.startsWith("'") || token.startsWith('`')) {
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

function resolveCodeLanguage(code: string, language: string) {
  const normalized = normalizeCodeLanguage(language)
  return normalized || inferSupportedCodeLanguage(code)
}

function normalizeCodeLanguage(language: string) {
  switch (language.trim().toLowerCase()) {
    case 'c#':
    case 'cs':
    case 'csharp':
      return 'csharp'
    case 'ts':
    case 'tsx':
    case 'typescript':
      return 'typescript'
    case 'swift':
      return 'swift'
    default:
      return ''
  }
}

function inferSupportedCodeLanguage(code: string) {
  if (
    /swift-tools-version|import\s+PackageDescription|\blet\s+package\s*=\s*Package\s*\(/.test(
      code,
    )
    || /\b(?:func|struct|enum|protocol|extension)\s+[A-Za-z_][A-Za-z0-9_]*(?:\s*[:<{(])?/.test(
      code,
    )
    || /\b(?:let|var)\s+[A-Za-z_][A-Za-z0-9_]*\s*:\s*(?:String|Int|Bool|Double|[A-Z][A-Za-z0-9_]*)/.test(
      code,
    )
  ) {
    return 'swift'
  }

  if (
    /\b(?:export\s+)?(?:type|interface)\s+[A-Za-z_][A-Za-z0-9_]*/.test(code)
    || /\bimport\s+.*\s+from\s+['"]/.test(code)
    || /\b(?:const|let)\s+[A-Za-z_][A-Za-z0-9_]*\s*:\s*[A-Za-z_][A-Za-z0-9_<>{}[\]| ]*\s*=/.test(
      code,
    )
  ) {
    return 'typescript'
  }

  if (
    /\bnamespace\s+[A-Za-z_][A-Za-z0-9_.]*/.test(code)
    || /\busing\s+[A-Za-z_][A-Za-z0-9_.]*\s*;/.test(code)
    || /\b(?:public|private|internal|protected)\s+(?:static\s+)?(?:class|record|struct|interface)\s+/.test(
      code,
    )
    || /\b(?:public|private|internal|protected)\s+(?:static\s+)?(?:string|int|bool|void|[A-Z][A-Za-z0-9_<>, ]*)\s+[A-Za-z_][A-Za-z0-9_]*\s*\(/.test(
      code,
    )
  ) {
    return 'csharp'
  }

  return ''
}

function enrichTermDefinitions(html: string) {
  let termIndex = 0

  return html.replace(
    /<span class="term-keyword" data-term="([^"]+)" tabindex="0">([^<]+)<\/span>\s+means\s+([\s\S]*?)(?=(?:[.!?](?:\s|$))|(?:\s*<span class="term-keyword")|$)([.!?]?)/g,
    (_match, dataTerm: string, label: string, definition: string, terminator: string) => {
      const cleanDefinition = definition.trim()
      if (!cleanDefinition) {
        return `<span class="term-keyword" data-term="${dataTerm}" tabindex="0">${label}</span>`
      }

      const term = escapeHtml(unescapeHtml(dataTerm))
      const tooltipId = `term-tooltip-${termIndex++}`
      return `<span class="term-bubble"><span class="term-keyword" data-term="${term}" tabindex="0" role="button" aria-label="Key term: ${term}" aria-describedby="${tooltipId}">${label}</span><span class="term-popover" id="${tooltipId}" role="tooltip"><strong>${term}</strong> <span>${cleanDefinition}</span></span></span> means ${cleanDefinition}${terminator}`
    },
  )
}

function plainText(tokens: Tokens.Generic[]): string {
  return tokens
    .map((token) => {
      if ('text' in token && typeof token.text === 'string') {
        return token.text
      }

      if ('tokens' in token && Array.isArray(token.tokens)) {
        return plainText(token.tokens as Tokens.Generic[])
      }

      return ''
    })
    .join('')
}

function unescapeHtml(value: string) {
  return value
    .replaceAll('&quot;', '"')
    .replaceAll('&#39;', "'")
    .replaceAll('&gt;', '>')
    .replaceAll('&lt;', '<')
    .replaceAll('&amp;', '&')
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
