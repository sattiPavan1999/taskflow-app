import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import AddTodoForm from '../components/AddTodoForm';

describe('AddTodoForm', () => {
  it('renders the title input and Add button', () => {
    render(<AddTodoForm onAdd={vi.fn()} />);
    expect(screen.getByPlaceholderText('What needs to be done?')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /add/i })).toBeInTheDocument();
  });

  it('does not call onAdd when title is empty', async () => {
    const onAdd = vi.fn();
    render(<AddTodoForm onAdd={onAdd} />);
    await userEvent.click(screen.getByRole('button', { name: /add/i }));
    expect(onAdd).not.toHaveBeenCalled();
  });

  it('does not call onAdd when title is only whitespace', async () => {
    const onAdd = vi.fn();
    render(<AddTodoForm onAdd={onAdd} />);
    await userEvent.type(screen.getByPlaceholderText('What needs to be done?'), '   ');
    await userEvent.click(screen.getByRole('button', { name: /add/i }));
    expect(onAdd).not.toHaveBeenCalled();
  });

  it('calls onAdd with trimmed title and defaults when Add is clicked', async () => {
    const onAdd = vi.fn();
    render(<AddTodoForm onAdd={onAdd} />);
    await userEvent.type(screen.getByPlaceholderText('What needs to be done?'), '  Buy milk  ');
    await userEvent.click(screen.getByRole('button', { name: /add/i }));
    expect(onAdd).toHaveBeenCalledWith('Buy milk', 'medium', 'Personal', '');
  });

  it('calls onAdd when Enter is pressed in the title input', async () => {
    const onAdd = vi.fn();
    render(<AddTodoForm onAdd={onAdd} />);
    await userEvent.type(screen.getByPlaceholderText('What needs to be done?'), 'My task{Enter}');
    expect(onAdd).toHaveBeenCalledWith('My task', 'medium', 'Personal', '');
  });

  it('clears the title input after a successful add', async () => {
    render(<AddTodoForm onAdd={vi.fn()} />);
    const input = screen.getByPlaceholderText('What needs to be done?');
    await userEvent.type(input, 'New task');
    await userEvent.click(screen.getByRole('button', { name: /add/i }));
    expect(input).toHaveValue('');
  });

  it('passes the selected priority to onAdd', async () => {
    const onAdd = vi.fn();
    render(<AddTodoForm onAdd={onAdd} />);
    const prioritySelect = screen.getAllByRole('combobox')[0];
    await userEvent.selectOptions(prioritySelect, 'high');
    await userEvent.type(screen.getByPlaceholderText('What needs to be done?'), 'Urgent task');
    await userEvent.click(screen.getByRole('button', { name: /add/i }));
    expect(onAdd).toHaveBeenCalledWith('Urgent task', 'high', 'Personal', '');
  });

  it('passes the selected category to onAdd', async () => {
    const onAdd = vi.fn();
    render(<AddTodoForm onAdd={onAdd} />);
    const categorySelect = screen.getAllByRole('combobox')[1];
    await userEvent.selectOptions(categorySelect, 'Work');
    await userEvent.type(screen.getByPlaceholderText('What needs to be done?'), 'Work task');
    await userEvent.click(screen.getByRole('button', { name: /add/i }));
    expect(onAdd).toHaveBeenCalledWith('Work task', 'medium', 'Work', '');
  });

  it('shows the date picker when Due Date button is clicked', async () => {
    const { container } = render(<AddTodoForm onAdd={vi.fn()} />);
    await userEvent.click(screen.getByRole('button', { name: /due date/i }));
    expect(container.querySelector('input[type="date"]')).toBeInTheDocument();
  });

  it('shows the clear button after a date is selected', async () => {
    const { container } = render(<AddTodoForm onAdd={vi.fn()} />);
    await userEvent.click(screen.getByRole('button', { name: /due date/i }));
    const dateInput = container.querySelector('input[type="date"]') as HTMLInputElement;
    const { fireEvent } = await import('@testing-library/react');
    fireEvent.change(dateInput, { target: { value: '2026-06-01' } });
    expect(await screen.findByRole('button', { name: /clear due date/i })).toBeInTheDocument();
  });

  it('clears the due date when the clear button is clicked', async () => {
    const { container } = render(<AddTodoForm onAdd={vi.fn()} />);
    await userEvent.click(screen.getByRole('button', { name: /due date/i }));
    const dateInput = container.querySelector('input[type="date"]') as HTMLInputElement;
    const { fireEvent } = await import('@testing-library/react');
    fireEvent.change(dateInput, { target: { value: '2026-06-01' } });
    const clearBtn = await screen.findByRole('button', { name: /clear due date/i });
    await userEvent.click(clearBtn);
    expect(screen.queryByRole('button', { name: /clear due date/i })).not.toBeInTheDocument();
  });
});
