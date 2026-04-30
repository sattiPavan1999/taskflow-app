import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import Header from '../components/Header';

describe('Header', () => {
  it('renders the app title and subtitle', () => {
    render(<Header darkMode={false} onToggleDark={vi.fn()} />);
    expect(screen.getByText('Todo Master')).toBeInTheDocument();
    expect(screen.getByText('Stay organized, stay productive')).toBeInTheDocument();
  });

  it('calls onToggleDark when the toggle is clicked', async () => {
    const onToggleDark = vi.fn();
    render(<Header darkMode={false} onToggleDark={onToggleDark} />);
    await userEvent.click(screen.getByRole('button', { name: /toggle dark mode/i }));
    expect(onToggleDark).toHaveBeenCalledTimes(1);
  });

  it('toggle button has "on" class in dark mode', () => {
    render(<Header darkMode={true} onToggleDark={vi.fn()} />);
    expect(screen.getByRole('button', { name: /toggle dark mode/i })).toHaveClass('on');
  });

  it('toggle button does not have "on" class in light mode', () => {
    render(<Header darkMode={false} onToggleDark={vi.fn()} />);
    expect(screen.getByRole('button', { name: /toggle dark mode/i })).not.toHaveClass('on');
  });
});
