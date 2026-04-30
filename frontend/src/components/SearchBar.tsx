import { Search, ArrowUpDown, ChevronDown } from 'lucide-react';

export type SortField = 'createdAt' | 'dueDate' | 'title';

interface Props {
  searchQuery: string;
  onSearchChange: (v: string) => void;
  sortField: SortField;
  onSortChange: (v: SortField) => void;
}

export default function SearchBar({ searchQuery, onSearchChange, sortField, onSortChange }: Props) {
  return (
    <div className="search-row">
      <div className="search-wrap">
        <Search size={14} className="search-icon" />
        <input
          className="search-input"
          placeholder="Search todos..."
          value={searchQuery}
          onChange={e => onSearchChange(e.target.value)}
        />
      </div>
      <div className="sort-wrap">
        <ArrowUpDown size={14} className="sort-icon" />
        <select
          value={sortField}
          onChange={e => onSortChange(e.target.value as SortField)}
          className="sort-select"
        >
          <option value="createdAt">Date Created</option>
          <option value="dueDate">Due Date</option>
          <option value="title">Title</option>
        </select>
        <ChevronDown size={12} className="dropdown-caret" />
      </div>
    </div>
  );
}
