import type { TabFilter } from '../types/task';

interface Props {
  activeTab: TabFilter;
  counts: Record<TabFilter, number>;
  onTabChange: (tab: TabFilter) => void;
}

const TABS: TabFilter[] = ['all', 'active', 'completed', 'overdue'];

export default function TabBar({ activeTab, counts, onTabChange }: Props) {
  return (
    <div className="tab-list">
      {TABS.map(tab => (
        <button
          key={tab}
          className={`tab-btn${activeTab === tab ? ' active' : ''}`}
          onClick={() => onTabChange(tab)}
        >
          {tab.charAt(0).toUpperCase() + tab.slice(1)}
          <span className="tab-badge">{counts[tab]}</span>
        </button>
      ))}
    </div>
  );
}
