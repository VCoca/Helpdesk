import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'

import { apiGet, apiPost } from '../api/client'
import {
  createTicketSchema,
  type CreateTicketFormValues,
  type CreateTicketInput,
} from '../schemas/TicketSchema'
import type { Category, Priority, TicketDetail } from '../types/ticket'

const priorities: Priority[] = ['Low', 'Medium', 'High', 'Critical']

function NewTicketPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const categories = useQuery({
    queryKey: ['categories'],
    queryFn: () => apiGet<Category[]>('/api/categories'),
  })

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateTicketFormValues, unknown, CreateTicketInput>({
    resolver: zodResolver(createTicketSchema),
    defaultValues: { title: '', description: '', priority: 'Medium' },
  })

  const createTicket = useMutation({
    mutationFn: (values: CreateTicketInput) =>
      apiPost<CreateTicketInput, TicketDetail>('/api/tickets', values),
    onSuccess: (ticket) => {
      // The cached list no longer matches the server. Invalidating refetches it
      // rather than trying to splice the new ticket in by hand.
      queryClient.invalidateQueries({ queryKey: ['tickets'] })
      navigate(`/tickets/${ticket.id}`)
    },
  })

  return (
    <div className="mx-auto max-w-xl p-6">
      <h1 className="mb-6 text-2xl font-semibold">New ticket</h1>

      <form
        onSubmit={handleSubmit((values) => createTicket.mutate(values))}
        noValidate
        className="flex flex-col gap-4"
      >
        <div className="flex flex-col gap-1">
          <label htmlFor="title" className="font-medium">
            Title
          </label>
          <input
            id="title"
            type="text"
            {...register('title')}
            aria-invalid={errors.title ? 'true' : 'false'}
            className="rounded border border-gray-400 px-3 py-2"
          />
          {errors.title && <p className="text-sm text-red-600">{errors.title.message}</p>}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="description" className="font-medium">
            Description
          </label>
          <textarea
            id="description"
            rows={6}
            {...register('description')}
            aria-invalid={errors.description ? 'true' : 'false'}
            className="rounded border border-gray-400 px-3 py-2"
          />
          {errors.description && (
            <p className="text-sm text-red-600">{errors.description.message}</p>
          )}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="priority" className="font-medium">
            Priority
          </label>
          <select
            id="priority"
            {...register('priority')}
            className="rounded border border-gray-400 px-3 py-2"
          >
            {priorities.map((priority) => (
              <option key={priority} value={priority}>
                {priority}
              </option>
            ))}
          </select>
          {errors.priority && <p className="text-sm text-red-600">{errors.priority.message}</p>}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="categoryId" className="font-medium">
            Category
          </label>

          {/* The dropdown fetches its own data, so it needs the same four states
              as any other screen that loads from the server. */}
          {categories.isPending && <p>Loading categories...</p>}

          {categories.isError && (
            <p className="text-sm text-red-600">
              Could not load categories: {categories.error.message}{' '}
              <button type="button" onClick={() => categories.refetch()} className="underline">
                Retry
              </button>
            </p>
          )}

          {categories.isSuccess && categories.data.length === 0 && (
            <p className="text-sm">No categories exist yet, so a ticket cannot be filed.</p>
          )}

          {categories.isSuccess && categories.data.length > 0 && (
            <select
              id="categoryId"
              defaultValue=""
              {...register('categoryId')}
              aria-invalid={errors.categoryId ? 'true' : 'false'}
              className="rounded border border-gray-400 px-3 py-2"
            >
              <option value="" disabled>
                Choose a category
              </option>
              {categories.data.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          )}

          {errors.categoryId && <p className="text-sm text-red-600">{errors.categoryId.message}</p>}
        </div>

        {/* Validation the browser never saw: the server rejected it. */}
        {createTicket.isError && (
          <p className="text-sm text-red-600">Could not create ticket: {createTicket.error.message}</p>
        )}

        <div className="flex items-center gap-3">
          <button
            type="submit"
            disabled={createTicket.isPending || !categories.isSuccess}
            className="rounded bg-blue-600 px-4 py-2 text-white disabled:opacity-50"
          >
            {createTicket.isPending ? 'Creating...' : 'Create ticket'}
          </button>

          <Link to="/tickets" className="underline">
            Cancel
          </Link>
        </div>
      </form>
    </div>
  )
}

export default NewTicketPage
