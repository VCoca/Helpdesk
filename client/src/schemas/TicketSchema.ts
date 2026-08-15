import { z } from 'zod'

export const createTicketSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200),
  description: z.string().min(1, 'Description is required').max(2000),
  priority: z.enum(['Low', 'Medium', 'High', 'Critical']),
  categoryId: z.coerce.number().int().positive('Choose a category'),
})

export type CreateTicketFormValues = z.input<typeof createTicketSchema>
export type CreateTicketInput = z.output<typeof createTicketSchema>
