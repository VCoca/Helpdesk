const BASE = import.meta.env.VITE_API_BASE_URL as string | undefined

// Checked per request rather than at module load. Throwing at module scope kills
// the whole import graph, so a missing variable would white-screen every page —
// including the ones that never call the API. This way it surfaces as a normal
// query error on the screens that actually fetch.
function baseUrl(): string {
  if (!BASE) {
    throw new Error(
      'VITE_API_BASE_URL is not set. Copy client/.env.example to client/.env.local and restart the dev server.',
    )
  }
  return BASE
}

// ASP.NET Core answers 400/404 with a ProblemDetails body. Reading the message
// out of it is the difference between showing the user "Category does not
// exist." and showing them "400 Bad Request".
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

export async function apiGet<T>(path: string): Promise<T> {
  const res = await fetch(`${baseUrl()}${path}`)
  if (!res.ok) throw await toError(res)
  return res.json() as Promise<T>
}

export async function apiPost<TBody, TResult>(path: string, body: TBody): Promise<TResult> {
  const res = await fetch(`${baseUrl()}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!res.ok) throw await toError(res)
  return res.json() as Promise<TResult>
}
