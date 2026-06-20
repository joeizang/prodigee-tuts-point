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

export function Panel({
  children,
  className = '',
  title,
}: {
  children: ReactNode
  className?: string
  title: string
}) {
  return (
    <section className={className ? `panel ${className}` : 'panel'}>
      <h3>{title}</h3>
      {children}
    </section>
  )
}
