import type { AuthResponse, LoginPayload, RegisterPayload } from '../types/auth';

const BASE_URL = '/auth';

async function request<T>(path: string, body: unknown): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error((err as { message?: string }).message || `Request failed: ${res.status}`);
  }
  return res.json();
}

export const authApi = {
  login: (payload: LoginPayload) => request<AuthResponse>('/login', payload),
  register: (payload: RegisterPayload) => request<AuthResponse>('/register', payload),
};
