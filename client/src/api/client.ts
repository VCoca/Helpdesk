import { getToken } from './auth'

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

// Every protected endpoint wants the same header. Omitted entirely when there
// is no token, so /api/auth/login is not sent a stale "Bearer null".
function authHeaders(): Record<string, string> {
  const token = getToken()
  return token ? { Authorization: `Bearer ${token}` } : {}
}

export async function apiGet<T>(path: string): Promise<T> {
  const res = await fetch(`${baseUrl()}${path}`, { headers: authHeaders() })
  if (!res.ok) throw await toError(res)
  return res.json() as Promise<T>
}

export async function apiPost<TBody, TResult>(path: string, body: TBody): Promise<TResult> {
  const res = await fetch(`${baseUrl()}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(body),
  })
  if (!res.ok) throw await toError(res)
  return res.json() as Promise<TResult>
}
