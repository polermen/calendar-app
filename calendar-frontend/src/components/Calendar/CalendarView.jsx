import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../../services/authService';
import { taskService } from '../../services/taskService';
import YearView from './YearView';
import MonthView from './MonthView';
import WeekView from './WeekView';
import DayView from './DayView';
import AddTaskModal from '../Tasks/AddTaskModal';
import TodoListSidebar from '../Todo/TodoListSidebar';
import ShareModal from '../Share/ShareModal';
import SpectateView from '../Share/SpectateView';
import './Calendar.css';

function CalendarView() {
  const navigate = useNavigate();
  const user = authService.getStoredUser();

  const today = new Date();
  const [currentView, setCurrentView] = useState('year');
  const [isSpectating, setIsSpectating] = useState(false);
  const [spectatingOwnerId, setSpectatingOwnerId] = useState(null);
  const [currentYear, setCurrentYear] = useState(today.getFullYear());
  const [currentMonth, setCurrentMonth] = useState(today.getMonth());
  const [currentDay, setCurrentDay] = useState(today.getDate());
  const [isTaskModalOpen, setIsTaskModalOpen] = useState(false);
  const [selectedDate, setSelectedDate] = useState(null);
  const [tasks, setTasks] = useState([]);
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const [resetCountdown, setResetCountdown] = useState(null);
  const [resetTimerId, setResetTimerId] = useState(null);
  const [isShareModalOpen, setIsShareModalOpen] = useState(false);

  useEffect(() => {
    loadTasks();
  }, [currentView, currentYear, currentMonth, refreshTrigger, spectatingOwnerId]);

  useEffect(() => {
    return () => {
      if (resetTimerId) {
        clearInterval(resetTimerId);
      }
    };
  }, [resetTimerId]);

  const loadTasks = async () => {
    try {
      let taskData;
      if (currentView === 'year') {
        const startDate = new Date(currentYear, 0, 1).toISOString();
        const endDate = new Date(currentYear, 11, 31).toISOString();
        taskData = await taskService.getAllTasks(startDate, endDate, spectatingOwnerId);
      } else if (currentView === 'month') {
        const startDate = new Date(currentYear, currentMonth, 1).toISOString();
        const endDate = new Date(currentYear, currentMonth + 1, 0).toISOString();
        taskData = await taskService.getAllTasks(startDate, endDate, spectatingOwnerId);
      } else {
        taskData = await taskService.getAllTasks(null, null, spectatingOwnerId);
      }
      setTasks(taskData || []);
    } catch (err) {
      console.error('Failed to load tasks:', err);
      setTasks([]);
    }
  };

  const handleLogout = async () => {
    await authService.logout();
    navigate('/login');
  };

  const handleMonthClick = (year, month) => {
    setCurrentYear(year);
    setCurrentMonth(month);
    setCurrentView('month');
  };

  const handleDayClick = (year, month, day) => {
    setCurrentYear(year);
    setCurrentMonth(month);
    setCurrentDay(day);
    setCurrentView('day');
  };

  const goToToday = () => {
    const now = new Date();
    setCurrentYear(now.getFullYear());
    setCurrentMonth(now.getMonth());
    setCurrentDay(now.getDate());
  };

  const previousPeriod = () => {
    if (currentView === 'year') {
      setCurrentYear(currentYear - 1);
    } else if (currentView === 'month') {
      const newDate = new Date(currentYear, currentMonth - 1);
      setCurrentYear(newDate.getFullYear());
      setCurrentMonth(newDate.getMonth());
    } else if (currentView === 'week') {
      const newDate = new Date(currentYear, currentMonth, currentDay - 7);
      setCurrentYear(newDate.getFullYear());
      setCurrentMonth(newDate.getMonth());
      setCurrentDay(newDate.getDate());
    } else if (currentView === 'day') {
      const newDate = new Date(currentYear, currentMonth, currentDay - 1);
      setCurrentYear(newDate.getFullYear());
      setCurrentMonth(newDate.getMonth());
      setCurrentDay(newDate.getDate());
    }
  };

  const nextPeriod = () => {
    if (currentView === 'year') {
      setCurrentYear(currentYear + 1);
    } else if (currentView === 'month') {
      const newDate = new Date(currentYear, currentMonth + 1);
      setCurrentYear(newDate.getFullYear());
      setCurrentMonth(newDate.getMonth());
    } else if (currentView === 'week') {
      const newDate = new Date(currentYear, currentMonth, currentDay + 7);
      setCurrentYear(newDate.getFullYear());
      setCurrentMonth(newDate.getMonth());
      setCurrentDay(newDate.getDate());
    } else if (currentView === 'day') {
      const newDate = new Date(currentYear, currentMonth, currentDay + 1);
      setCurrentYear(newDate.getFullYear());
      setCurrentMonth(newDate.getMonth());
      setCurrentDay(newDate.getDate());
    }
  };

  const handleAddTask = (date = null) => {
    setSelectedDate(date);
    setIsTaskModalOpen(true);
  };

  const handleTaskSubmit = async (taskData) => {
    try {
      await taskService.createTask(taskData);
      setRefreshTrigger(prev => prev + 1);
    } catch (err) {
      console.error('Failed to create task:', err);
      alert('Failed to create task. Please try again.');
    }
  };

  const getCurrentDateForTodos = () => {
    if (currentView === 'year') {
      return new Date(currentYear, 0, 1);
    } else if (currentView === 'month') {
      return new Date(currentYear, currentMonth, 1);
    } else {
      return new Date(currentYear, currentMonth, currentDay);
    }
  };

  const handleResetCalendar = async () => {
    if (resetCountdown === null) {
      // Start countdown
      setResetCountdown(5);
      const timerId = setInterval(() => {
        setResetCountdown(prev => {
          if (prev <= 1) {
            clearInterval(timerId);
            executeReset();
            return null;
          }
          return prev - 1;
        });
      }, 1000);
      setResetTimerId(timerId);
    } else {
      // Cancel countdown
      if (resetTimerId) {
        clearInterval(resetTimerId);
      }
      setResetCountdown(null);
      setResetTimerId(null);
    }
  };

  const executeReset = async () => {
    try {
      // Delete all tasks
      const allTasks = await taskService.getAllTasks();
      for (const task of allTasks) {
        try {
          await taskService.deleteTask(task.taskId);
        } catch (err) {
          console.error(`Failed to delete task ${task.taskId}:`, err);
        }
      }

      // Delete all todo lists
      const { todoService } = await import('../../services/todoService');
      const allLists = await todoService.getAllTodoLists();
      for (const list of allLists) {
        try {
          await todoService.deleteTodoList(list.todoListId);
        } catch (err) {
          console.error(`Failed to delete todo list ${list.todoListId}:`, err);
        }
      }

      // Refresh the view
      setRefreshTrigger(prev => prev + 1);
      setResetCountdown(null);
      setResetTimerId(null);
      alert('Calendar reset successfully!');
    } catch (err) {
      console.error('Failed to reset calendar:', err);
      alert('Failed to reset calendar. Please try again.');
      setResetCountdown(null);
      setResetTimerId(null);
    }
  };

  const handleSelectSpectatingCalendar = (calendar) => {
    setSpectatingOwnerId(calendar.ownerId);
    setIsSpectating(true);
    setCurrentView('year');
    // Will need to load tasks for this owner instead of current user
  };

  const handleBackToMyCalendar = () => {
    setIsSpectating(false);
    setSpectatingOwnerId(null);
    setRefreshTrigger(prev => prev + 1);
  };

  // If in spectate mode, show the spectate view selector
  if (!isSpectating && currentView === 'spectate') {
    return (
      <div className="calendar-container">
        <header className="calendar-header">
          <div className="header-content">
            <div className="header-left">
              <h1>👀 Spectate Calendars</h1>
              <p className="welcome-text">View calendars shared with you</p>
            </div>
            <button onClick={() => setCurrentView('year')} className="btn-logout">
              Back to My Calendar
            </button>
          </div>
        </header>
        <div className="calendar-main">
          <SpectateView onSelectCalendar={handleSelectSpectatingCalendar} />
        </div>
      </div>
    );
  }

  return (
    <div className="calendar-container">
      <header className="calendar-header">
        <div className="header-content">
          <div className="header-left">
            <h1>📅 {isSpectating ? 'Spectating Calendar' : 'My Calendar'}</h1>
            <p className="welcome-text">
              {isSpectating ? 'Viewing shared calendar' : `Welcome, ${user?.username}!`}
            </p>
          </div>
          <div style={{ display: 'flex', gap: '10px' }}>
            {isSpectating && (
              <button onClick={handleBackToMyCalendar} className="btn-logout">
                Back to My Calendar
              </button>
            )}
            <button onClick={handleLogout} className="btn-logout">
              Logout
            </button>
          </div>
        </div>
      </header>

      <div className="calendar-main">
        <div className="calendar-toolbar">
          <div className="toolbar-left">
            {!isSpectating && (
              <button
                onClick={handleResetCalendar}
                className={`btn-reset ${resetCountdown !== null ? 'counting' : ''}`}
                title={resetCountdown !== null ? 'Click again to cancel' : 'Reset entire calendar'}
              >
                {resetCountdown !== null ? `Cancel (${resetCountdown}s)` : '🗑️ Reset'}
              </button>
            )}
            <button onClick={previousPeriod} className="btn-nav">
              ← Previous
            </button>
            <button onClick={goToToday} className="btn-today">
              Today
            </button>
            <button onClick={nextPeriod} className="btn-nav">
              Next →
            </button>
            {!isSpectating && (
              <button onClick={() => handleAddTask()} className="btn-add-task">
                + Add Event
              </button>
            )}
            {!isSpectating && (
              <>
                <button onClick={() => setIsShareModalOpen(true)} className="btn-share">
                  🔗 Share
                </button>
                <button onClick={() => setCurrentView('spectate')} className="btn-spectate">
                  👀 Spectate
                </button>
              </>
            )}
          </div>

          <div className="view-switcher">
            <button
              className={`view-btn ${currentView === 'year' ? 'active' : ''}`}
              onClick={() => setCurrentView('year')}
            >
              Year
            </button>
            <button
              className={`view-btn ${currentView === 'month' ? 'active' : ''}`}
              onClick={() => setCurrentView('month')}
            >
              Month
            </button>
            <button
              className={`view-btn ${currentView === 'week' ? 'active' : ''}`}
              onClick={() => setCurrentView('week')}
            >
              Week
            </button>
            <button
              className={`view-btn ${currentView === 'day' ? 'active' : ''}`}
              onClick={() => setCurrentView('day')}
            >
              Day
            </button>
          </div>
        </div>

        <div className="calendar-layout">
          <div className="calendar-content-main">
            {currentView === 'year' && (
              <YearView
                currentYear={currentYear}
                onMonthClick={handleMonthClick}
                tasks={tasks}
              />
            )}

            {currentView === 'month' && (
              <MonthView
                year={currentYear}
                month={currentMonth}
                onDayClick={handleDayClick}
                tasks={tasks}
              />
            )}

            {currentView === 'week' && (
              <WeekView
                year={currentYear}
                month={currentMonth}
                day={currentDay}
                tasks={tasks}
              />
            )}

            {currentView === 'day' && (
              <DayView
                year={currentYear}
                month={currentMonth}
                day={currentDay}
                tasks={tasks}
              />
            )}
          </div>

          {(currentView === 'year' || currentView === 'month' || currentView === 'week') && (
            <div className="calendar-sidebar">
              <TodoListSidebar
                scope={currentView === 'year' ? 'Year' : currentView === 'month' ? 'Month' : 'Week'}
                date={getCurrentDateForTodos()}
                spectateOwnerId={spectatingOwnerId}
                readOnly={isSpectating}
              />
            </div>
          )}
        </div>
      </div>

      <AddTaskModal
        isOpen={isTaskModalOpen}
        onClose={() => setIsTaskModalOpen(false)}
        onSubmit={handleTaskSubmit}
        selectedDate={selectedDate}
        defaultScope={currentView === 'year' ? 'Year' : currentView === 'month' ? 'Month' : 'Day'}
      />

      <ShareModal
        isOpen={isShareModalOpen}
        onClose={() => setIsShareModalOpen(false)}
      />
    </div>
  );
}

export default CalendarView;
