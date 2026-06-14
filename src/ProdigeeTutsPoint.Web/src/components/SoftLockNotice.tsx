import { AlertTriangle } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { SoftLock } from '../api'

export function SoftLockNotice({
  locks,
  title,
}: {
  locks: SoftLock[]
  title: string
}) {
  if (locks.length === 0) {
    return null
  }

  return (
    <section className="soft-lock">
      <div className="soft-lock-icon">
        <AlertTriangle size={18} />
      </div>
      <div>
        <h3>{title}</h3>
        <p>You can continue, but these items will make the next step more useful.</p>
        <div className="soft-lock-links">
          {locks.map((lock) => (
            <Link key={`${lock.targetType}-${lock.targetId}`} to={softLockPath(lock)}>
              {lock.title}
            </Link>
          ))}
        </div>
      </div>
    </section>
  )
}

function softLockPath(lock: SoftLock) {
  if (lock.targetType === 'lesson') {
    return `/lessons/${lock.targetId}`
  }

  if (lock.targetType === 'exercise') {
    return `/exercises/${lock.targetId}`
  }

  if (lock.targetType === 'milestone') {
    return `/projects/wordfreq-csharp/milestones/${lock.targetId}`
  }

  return '/search'
}
