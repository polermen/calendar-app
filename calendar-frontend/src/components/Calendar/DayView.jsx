import './DayView.css';

function DayView({ year, month, day, tasks = [] }) {
  const monthNames = ['January', 'February', 'March', 'April', 'May', 'June',
                      'July', 'August', 'September', 'October', 'November', 'December'];
  const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

  const currentDate = new Date(year, month, day);
  const dayOfWeek = dayNames[currentDate.getDay()];

  const today = new Date();
  const isToday = currentDate.toDateString() === today.toDateString();

  const hours = Array.from({ length: 24 }, (_, i) => i);

  const formatHour = (hour) => {
    if (hour === 0) return '12 AM';
    if (hour === 12) return '12 PM';
    if (hour < 12) return `${hour} AM`;
    return `${hour - 12} PM`;
  };

  const getTasksForDay = () => {
    return tasks.filter(task => {
      const taskDate = new Date(task.taskDate || task.startDate);
      return taskDate.toDateString() === currentDate.toDateString();
    }).sort((a, b) => {
      const timeA = new Date(a.startDate).getTime();
      const timeB = new Date(b.startDate).getTime();
      return timeA - timeB;
    });
  };

  const getTasksForHour = (hour) => {
    const dayTasks = getTasksForDay();
    return dayTasks.filter(task => {
      const startDate = new Date(task.startDate);
      return startDate.getHours() === hour;
    });
  };

  const formatTime = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit', hour12: true });
  };

  return (
    <div className="day-view">
      <div className="day-view-header">
        <div className={`date-display ${isToday ? 'is-today' : ''}`}>
          <div className="day-name">{dayOfWeek}</div>
          <div className="day-number">{day}</div>
          <div className="month-year">{monthNames[month]} {year}</div>
        </div>
      </div>

      <div className="day-schedule">
        <div className="schedule-header">
          <h3>Daily Schedule</h3>
          {isToday && <span className="today-badge">Today</span>}
        </div>

        <div className="time-grid">
          {hours.map(hour => {
            const hourTasks = getTasksForHour(hour);

            return (
              <div key={hour} className="time-slot">
                <div className="time-label">{formatHour(hour)}</div>
                <div className="time-content">
                  {hourTasks.length > 0 ? (
                    hourTasks.map(task => (
                      <div key={task.taskId} className="day-event">
                        <div className="event-time-range">
                          {formatTime(task.startDate)} - {formatTime(task.endDate)}
                        </div>
                        <div className="event-title-day">{task.title}</div>
                        {task.description && (
                          <div className="event-description">{task.description}</div>
                        )}
                      </div>
                    ))
                  ) : null}
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}

export default DayView;
