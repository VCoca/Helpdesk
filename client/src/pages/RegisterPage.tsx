import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'

import { apiPost } from '../api/client'
import { saveAuth } from '../api/auth'
import { registerSchema, type RegisterInput } from '../schemas/AuthSchema'
import type { AuthResponse } from '../types/auth'

function RegisterPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterInput>({
    resolver: zodResolver(registerSchema),
    defaultValues: { fullName: '', email: '', password: '' },
  })

  const createAccount = useMutation({
    mutationFn: (values: RegisterInput) =>
      apiPost<RegisterInput, AuthResponse>('/api/auth/register', values),
    onSuccess: (auth) => {
      saveAuth(auth)
      queryClient.clear()
      navigate('/tickets', { replace: true })
    },
  })

  return (
    <div className="mx-auto max-w-sm p-6">
      <h1 className="mb-6 text-2xl font-semibold">Create an account</h1>

      <form
        onSubmit={handleSubmit((values) => createAccount.mutate(values))}
        noValidate
        className="flex flex-col gap-4"
      >
        <div className="flex flex-col gap-1">
          <label htmlFor="fullName" className="font-medium">Full name</label>
          <input
            id="fullName"
            type="text"
            autoComplete="name"
            {...register('fullName')}
            aria-invalid={errors.fullName ? 'true' : 'false'}
            className="rounded border border-gray-400 px-3 py-2"
          />
          {errors.fullName && <p className="text-sm text-red-600">{errors.fullName.message}</p>}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="email" className="font-medium">Email</label>
          <input
            id="email"
            type="email"
            autoComplete="email"
            {...register('email')}
            aria-invalid={errors.email ? 'true' : 'false'}
            className="rounded border border-gray-400 px-3 py-2"
          />
          {errors.email && <p className="text-sm text-red-600">{errors.email.message}</p>}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="password" className="font-medium">Password</label>
          <input
            id="password"
            type="password"
            autoComplete="new-password"
            {...register('password')}
            aria-invalid={errors.password ? 'true' : 'false'}
            className="rounded border border-gray-400 px-3 py-2"
          />
          {errors.password && <p className="text-sm text-red-600">{errors.password.message}</p>}
          <p className="text-sm text-gray-500">
            At least 8 characters, with an uppercase letter, a lowercase letter, a digit and a symbol.
          </p>
        </div>

        {/* Everything the browser could not know: email already taken, and the
            password complexity rules Identity applies. */}
        {createAccount.isError && (
          <p className="text-sm text-red-600">{createAccount.error.message}</p>
        )}

        <button
          type="submit"
          disabled={createAccount.isPending}
          className="rounded bg-blue-600 px-4 py-2 text-white disabled:opacity-50"
        >
          {createAccount.isPending ? 'Creating account...' : 'Create account'}
        </button>
      </form>

      <p className="mt-6 text-sm">
        Already have an account? <Link to="/login" className="underline">Log in</Link>
      </p>

      <p className="mt-2 text-sm text-gray-500">
        New accounts are always Users. Agent accounts are not self-service.
      </p>
    </div>
  )
}

export default RegisterPage
