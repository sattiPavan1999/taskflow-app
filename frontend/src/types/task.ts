export type Priority = 'low' | 'medium' | 'high';
export type TabFilter = 'all' | 'active' | 'completed' | 'overdue';

export interface Task {
  id: number;
  title: string;
  description?: string | null;
  isCompleted: boolean;
  createdAt: string;
  priority: Priority;
  category?: string | null;
  dueDate?: string | null;
}

export interface CreateTaskPayload {
  title: string;
  description?: string;
  priority: Priority;
  category?: string;
  dueDate?: string;
}

export interface UpdateTaskPayload {
  title: string;
  description?: string;
  isCompleted: boolean;
  priority: Priority;
  category?: string;
  dueDate?: string;
}
