import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'

import { apiPost } from '../api/client'
import { setToken } from '../api/auth'

interface AuthResponse {
  token: string
  expiresAtUtc: string
  userId: number
  email: string
  fullName: string
  role: 'User' | 'Agent'
}

// Deliberately bare: plain useState rather than React Hook Form + Zod, and no
// redirect-back-to-where-you-were. Enough to obtain a token; the real login
// screen is a separate task.
function LoginPage() {
  const [email, setEmail] = useState('jelena.popovic@helpdesk.local')
  const [password, setPassword] = useState('Password123!')

  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const login = useMutation({
    mutationFn: () => apiPost<{ email: string; password: string }, AuthResponse>(
      '/api/auth/login',
      { email, password },
    ),
    onSuccess: (auth) => {
      setToken(auth.token)

      // Drop anything cached as the previous identity, including the 401s from
      // before logging in.
      queryClient.clear()

      navigate('/tickets')
    },
  })

  return (
    <div className="mx-auto max-w-sm p-6">
      <h1 className="mb-6 text-2xl font-semibold">Log in</h1>

      <form
        onSubmit={(e) => {
          e.preventDefault()
          login.mutate()
        }}
        className="flex flex-col gap-4"
      >
        <div className="flex flex-col gap-1">
          <label htmlFor="email" className="font-medium">Email</label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="rounded border border-gray-400 px-3 py-2"
          />
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="password" className="font-medium">Password</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="rounded border border-gray-400 px-3 py-2"
          />
        </div>

        {login.isError && <p className="text-sm text-red-600">{login.error.message}</p>}

        <button
          type="submit"
          disabled={login.isPending}
          className="rounded bg-blue-600 px-4 py-2 text-white disabled:opacity-50"
        >
          {login.isPending ? 'Logging in...' : 'Log in'}
        </button>
      </form>

      <p className="mt-6 text-sm text-gray-500">
        Seeded accounts, all with password <code>Password123!</code>:
        <br />
        jelena.popovic@helpdesk.local (User) — sees 7 tickets
        <br />
        ana.kovac@helpdesk.local (Agent) — sees all 25
      </p>
    </div>
  )
}

export default LoginPage
