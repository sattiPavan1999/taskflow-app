import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import TabBar from '../components/TabBar';
import type { TabFilter } from '../types/task';

const defaultCounts: Record<TabFilter, number> = {
  all: 5,
  active: 3,
  completed: 1,
  overdue: 1,
};

describe('TabBar', () => {
  it('renders all four tab labels', () => {
    render(<TabBar activeTab="all" counts={defaultCounts} onTabChange={vi.fn()} />);
    expect(screen.getByRole('button', { name: /all/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /active/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /completed/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /overdue/i })).toBeInTheDocument();
  });

  it('displays the correct count for each tab', () => {
    render(<TabBar activeTab="all" counts={defaultCounts} onTabChange={vi.fn()} />);
    const badges = screen.getAllByText(/^\d+$/);
    const badgeValues = badges.map(b => b.textContent);
    expect(badgeValues).toContain('5');
    expect(badgeValues).toContain('3');
    expect(badgeValues).toContain('1');
  });

  it('applies "active" class to the active tab only', () => {
    render(<TabBar activeTab="active" counts={defaultCounts} onTabChange={vi.fn()} />);
    expect(screen.getByRole('button', { name: /^active/i })).toHaveClass('active');
    expect(screen.getByRole('button', { name: /^all/i })).not.toHaveClass('active');
    expect(screen.getByRole('button', { name: /^completed/i })).not.toHaveClass('active');
    expect(screen.getByRole('button', { name: /^overdue/i })).not.toHaveClass('active');
  });

  it('calls onTabChange with the correct tab when clicked', async () => {
    const onTabChange = vi.fn();
    render(<TabBar activeTab="all" counts={defaultCounts} onTabChange={onTabChange} />);
    await userEvent.click(screen.getByRole('button', { name: /^completed/i }));
    expect(onTabChange).toHaveBeenCalledWith('completed');
  });

  it('calls onTabChange for each tab with the right value', async () => {
    const onTabChange = vi.fn();
    render(<TabBar activeTab="all" counts={defaultCounts} onTabChange={onTabChange} />);

    for (const [label, value] of [
      ['all', 'all'],
      ['active', 'active'],
      ['overdue', 'overdue'],
    ] as const) {
      await userEvent.click(screen.getByRole('button', { name: new RegExp(`^${label}`, 'i') }));
      expect(onTabChange).toHaveBeenCalledWith(value);
    }
  });
});
