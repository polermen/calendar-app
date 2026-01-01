import './WeekView.css';

function WeekView({ year, month, day, tasks = [] }) {
  const getWeekDays = () => {
    const currentDate = new Date(year, month, day);
    const currentDayOfWeek = currentDate.getDay();

    const weekDays = [];

    for (let i = 0; i < 7; i++) {
      const date = new Date(currentDate);
      date.setDate(currentDate.getDate() - currentDayOfWeek + i);
      weekDays.push(date);
    }

    return weekDays;
  };

  const getTasksForDay = (date) => {
    return tasks.filter(task => {
      const taskDate = new Date(task.taskDate || task.startDate);
      return taskDate.toDateString() === date.toDateString();
    }).sort((a, b) => {
      const timeA = new Date(a.startDate).getTime();
      const timeB = new Date(b.startDate).getTime();
      return timeA - timeB;
    });
  };

  const formatTime = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit', hour12: true });
  };

  const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
  const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

  const weekDays = getWeekDays();
  const today = new Date();

  const isToday = (date) => {
    return date.toDateString() === today.toDateString();
  };

  const startDate = weekDays[0];
  const endDate = weekDays[6];

  return (
    <div className="week-view">
      <div className="week-view-header">
        <h2>
          {monthNames[startDate.getMonth()]} {startDate.getDate()} - {monthNames[endDate.getMonth()]} {endDate.getDate()}, {year}
        </h2>
      </div>

      <div className="week-grid">
        {weekDays.map((date, index) => {
          const dayTasks = getTasksForDay(date);

          return (
            <div key={index} className={`week-day ${isToday(date) ? 'today' : ''}`}>
              <div className="week-day-header">
                <div className="day-name">{dayNames[date.getDay()]}</div>
                <div className={`day-number ${isToday(date) ? 'today-number' : ''}`}>
                  {date.getDate()}
                </div>
              </div>
              <div className="week-day-content">
                <div className="time-slots">
                  {dayTasks.length === 0 ? (
                    <p className="placeholder">No events</p>
                  ) : (
                    dayTasks.map(task => (
                      <div key={task.taskId} className="week-event">
                        <div className="event-time">
                          {formatTime(task.startDate)} - {formatTime(task.endDate)}
                        </div>
                        <div className="event-title">{task.title}</div>
                      </div>
                    ))
                  )}
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export default WeekView;
