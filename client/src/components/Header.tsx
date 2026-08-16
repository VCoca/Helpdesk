import { useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'

import { clearAuth } from '../api/auth'
import { useCurrentUser } from '../hooks/useCurrentUser'

function Header() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  // Subscribed, not read once: if a 401 clears the session while this page is
  // open, the header stops showing a name that is no longer signed in.
  const currentUser = useCurrentUser()

  function logOut() {
    clearAuth()

    // The cache still holds the previous user's tickets. Without this they would
    // flash on screen for whoever logs in next.
    queryClient.clear()

    navigate('/login', { replace: true })
  }

  return (
    <header className="border-b border-gray-300">
      <div className="mx-auto flex max-w-4xl items-center justify-between gap-4 p-4">
        <nav className="flex items-center gap-4">
          <Link to="/" className="font-semibold">Helpdesk</Link>
          {currentUser && <Link to="/tickets" className="underline">Tickets</Link>}
        </nav>

        {currentUser ? (
          <div className="flex items-center gap-3 text-sm">
            <span>
              {currentUser.fullName}{' '}
              <span className="rounded bg-gray-200 px-2 py-0.5 text-gray-700">
                {currentUser.role}
              </span>
            </span>
            <button type="button" onClick={logOut} className="underline">
              Log out
            </button>
          </div>
        ) : (
          <div className="flex items-center gap-3 text-sm">
            <Link to="/login" className="underline">Log in</Link>
            <Link to="/register" className="underline">Register</Link>
          </div>
        )}
      </div>
    </header>
  )
}

export default Header
