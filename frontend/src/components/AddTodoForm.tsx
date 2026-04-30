import { useState } from 'react';
import { Plus, Calendar, ChevronDown, X } from 'lucide-react';
import type { Priority } from '../types/task';

interface Props {
  onAdd: (title: string, priority: Priority, category: string, dueDate: string) => void;
}

export default function AddTodoForm({ onAdd }: Props) {
  const [title, setTitle] = useState('');
  const [priority, setPriority] = useState<Priority>('medium');
  const [category, setCategory] = useState('Personal');
  const [dueDate, setDueDate] = useState('');
  const [showDatePicker, setShowDatePicker] = useState(false);

  const handleAdd = () => {
    if (!title.trim()) return;
    onAdd(title.trim(), priority, category, dueDate);
    setTitle('');
    setDueDate('');
    setShowDatePicker(false);
  };

  const formatDate = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  };

  return (
    <div className="add-section">
      <div className="add-row">
        <input
          className="title-input"
          placeholder="What needs to be done?"
          value={title}
          onChange={e => setTitle(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && handleAdd()}
        />
        <button className="add-btn" onClick={handleAdd}>
          <Plus size={14} />
          Add
        </button>
      </div>

      <div className="filter-row">
        <div className="filter-dropdown">
          <select
            value={priority}
            onChange={e => setPriority(e.target.value as Priority)}
            className="dropdown-select"
          >
            <option value="low">Low Priority</option>
            <option value="medium">Medium Priority</option>
            <option value="high">High Priority</option>
          </select>
          <ChevronDown size={12} className="dropdown-caret" />
        </div>

        <div className="filter-dropdown">
          <select
            value={category}
            onChange={e => setCategory(e.target.value)}
            className="dropdown-select"
          >
            <option value="Personal">Personal</option>
            <option value="Work">Work</option>
            <option value="Shopping">Shopping</option>
          </select>
          <ChevronDown size={12} className="dropdown-caret" />
        </div>

        <div className="date-wrap">
          <button
            className={`date-btn${dueDate ? ' has-date' : ''}`}
            onClick={() => setShowDatePicker(s => !s)}
          >
            <Calendar size={13} />
            {dueDate ? formatDate(dueDate) : 'Due Date'}
          </button>
          {dueDate && (
            <button
              className="clear-date"
              onClick={() => { setDueDate(''); setShowDatePicker(false); }}
              aria-label="Clear due date"
            >
              <X size={11} />
            </button>
          )}
          {showDatePicker && (
            <input
              type="date"
              className="date-picker"
              value={dueDate}
              onChange={e => { setDueDate(e.target.value); setShowDatePicker(false); }}
              onBlur={() => setTimeout(() => setShowDatePicker(false), 150)}
              autoFocus
            />
          )}
        </div>
      </div>
    </div>
  );
}
