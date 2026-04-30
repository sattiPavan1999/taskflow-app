import { render, screen } from '@testing-library/react';
import EmptyState from '../components/EmptyState';

describe('EmptyState', () => {
  it('renders the empty state message', () => {
    render(<EmptyState />);
    expect(screen.getByText('No todos here!')).toBeInTheDocument();
    expect(screen.getByText('Add one above to get started')).toBeInTheDocument();
  });
});
