import { useState, useEffect, useCallback } from 'react';
import { taskApi } from './api/taskApi';
import type { Task, Priority, TabFilter } from './types/task';
import Header from './components/Header';
import AddTodoForm from './components/AddTodoForm';
import SearchBar, { type SortField } from './components/SearchBar';
import TabBar from './components/TabBar';
import TodoItem from './components/TodoItem';
import EmptyState from './components/EmptyState';
import ToastContainer, { type ToastItem } from './components/ToastContainer';
import './App.css';

let toastCounter = 0;

export default function App() {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(true);
  const [darkMode, setDarkMode] = useState(false);
  const [activeTab, setActiveTab] = useState<TabFilter>('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [sortField, setSortField] = useState<SortField>('createdAt');
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const showToast = useCallback((message: string) => {
    const id = ++toastCounter;
    setToasts(prev => [...prev, { id, message }]);
    setTimeout(() => setToasts(prev => prev.filter(t => t.id !== id)), 3000);
  }, []);

  useEffect(() => {
    taskApi.getAll()
      .then(setTasks)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const isOverdue = (task: Task) =>
    !task.isCompleted && !!task.dueDate && new Date(task.dueDate) < new Date();

  const filteredTasks = tasks
    .filter(t => {
      if (activeTab === 'active') return !t.isCompleted;
      if (activeTab === 'completed') return t.isCompleted;
      if (activeTab === 'overdue') return isOverdue(t);
      return true;
    })
    .filter(t => t.title.toLowerCase().includes(searchQuery.toLowerCase()))
    .sort((a, b) => {
      if (sortField === 'title') return a.title.localeCompare(b.title);
      if (sortField === 'dueDate') {
        if (!a.dueDate) return 1;
        if (!b.dueDate) return -1;
        return new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
      }
      return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
    });

  const counts: Record<TabFilter, number> = {
    all: tasks.length,
    active: tasks.filter(t => !t.isCompleted).length,
    completed: tasks.filter(t => t.isCompleted).length,
    overdue: tasks.filter(isOverdue).length,
  };

  const handleAdd = async (title: string, priority: Priority, category: string, dueDate: string) => {
    try {
      const created = await taskApi.create({
        title,
        priority,
        category: category || undefined,
        dueDate: dueDate || undefined,
      });
      setTasks(prev => [...prev, created]);
    } catch (e) {
      console.error(e);
    }
  };

  const handleToggle = async (task: Task) => {
    try {
      const updated = await taskApi.update(task.id, {
        title: task.title,
        description: task.description ?? undefined,
        isCompleted: !task.isCompleted,
        priority: task.priority,
        category: task.category ?? undefined,
        dueDate: task.dueDate ?? undefined,
      });
      setTasks(prev => prev.map(t => (t.id === task.id ? updated : t)));
      if (!task.isCompleted) showToast('Todo completed! 🎉');
    } catch (e) {
      console.error(e);
    }
  };

  const handleClearCompleted = async () => {
    const completed = tasks.filter(t => t.isCompleted);
    const results = await Promise.allSettled(completed.map(t => taskApi.delete(t.id)));
    const deletedIds = new Set(
      completed.filter((_, i) => results[i].status === 'fulfilled').map(t => t.id)
    );
    setTasks(prev => prev.filter(t => !deletedIds.has(t.id)));
  };

  const itemsLeft = counts.active;

  return (
    <div className={`app${darkMode ? ' dark' : ''}`}>
      <div className="page-bg">
        <div className="container">
          <Header darkMode={darkMode} onToggleDark={() => setDarkMode(d => !d)} />

          <div className="card">
            <AddTodoForm onAdd={handleAdd} />

            <SearchBar
              searchQuery={searchQuery}
              onSearchChange={setSearchQuery}
              sortField={sortField}
              onSortChange={setSortField}
            />

            <div className="tabs-area">
              <TabBar activeTab={activeTab} counts={counts} onTabChange={setActiveTab} />

              <div className="todo-list">
                {loading ? (
                  <div className="loading-state">Loading...</div>
                ) : filteredTasks.length === 0 ? (
                  <EmptyState />
                ) : (
                  filteredTasks.map(task => (
                    <TodoItem key={task.id} task={task} onToggle={handleToggle} />
                  ))
                )}
              </div>
            </div>

            {tasks.length > 0 && (
              <div className="card-footer">
                <span className="items-left">
                  {itemsLeft} {itemsLeft === 1 ? 'item' : 'items'} left
                </span>
                {counts.completed > 0 && (
                  <button className="clear-completed" onClick={handleClearCompleted}>
                    Clear completed
                  </button>
                )}
              </div>
            )}
          </div>
        </div>

        <ToastContainer toasts={toasts} />
      </div>
    </div>
  );
}
