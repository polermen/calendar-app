import { useState, useEffect } from 'react';
import { todoService } from '../../services/todoService';
import './TodoListSidebar.css';

function TodoListSidebar({ scope, date, spectateOwnerId = null, readOnly = false }) {
  const [todoLists, setTodoLists] = useState([]);
  const [newListTitle, setNewListTitle] = useState('');
  const [newItemText, setNewItemText] = useState({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    loadTodoLists();
  }, [scope, date, spectateOwnerId]);

  const loadTodoLists = async () => {
    try {
      setLoading(true);
      const formattedDate = date ? date.toISOString().split('T')[0] : null;

      // Get all todo lists and filter by scope and date on the client side
      const allLists = await todoService.getAllTodoLists(null, null, spectateOwnerId);
      const filteredLists = allLists.filter(list => {
        if (list.scope !== scope) return false;

        if (formattedDate && list.listDate) {
          const listDate = new Date(list.listDate);
          const targetDate = new Date(formattedDate);

          if (scope === 'Year') {
            return listDate.getFullYear() === targetDate.getFullYear();
          } else if (scope === 'Month') {
            return listDate.getFullYear() === targetDate.getFullYear() &&
                   listDate.getMonth() === targetDate.getMonth();
          } else if (scope === 'Day') {
            return listDate.toDateString() === targetDate.toDateString();
          }
        }

        return true;
      });

      setTodoLists(filteredLists);
      setError('');
    } catch (err) {
      setError('Failed to load todos');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateList = async (e) => {
    e.preventDefault();
    if (!newListTitle.trim()) return;

    try {
      const formattedDate = date ? date.toISOString().split('T')[0] : null;
      await todoService.createTodoList({
        title: newListTitle,
        scope: scope,
        listDate: formattedDate
      });
      setNewListTitle('');
      loadTodoLists();
    } catch (err) {
      setError('Failed to create todo list');
      console.error(err);
    }
  };

  const handleAddItem = async (listId) => {
    const text = newItemText[listId];
    if (!text?.trim()) return;

    try {
      await todoService.addTodoItem(listId, {
        text: text,
        priority: 0
      });
      setNewItemText({ ...newItemText, [listId]: '' });
      loadTodoLists();
    } catch (err) {
      setError('Failed to add item');
      console.error(err);
    }
  };

  const handleToggleItem = async (listId, itemId) => {
    try {
      await todoService.toggleTodoItem(itemId);
      loadTodoLists();
    } catch (err) {
      setError('Failed to toggle item');
      console.error(err);
    }
  };

  const handleDeleteList = async (listId) => {
    if (!window.confirm('Delete this todo list?')) return;

    try {
      await todoService.deleteTodoList(listId);
      loadTodoLists();
    } catch (err) {
      setError('Failed to delete list');
      console.error(err);
    }
  };

  const handleDeleteItem = async (listId, itemId) => {
    try {
      await todoService.deleteTodoItem(itemId);
      loadTodoLists();
    } catch (err) {
      setError('Failed to delete item');
      console.error(err);
    }
  };

  return (
    <div className="todo-sidebar">
      <div className="todo-header">
        <h3>📝 Todo Lists</h3>
        <p className="todo-scope">{scope} View</p>
      </div>

      {error && <div className="error-message-small">{error}</div>}

      {!readOnly && (
        <form onSubmit={handleCreateList} className="new-list-form">
          <input
            type="text"
            value={newListTitle}
            onChange={(e) => setNewListTitle(e.target.value)}
            placeholder="New todo list name..."
            className="new-list-input"
          />
          <button type="submit" className="btn-add-list">
            + Add List
          </button>
        </form>
      )}

      {loading && <p className="loading-text">Loading todos...</p>}

      <div className="todo-lists">
        {todoLists.length === 0 && !loading && (
          <p className="empty-message">No todo lists yet. Create one above!</p>
        )}

        {todoLists.map(list => (
          <div key={list.todoListId} className="todo-list-card">
            <div className="todo-list-header">
              <h4>{list.title}</h4>
              {!readOnly && (
                <button
                  onClick={() => handleDeleteList(list.todoListId)}
                  className="btn-delete-list"
                  title="Delete list"
                >
                  🗑️
                </button>
              )}
            </div>

            <div className="todo-items">
              {list.items && list.items.length > 0 ? (
                list.items.map(item => (
                  <div key={item.todoItemId} className="todo-item">
                    <input
                      type="checkbox"
                      checked={item.isCompleted}
                      onChange={() => handleToggleItem(list.todoListId, item.todoItemId)}
                      className="todo-checkbox"
                      disabled={readOnly}
                    />
                    <span className={item.isCompleted ? 'completed' : ''}>
                      {item.text}
                    </span>
                    {!readOnly && (
                      <button
                        onClick={() => handleDeleteItem(list.todoListId, item.todoItemId)}
                        className="btn-delete-item"
                        title="Delete item"
                      >
                        ×
                      </button>
                    )}
                  </div>
                ))
              ) : (
                <p className="empty-items">No items yet</p>
              )}
            </div>

            {!readOnly && (
              <div className="add-item-form">
                <input
                  type="text"
                  value={newItemText[list.todoListId] || ''}
                  onChange={(e) =>
                    setNewItemText({ ...newItemText, [list.todoListId]: e.target.value })
                  }
                  onKeyPress={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault();
                      handleAddItem(list.todoListId);
                    }
                  }}
                  placeholder="Add new item..."
                  className="add-item-input"
                />
                <button
                  onClick={() => handleAddItem(list.todoListId)}
                  className="btn-add-item"
                >
                  +
                </button>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

export default TodoListSidebar;
