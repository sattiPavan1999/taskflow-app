import { CheckCircle2 } from 'lucide-react';

export interface ToastItem {
  id: number;
  message: string;
}

interface Props {
  toasts: ToastItem[];
}

export default function ToastContainer({ toasts }: Props) {
  if (toasts.length === 0) return null;

  return (
    <div className="toast-area">
      {toasts.map(t => (
        <div key={t.id} className="toast">
          <CheckCircle2 size={18} className="toast-check" />
          {t.message}
        </div>
      ))}
    </div>
  );
}
