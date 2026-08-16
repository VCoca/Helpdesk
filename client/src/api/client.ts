import { clearAuth, getToken } from './auth'

const BASE = import.meta.env.VITE_API_BASE_URL as string | undefined

function baseUrl(): string {
  if (!BASE) {
    throw new Error(
      'VITE_API_BASE_URL is not set. Copy client/.env.example to client/.env.local and restart the dev server.',
    )
  }
  return BASE
}

interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

async function toError(res: Response): Promise<Error> {
  let problem: ProblemDetails | null = null

  try {
    problem = (await res.json()) as ProblemDetails
  } catch {
    // Not JSON — an unhandled 500 returns a plain stack trace. Fall back below.
  }

  const fieldErrors = problem?.errors ? Object.values(problem.errors).flat() : []

  return new Error(
    fieldErrors.join(' ') ||
      problem?.detail ||
      problem?.title ||
      `${res.status} ${res.statusText}`,
  )
}

function authHeaders(path: string): Record<string, string> {
  if (path.startsWith('/api/auth/')) return {}

  const token = getToken()
  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function request<T>(path: string, init: RequestInit, extraHeaders: Record<string, string> = {}): Promise<T> {
  const auth = authHeaders(path)
  const sentToken = 'Authorization' in auth

  const res = await fetch(`${baseUrl()}${path}`, {
    ...init,
    headers: { ...extraHeaders, ...auth },
  })

  if (!res.ok) {
    if (res.status === 401 && sentToken) clearAuth()

    throw await toError(res)
  }

  return res.json() as Promise<T>
}

export function apiGet<T>(path: string): Promise<T> {
  return request<T>(path, { method: 'GET' })
}

export function apiPost<TBody, TResult>(path: string, body: TBody): Promise<TResult> {
  return request<TResult>(
    path,
    { method: 'POST', body: JSON.stringify(body) },
    { 'Content-Type': 'application/json' },
  )
}
