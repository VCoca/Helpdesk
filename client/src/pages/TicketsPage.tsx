import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { apiGet } from '../api/client'
import type { TicketListItem } from '../types/ticket'

function TicketsPage() {
    const { data, isPending, isError, error } = useQuery({
        queryKey: ['tickets'],
        queryFn: () => apiGet<TicketListItem[]>('/api/tickets'),
    })

    if (isPending) {
        return <p>Loading tickets...</p>
    }

    if (isError) {
        return <p>Error: {error.message}</p>
    }

    return (
        <div>
            <h1>Tickets</h1>

            <Link to="/tickets/new">New ticket</Link>

            {data.length === 0 ? (
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
                        
                    </div>
                ))
            )}
        </div>
    )
}

export default TicketsPage
