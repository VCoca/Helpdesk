export type TicketStatus = 'New' | 'InProgress' | 'Resolved' | 'Closed' | 'Rejected'
export type Priority = 'Low' | 'Medium' | 'High' | 'Critical'

export interface TicketListItem {
  id: number
  title: string
  status: TicketStatus
  priority: Priority
  categoryName: string
  authorName: string
  assignedAgentName: string | null
  createdAt: string
}

// What POST /api/tickets and GET /api/tickets/{id} return: the list fields plus
// the ones only the detail endpoint sends.
export interface TicketDetail extends TicketListItem {
  description: string
  updatedAt: string
  closedAt: string | null
}

export interface Category {
  id: number
  name: string
}
