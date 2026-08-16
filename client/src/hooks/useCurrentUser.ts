import { useSyncExternalStore } from 'react'

import { getCurrentUser, subscribeToAuth } from '../api/auth'
import type { CurrentUser } from '../types/auth'

/**
 * The logged-in user, re-rendering whenever the session changes — including when
 * a 401 clears it mid-page, which is the case a plain read during render misses.
 */
export function useCurrentUser(): CurrentUser | null {
  return useSyncExternalStore(subscribeToAuth, getCurrentUser)
}
