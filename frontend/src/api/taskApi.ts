import type { Task, CreateTaskPayload, UpdateTaskPayload } from '../types/task';

const BASE_URL = '/api';

function getToken(): string | null {
  return localStorage.getItem('auth_token');
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const token = getToken();
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetch(`${BASE_URL}${path}`, { ...options, headers });

  if (res.status === 401) {
    localStorage.removeItem('auth_token');
    window.location.href = '/login';
    throw new Error('Unauthorized');
  }

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error((err as { message?: string }).message || `Request failed: ${res.status}`);
  }
  if (res.status === 204) return undefined as T;
  return res.json();
}

export const taskApi = {
  getAll: () => request<Task[]>('/tasks'),
  create: (payload: CreateTaskPayload) =>
    request<Task>('/tasks', { method: 'POST', body: JSON.stringify(payload) }),
  update: (id: number, payload: UpdateTaskPayload) =>
    request<Task>(`/tasks/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
  delete: (id: number) => request<void>(`/tasks/${id}`, { method: 'DELETE' }),
};
