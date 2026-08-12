import { z } from 'zod'

export const createTicketSchema = z.object({
  title: z.string().min(1, 'Title is required').max(200),
  description: z.string().min(1, 'Description is required').max(2000),
  priority: z.enum(['Low', 'Medium', 'High', 'Critical']),
  categoryId: z.coerce.number().int().positive('Choose a category'),
})

// Two different types, because `coerce` makes them different. The form holds
// what the <select> gives us (a string); the API gets what Zod parsed it into
// (a number). React Hook Form needs to be told about both.
export type CreateTicketFormValues = z.input<typeof createTicketSchema>
export type CreateTicketInput = z.output<typeof createTicketSchema>
