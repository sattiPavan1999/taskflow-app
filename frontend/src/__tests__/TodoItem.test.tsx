import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import TodoItem from '../components/TodoItem';
import type { Task } from '../types/task';

const BASE_TASK: Task = {
  id: '1',
  title: 'Write tests',
  isCompleted: false,
  createdAt: '2026-04-01T00:00:00Z',
  priority: 'medium',
  category: null,
  dueDate: null,
};

describe('TodoItem', () => {
  it('renders the task title', () => {
    render(<TodoItem task={BASE_TASK} onToggle={vi.fn()} />);
    expect(screen.getByText('Write tests')).toBeInTheDocument();
  });

  it('renders the priority badge', () => {
    render(<TodoItem task={BASE_TASK} onToggle={vi.fn()} />);
    expect(screen.getByText('medium')).toBeInTheDocument();
  });

  it('applies the correct priority class to the badge', () => {
    const { rerender } = render(<TodoItem task={BASE_TASK} onToggle={vi.fn()} />);
    expect(screen.getByText('medium')).toHaveClass('pri-medium');

    rerender(<TodoItem task={{ ...BASE_TASK, priority: 'high' }} onToggle={vi.fn()} />);
    expect(screen.getByText('high')).toHaveClass('pri-high');

    rerender(<TodoItem task={{ ...BASE_TASK, priority: 'low' }} onToggle={vi.fn()} />);
    expect(screen.getByText('low')).toHaveClass('pri-low');
  });

  it('renders the category badge when category is set', () => {
    render(<TodoItem task={{ ...BASE_TASK, category: 'Work' }} onToggle={vi.fn()} />);
    expect(screen.getByText(/work/i)).toBeInTheDocument();
  });

  it('does not render a category badge when category is null', () => {
    render(<TodoItem task={BASE_TASK} onToggle={vi.fn()} />);
    expect(screen.queryByText(/personal|work|shopping/i)).not.toBeInTheDocument();
  });

  it('renders the due date badge when dueDate is set', () => {
    render(<TodoItem task={{ ...BASE_TASK, dueDate: '2026-06-15T00:00:00Z' }} onToggle={vi.fn()} />);
    expect(screen.getByText(/jun 15/i)).toBeInTheDocument();
  });

  it('does not render a due date badge when dueDate is null', () => {
    render(<TodoItem task={BASE_TASK} onToggle={vi.fn()} />);
    // only priority badge rendered — no calendar-format date text
    expect(screen.queryByText(/jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec/i)).not.toBeInTheDocument();
  });

  it('adds "overdue" class when task is past due and not completed', () => {
    const pastDate = new Date(Date.now() - 86400000).toISOString(); // yesterday
    const { container } = render(
      <TodoItem task={{ ...BASE_TASK, dueDate: pastDate }} onToggle={vi.fn()} />
    );
    expect(container.firstChild).toHaveClass('overdue');
  });

  it('does not add "overdue" class when completed even if past due', () => {
    const pastDate = new Date(Date.now() - 86400000).toISOString();
    const { container } = render(
      <TodoItem task={{ ...BASE_TASK, isCompleted: true, dueDate: pastDate }} onToggle={vi.fn()} />
    );
    expect(container.firstChild).not.toHaveClass('overdue');
    expect(container.firstChild).toHaveClass('completed');
  });

  it('adds "completed" class when task is done', () => {
    const { container } = render(
      <TodoItem task={{ ...BASE_TASK, isCompleted: true }} onToggle={vi.fn()} />
    );
    expect(container.firstChild).toHaveClass('completed');
  });

  it('applies strikethrough class to title when completed', () => {
    render(<TodoItem task={{ ...BASE_TASK, isCompleted: true }} onToggle={vi.fn()} />);
    expect(screen.getByText('Write tests')).toHaveClass('done');
  });

  it('checkbox has cb-checked class when completed', () => {
    render(<TodoItem task={{ ...BASE_TASK, isCompleted: true }} onToggle={vi.fn()} />);
    expect(screen.getByRole('button', { name: /mark incomplete/i })).toHaveClass('cb-checked');
  });

  it('checkbox aria-label is "Mark complete" when not completed', () => {
    render(<TodoItem task={BASE_TASK} onToggle={vi.fn()} />);
    expect(screen.getByRole('button', { name: /mark complete$/i })).toBeInTheDocument();
  });

  it('calls onToggle with the task when checkbox is clicked', async () => {
    const onToggle = vi.fn();
    render(<TodoItem task={BASE_TASK} onToggle={onToggle} />);
    await userEvent.click(screen.getByRole('button', { name: /mark complete/i }));
    expect(onToggle).toHaveBeenCalledWith(BASE_TASK);
  });

  it('due date badge has "due-overdue" class when overdue', () => {
    const pastDate = new Date(Date.now() - 86400000).toISOString();
    render(<TodoItem task={{ ...BASE_TASK, dueDate: pastDate }} onToggle={vi.fn()} />);
    const dueBadge = screen.getByText(/.+ \d+/);
    expect(dueBadge).toHaveClass('due-overdue');
  });

  it('due date badge has "due-done" class when completed', () => {
    const anyDate = '2026-06-01T00:00:00Z';
    render(<TodoItem task={{ ...BASE_TASK, isCompleted: true, dueDate: anyDate }} onToggle={vi.fn()} />);
    const dueBadge = screen.getByText(/jun 1/i);
    expect(dueBadge).toHaveClass('due-done');
  });
});
