import { useQuery } from '@tanstack/react-query'
import { apiGet } from '../api/apiGet'
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

            {data.map((ticket) => (
                <div key={ticket.id}>
                    <h2>{ticket.title}</h2>
                    <p>{ticket.status}</p>
                </div>
            ))}
        </div>
    )
}

export default TicketsPage