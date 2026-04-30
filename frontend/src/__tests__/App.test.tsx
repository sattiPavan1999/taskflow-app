import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi, type Mock } from 'vitest';
import App from '../App';
import { taskApi } from '../api/taskApi';
import type { Task } from '../types/task';

vi.mock('../api/taskApi');

const MOCK_TASKS: Task[] = [
  {
    id: '1',
    title: 'Active task',
    isCompleted: false,
    createdAt: '2026-04-01T00:00:00Z',
    priority: 'medium',
    category: 'Personal',
    dueDate: null,
  },
  {
    id: '2',
    title: 'Done task',
    isCompleted: true,
    createdAt: '2026-04-02T00:00:00Z',
    priority: 'low',
    category: 'Work',
    dueDate: null,
  },
  {
    id: '3',
    title: 'Overdue task',
    isCompleted: false,
    createdAt: '2026-04-01T00:00:00Z',
    priority: 'high',
    category: null,
    dueDate: '2026-01-01T00:00:00Z', // past date
  },
];

beforeEach(() => {
  (taskApi.getAll as Mock).mockResolvedValue(MOCK_TASKS);
  (taskApi.create as Mock).mockResolvedValue({
    id: '4',
    title: 'New task',
    isCompleted: false,
    createdAt: new Date().toISOString(),
    priority: 'medium',
    category: 'Personal',
    dueDate: null,
  } satisfies Task);
  (taskApi.update as Mock).mockImplementation((_id, payload) =>
    Promise.resolve({ ...MOCK_TASKS[0], ...payload })
  );
  (taskApi.delete as Mock).mockResolvedValue(undefined);
});

afterEach(() => vi.clearAllMocks());

describe('App — initial load', () => {
  it('shows all tasks after loading', async () => {
    render(<App />);
    await waitFor(() => {
      expect(screen.getByText('Active task')).toBeInTheDocument();
      expect(screen.getByText('Done task')).toBeInTheDocument();
      expect(screen.getByText('Overdue task')).toBeInTheDocument();
    });
  });

  it('shows correct tab counts after loading', async () => {
    render(<App />);
    await waitFor(() => {
      // All=3, Active=2, Completed=1, Overdue=1
      const badges = screen.getAllByText(/^\d+$/);
      const values = badges.map(b => b.textContent);
      expect(values).toContain('3');
      expect(values).toContain('2');
      expect(values).toContain('1');
    });
  });

  it('shows items-left count after loading', async () => {
    render(<App />);
    await waitFor(() => expect(screen.getByText('2 items left')).toBeInTheDocument());
  });
});

describe('App — dark mode', () => {
  it('toggles the dark class on the root element', async () => {
    const { container } = render(<App />);
    const root = container.firstChild as HTMLElement;
    expect(root).not.toHaveClass('dark');
    await userEvent.click(screen.getByRole('button', { name: /toggle dark mode/i }));
    expect(root).toHaveClass('dark');
    await userEvent.click(screen.getByRole('button', { name: /toggle dark mode/i }));
    expect(root).not.toHaveClass('dark');
  });
});

describe('App — adding a task', () => {
  it('calls taskApi.create and appends the task to the list', async () => {
    render(<App />);
    await waitFor(() => screen.getByText('Active task'));

    await userEvent.type(screen.getByPlaceholderText('What needs to be done?'), 'New task');
    await userEvent.click(screen.getByRole('button', { name: /^add/i }));

    await waitFor(() => expect(taskApi.create).toHaveBeenCalledWith(
      expect.objectContaining({ title: 'New task' })
    ));
    await waitFor(() => expect(screen.getByText('New task')).toBeInTheDocument());
  });

  it('clears the input after adding', async () => {
    render(<App />);
    await waitFor(() => screen.getByText('Active task'));
    const input = screen.getByPlaceholderText('What needs to be done?');
    await userEvent.type(input, 'New task');
    await userEvent.click(screen.getByRole('button', { name: /^add/i }));
    await waitFor(() => expect(input).toHaveValue(''));
  });
});

describe('App — completing a task', () => {
  it('calls taskApi.update with isCompleted=true when checkbox clicked', async () => {
    render(<App />);
    await waitFor(() => screen.getByText('Active task'));

    // Click the first "Mark complete" checkbox (Active task, id=1)
    const checkboxes = screen.getAllByRole('button', { name: /mark complete$/i });
    await userEvent.click(checkboxes[0]);

    await waitFor(() => expect(taskApi.update).toHaveBeenCalledWith(
      '1',
      expect.objectContaining({ isCompleted: true })
    ));
  });

  it('shows the toast after completing a task', async () => {
    (taskApi.update as Mock).mockResolvedValue({ ...MOCK_TASKS[0], isCompleted: true });
    render(<App />);
    await waitFor(() => screen.getByText('Active task'));

    const checkboxes = screen.getAllByRole('button', { name: /mark complete$/i });
    await userEvent.click(checkboxes[0]);
    await waitFor(() =>
      expect(screen.getByText('Todo completed! 🎉')).toBeInTheDocument()
    );
  });

  it('shows "Clear completed" button when there are completed tasks', async () => {
    render(<App />);
    await waitFor(() =>
      expect(screen.getByRole('button', { name: /clear completed/i })).toBeInTheDocument()
    );
  });
});

describe('App — clearing completed', () => {
  it('calls taskApi.delete for each completed task and removes them', async () => {
    render(<App />);
    await waitFor(() => screen.getByText('Done task'));

    await userEvent.click(screen.getByRole('button', { name: /clear completed/i }));

    await waitFor(() => {
      expect(taskApi.delete).toHaveBeenCalledWith('2');
      expect(screen.queryByText('Done task')).not.toBeInTheDocument();
    });
  });
});

describe('App — search', () => {
  it('filters the visible tasks by search query', async () => {
    render(<App />);
    await waitFor(() => screen.getByText('Active task'));

    await userEvent.type(screen.getByPlaceholderText('Search todos...'), 'done');

    expect(screen.queryByText('Active task')).not.toBeInTheDocument();
    expect(screen.getByText('Done task')).toBeInTheDocument();
  });

  it('shows empty state when search has no matches', async () => {
    render(<App />);
    await waitFor(() => screen.getByText('Active task'));

    await userEvent.type(screen.getByPlaceholderText('Search todos...'), 'xyznotfound');

    expect(screen.getByText('No todos here!')).toBeInTheDocument();
  });
});

describe('App — tab filtering', () => {
  it('shows only active tasks on the Active tab', async () => {
    render(<App />);
    await waitFor(() => screen.getByText('Active task'));

    await userEvent.click(screen.getByRole('button', { name: /^active/i }));

    expect(screen.getByText('Active task')).toBeInTheDocument();
    expect(screen.queryByText('Done task')).not.toBeInTheDocument();
  });

  it('shows only completed tasks on the Completed tab', async () => {
    render(<App />);
    await waitFor(() => screen.getByText('Done task'));

    await userEvent.click(screen.getByRole('button', { name: /^completed/i }));

    expect(screen.getByText('Done task')).toBeInTheDocument();
    expect(screen.queryByText('Active task')).not.toBeInTheDocument();
  });

  it('shows only overdue tasks on the Overdue tab', async () => {
    render(<App />);
    await waitFor(() => screen.getByText('Overdue task'));

    await userEvent.click(screen.getByRole('button', { name: /^overdue/i }));

    expect(screen.getByText('Overdue task')).toBeInTheDocument();
    expect(screen.queryByText('Active task')).not.toBeInTheDocument();
    expect(screen.queryByText('Done task')).not.toBeInTheDocument();
  });

  it('shows empty state when no tasks match the active tab', async () => {
    (taskApi.getAll as Mock).mockResolvedValue([]);
    render(<App />);
    await waitFor(() => expect(screen.getByText('No todos here!')).toBeInTheDocument());
  });
});

describe('App — empty state', () => {
  it('shows empty state when there are no tasks', async () => {
    (taskApi.getAll as Mock).mockResolvedValue([]);
    render(<App />);
    await waitFor(() => {
      expect(screen.getByText('No todos here!')).toBeInTheDocument();
      expect(screen.getByText('Add one above to get started')).toBeInTheDocument();
    });
  });

  it('does not render items-left footer when there are no tasks', async () => {
    (taskApi.getAll as Mock).mockResolvedValue([]);
    render(<App />);
    await waitFor(() => screen.getByText('No todos here!'));
    expect(screen.queryByText(/items? left/)).not.toBeInTheDocument();
  });
});

describe('App — sort', () => {
  it('sorts by title when Title sort is selected', async () => {
    const { container } = render(<App />);
    await waitFor(() => screen.getByText('Active task'));

    // The sort select is the one inside .sort-wrap
    const sortSelect = container.querySelector('.sort-select') as HTMLSelectElement;
    await userEvent.selectOptions(sortSelect, 'title');

    const titles = Array.from(container.querySelectorAll('.todo-title')).map(el => el.textContent);
    expect(titles).toEqual([...titles].sort());
  });
});
