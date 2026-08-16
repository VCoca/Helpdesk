import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { apiGet } from '../api/client'
import { useCurrentUser } from '../hooks/useCurrentUser'
import type { TicketListItem } from '../types/ticket'

function TicketsPage() {
    const isAgent = useCurrentUser()?.role === 'Agent'

    const { data, isPending, isError, error } = useQuery({
        queryKey: ['tickets'],
        queryFn: () => apiGet<TicketListItem[]>('/api/tickets'),
    })

    return (
        <div>
            <h1>Tickets</h1>

            {/* The list the server sends is already scoped: an Agent gets every
                ticket, anyone else only their own. This just labels it. */}
            <p>{isAgent ? 'Showing all tickets' : 'Showing your tickets'}</p>

            <Link to="/tickets/new">New ticket</Link>

            {isPending && <p>Loading tickets...</p>}

            {isError && <p>Error: {error.message}</p>}

            {data && (data.length === 0 ? (
                <p>No tickets yet.</p>
            ) : (
                data.map((ticket) => (
                    <div key={ticket.id}>
                        <p>-------------------</p>
                        <h2>
                            <Link to={`/tickets/${ticket.id}`}>{ticket.title}</Link>
                        </h2>
                        <p>{ticket.status}</p>
                        <p>{ticket.categoryName}</p>

                        {/* Agent-only affordance. The endpoints behind it are not
                            built yet, so it is disabled rather than wired up. */}
                        {isAgent && (
                            <p>
                                <button type="button" disabled>Assign to me</button>{' '}
                                <span>reported by {ticket.authorName}</span>
                            </p>
                        )}
                    </div>
                ))
            ))}
        </div>
    )
}

export default TicketsPage
