import { useQuery } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'

import { apiGet } from '../api/client'
import type { TicketDetail } from '../types/ticket'

function formatDate(iso: string) {
  return new Date(iso).toLocaleString()
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <dt className="text-sm text-gray-500">{label}</dt>
      <dd>{children}</dd>
    </div>
  )
}

function TicketDetailsPage() {
  const { ticketId } = useParams<{ ticketId: string }>()

  // The route pattern does not constrain the segment, so /tickets/abc reaches
  // this component. Parse before asking the server for a ticket that cannot exist.
  const id = Number(ticketId)
  const isValidId = Number.isInteger(id) && id > 0

  const { data, isPending, isError, error } = useQuery({
    // The id is part of the key, so navigating between tickets refetches
    // instead of showing the previous ticket's cached data.
    queryKey: ['ticket', id],
    queryFn: () => apiGet<TicketDetail>(`/api/tickets/${id}`),
    enabled: isValidId,
  })

  if (!isValidId) {
    return (
      <div className="mx-auto max-w-2xl p-6">
        <p>“{ticketId}” is not a valid ticket id.</p>
        <Link to="/tickets" className="underline">
          Back to tickets
        </Link>
      </div>
    )
  }

  if (isPending) {
    return <p className="p-6">Loading ticket...</p>
  }

  if (isError) {
    return (
      <div className="mx-auto max-w-2xl p-6">
        {/* A ticket belonging to someone else comes back as 404, the same as one
            that does not exist — the server will not confirm it is real. */}
        <p className="text-red-600">Could not load ticket #{id}: {error.message}</p>
        <Link to="/tickets" className="underline">
          Back to tickets
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-2xl p-6">
      <Link to="/tickets" className="text-sm underline">
        Back to tickets
      </Link>

      <h1 className="mt-4 text-2xl font-semibold">{data.title}</h1>
      <p className="text-sm text-gray-500">Ticket #{data.id}</p>

      <dl className="mt-6 grid grid-cols-2 gap-4">
        <Field label="Status">{data.status}</Field>
        <Field label="Priority">{data.priority}</Field>
        <Field label="Category">{data.categoryName}</Field>
        <Field label="Reported by">{data.authorName}</Field>
        <Field label="Assigned agent">{data.assignedAgentName ?? 'Unassigned'}</Field>
        <Field label="Created">{formatDate(data.createdAt)}</Field>
        <Field label="Last updated">{formatDate(data.updatedAt)}</Field>
        {data.closedAt && <Field label="Closed">{formatDate(data.closedAt)}</Field>}
      </dl>

      <h2 className="mt-8 mb-2 font-medium">Description</h2>
      {/* whitespace-pre-wrap so line breaks the reporter typed survive. */}
      <p className="whitespace-pre-wrap">{data.description}</p>
    </div>
  )
}

export default TicketDetailsPage
