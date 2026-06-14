import type { ReactNode } from 'react'

export function Page({
  children,
  title,
  wide = false,
}: {
  children: ReactNode
  title: string
  wide?: boolean
}) {
  return (
    <section className={wide ? 'page-panel wide' : 'page-panel'}>
      <h2>{title}</h2>
      {children}
    </section>
  )
}

export function Panel({ children, title }: { children: ReactNode; title: string }) {
  return (
    <section className="panel">
      <h3>{title}</h3>
      {children}
    </section>
  )
}
