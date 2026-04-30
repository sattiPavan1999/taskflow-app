import { Check, Tag, Calendar } from 'lucide-react';
import type { Task } from '../types/task';

interface Props {
  task: Task;
  onToggle: (task: Task) => void;
}

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });

const isOverdue = (task: Task) =>
  !task.isCompleted && !!task.dueDate && new Date(task.dueDate) < new Date();

export default function TodoItem({ task, onToggle }: Props) {
  const overdue = isOverdue(task);

  return (
    <div className={`todo-item${task.isCompleted ? ' completed' : overdue ? ' overdue' : ''}`}>
      <button
        className={`cb${task.isCompleted ? ' cb-checked' : ''}`}
        onClick={() => onToggle(task)}
        aria-label={task.isCompleted ? 'Mark incomplete' : 'Mark complete'}
      >
        {task.isCompleted && <Check size={11} strokeWidth={3} />}
      </button>

      <div className="todo-body">
        <span className={`todo-title${task.isCompleted ? ' done' : ''}`}>
          {task.title}
        </span>
        <div className="badges">
          <span className={`badge pri-${task.priority}`}>{task.priority}</span>

          {task.category && (
            <span className="badge cat-badge">
              <Tag size={10} />
              {task.category}
            </span>
          )}

          {task.dueDate && (
            <span className={`badge due${overdue ? ' due-overdue' : task.isCompleted ? ' due-done' : ''}`}>
              <Calendar size={10} />
              {formatDate(task.dueDate)}
            </span>
          )}
        </div>
      </div>
    </div>
  );
}
