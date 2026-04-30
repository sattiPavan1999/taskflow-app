import type { Task, CreateTaskPayload, UpdateTaskPayload } from '../types/task';

const BASE_URL = '/api';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || `Request failed: ${res.status}`);
  }
  if (res.status === 204) return undefined as T;
  return res.json();
}

export const taskApi = {
  getAll: () => request<Task[]>('/tasks'),
  create: (payload: CreateTaskPayload) =>
    request<Task>('/tasks', { method: 'POST', body: JSON.stringify(payload) }),
  update: (id: string, payload: UpdateTaskPayload) =>
    request<Task>(`/tasks/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
  delete: (id: string) => request<void>(`/tasks/${id}`, { method: 'DELETE' }),
};
