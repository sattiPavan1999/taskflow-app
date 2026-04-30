import { render, screen } from '@testing-library/react';
import ToastContainer from '../components/ToastContainer';

describe('ToastContainer', () => {
  it('renders nothing when toasts array is empty', () => {
    const { container } = render(<ToastContainer toasts={[]} />);
    expect(container.firstChild).toBeNull();
  });

  it('renders a toast for each item', () => {
    const toasts = [
      { id: 1, message: 'Todo completed! 🎉' },
      { id: 2, message: 'Another toast' },
    ];
    render(<ToastContainer toasts={toasts} />);
    expect(screen.getByText('Todo completed! 🎉')).toBeInTheDocument();
    expect(screen.getByText('Another toast')).toBeInTheDocument();
  });

  it('renders one toast element per item', () => {
    const toasts = [
      { id: 1, message: 'First' },
      { id: 2, message: 'Second' },
      { id: 3, message: 'Third' },
    ];
    render(<ToastContainer toasts={toasts} />);
    expect(screen.getAllByRole('generic', { hidden: true }).filter(
      el => el.classList.contains('toast')
    )).toHaveLength(3);
  });
});
