import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import SearchBar from '../components/SearchBar';
import type { SortField } from '../components/SearchBar';

const defaultProps = {
  searchQuery: '',
  onSearchChange: vi.fn(),
  sortField: 'createdAt' as SortField,
  onSortChange: vi.fn(),
};

describe('SearchBar', () => {
  it('renders search input with placeholder', () => {
    render(<SearchBar {...defaultProps} />);
    expect(screen.getByPlaceholderText('Search todos...')).toBeInTheDocument();
  });

  it('displays the current search query', () => {
    render(<SearchBar {...defaultProps} searchQuery="buy milk" />);
    expect(screen.getByDisplayValue('buy milk')).toBeInTheDocument();
  });

  it('calls onSearchChange when the user types', async () => {
    const onSearchChange = vi.fn();
    render(<SearchBar {...defaultProps} onSearchChange={onSearchChange} />);
    await userEvent.type(screen.getByPlaceholderText('Search todos...'), 'hello');
    expect(onSearchChange).toHaveBeenCalled();
  });

  it('renders the sort select with Date Created as default', () => {
    render(<SearchBar {...defaultProps} />);
    expect(screen.getByRole('combobox')).toHaveValue('createdAt');
  });

  it('calls onSortChange when sort selection changes', async () => {
    const onSortChange = vi.fn();
    render(<SearchBar {...defaultProps} onSortChange={onSortChange} />);
    await userEvent.selectOptions(screen.getByRole('combobox'), 'title');
    expect(onSortChange).toHaveBeenCalledWith('title');
  });

  it('reflects the current sortField value in the select', () => {
    render(<SearchBar {...defaultProps} sortField="dueDate" />);
    expect(screen.getByRole('combobox')).toHaveValue('dueDate');
  });
});
