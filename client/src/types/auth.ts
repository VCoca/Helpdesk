export type UserRole = 'User' | 'Agent'

/** The body returned by POST /api/auth/login and /api/auth/register. */
export interface AuthResponse {
  token: string
  expiresAtUtc: string
  userId: number
  email: string
  fullName: string
  role: UserRole
}

/** Everything from the login response except the token itself. */
export type CurrentUser = Omit<AuthResponse, 'token'>
