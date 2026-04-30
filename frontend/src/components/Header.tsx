import { Sun, Moon } from 'lucide-react';

interface Props {
  darkMode: boolean;
  onToggleDark: () => void;
}

export default function Header({ darkMode, onToggleDark }: Props) {
  return (
    <div className="header">
      <div>
        <h1 className="app-title">Todo Master</h1>
        <p className="app-subtitle">Stay organized, stay productive</p>
      </div>
      <div className="theme-toggle">
        <Sun size={14} className="theme-icon" />
        <button
          className={`toggle-switch${darkMode ? ' on' : ''}`}
          onClick={onToggleDark}
          aria-label="Toggle dark mode"
        >
          <span className="toggle-thumb" />
        </button>
        <Moon size={14} className="theme-icon" />
      </div>
    </div>
  );
}
