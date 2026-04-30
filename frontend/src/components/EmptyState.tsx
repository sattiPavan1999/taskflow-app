import { ClipboardList } from 'lucide-react';

export default function EmptyState() {
  return (
    <div className="empty-state">
      <ClipboardList size={48} className="empty-icon" strokeWidth={1.2} />
      <p className="empty-title">No todos here!</p>
      <p className="empty-sub">Add one above to get started</p>
    </div>
  );
}
