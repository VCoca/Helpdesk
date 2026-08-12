import { Route, Routes } from 'react-router-dom'

import HomePage from './pages/HomePage'
import TicketsPage from './pages/TicketsPage'
import TicketDetailsPage from './pages/TicketDetailsPage'
import NewTicketPage from './pages/NewTicketPage'

function App() {
  return(
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/tickets" element={<TicketsPage />} />
      <Route path="/tickets/new" element={<NewTicketPage />} />
      <Route path="/tickets/:ticketId" element={<TicketDetailsPage />} />
    </Routes>
  )
}

export default App
