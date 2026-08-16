import type { AuthResponse, CurrentUser } from '../types/auth'

const TOKEN_KEY = 'helpdesk.token'
const USER_KEY = 'helpdesk.user'

const listeners = new Set<() => void>()

let cachedUser: CurrentUser | null | undefined

function readUser(): CurrentUser | null {
  const raw = localStorage.getItem(USER_KEY)

  if (!raw) return null

  try {
    return JSON.parse(raw) as CurrentUser
  } catch {
    return null
  }
}

function emitAuthChanged() {
  cachedUser = readUser()
  listeners.forEach((listener) => listener())
}

export function subscribeToAuth(listener: () => void): () => void {
  listeners.add(listener)
  return () => listeners.delete(listener)
}

window.addEventListener('storage', (e) => {
  if (e.key === USER_KEY || e.key === TOKEN_KEY) emitAuthChanged()
})

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function getCurrentUser(): CurrentUser | null {
  if (cachedUser === undefined) cachedUser = readUser()
  return cachedUser
}

/** Stores the token and the identity that came with it, after a successful login. */
export function saveAuth(auth: AuthResponse) {
  const { token, ...user } = auth

  localStorage.setItem(TOKEN_KEY, token)
  localStorage.setItem(USER_KEY, JSON.stringify(user))

  emitAuthChanged()
}

export function clearAuth() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USER_KEY)

  emitAuthChanged()
}

export function isSessionValid(user: CurrentUser | null): boolean {
  if (!user || !getToken()) return false

  return new Date(user.expiresAtUtc).getTime() > Date.now()
}
