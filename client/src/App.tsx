import { Navigate, Outlet, Route, Routes, useLocation } from 'react-router-dom'

import Header from './components/Header'
import HomePage from './pages/HomePage'
import TicketsPage from './pages/TicketsPage'
import TicketDetailsPage from './pages/TicketDetailsPage'
import NewTicketPage from './pages/NewTicketPage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import { isSessionValid } from './api/auth'
import { useCurrentUser } from './hooks/useCurrentUser'

function Layout() {
  return (
    <>
      <Header />
      <main>
        <Outlet />
      </main>
    </>
  )
}

function RequireAuth() {
  const location = useLocation()

  return isSessionValid(useCurrentUser())
    ? <Outlet />
    : <Navigate to="/login" replace state={{ from: location }} />
}

function RedirectIfLoggedIn() {
  return isSessionValid(useCurrentUser())
    ? <Navigate to="/tickets" replace />
    : <Outlet />
}

function App() {
  return(
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<HomePage />} />

        <Route element={<RedirectIfLoggedIn />}>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
        </Route>

        <Route element={<RequireAuth />}>
          <Route path="/tickets" element={<TicketsPage />} />
          <Route path="/tickets/new" element={<NewTicketPage />} />
          <Route path="/tickets/:ticketId" element={<TicketDetailsPage />} />
        </Route>
      </Route>
    </Routes>
  )
}

export default App
