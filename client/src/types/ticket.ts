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