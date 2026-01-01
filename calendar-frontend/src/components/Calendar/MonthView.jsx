import './MonthView.css';

function MonthView({ year, month, onDayClick, tasks = [] }) {
  const monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

  const getDaysInMonth = () => {
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingDayOfWeek = firstDay.getDay();

    const days = [];

    for (let i = 0; i < startingDayOfWeek; i++) {
      days.push(null);
    }

    for (let day = 1; day <= daysInMonth; day++) {
      days.push(day);
    }

    return days;
  };

  const today = new Date();
  const isToday = (day) => {
    return today.getFullYear() === year &&
           today.getMonth() === month &&
           today.getDate() === day;
  };

  const getTasksForDay = (day) => {
    return tasks.filter(task => {
      const taskDate = new Date(task.taskDate || task.startDate);
      return taskDate.getFullYear() === year &&
             taskDate.getMonth() === month &&
             taskDate.getDate() === day;
    });
  };

  const days = getDaysInMonth();

  return (
    <div className="month-view">
      <div className="month-view-header">
        <h2>{monthNames[month]} {year}</h2>
      </div>

      <div className="month-calendar">
        <div className="month-day-headers">
          {dayNames.map(day => (
            <div key={day} className="month-day-header">{day}</div>
          ))}
        </div>

        <div className="month-days">
          {days.map((day, index) => {
            const dayTasks = day ? getTasksForDay(day) : [];

            return (
              <div
                key={index}
                className={`month-day ${!day ? 'empty' : ''} ${day && isToday(day) ? 'today' : ''}`}
                onClick={() => day && onDayClick(year, month, day)}
              >
                {day && (
                  <>
                    <div className="day-number">{day}</div>
                    <div className="day-content">
                      {dayTasks.slice(0, 3).map(task => (
                        <div
                          key={task.taskId}
                          className={`task-indicator ${task.isCompleted ? 'completed' : ''}`}
                          title={task.title}
                        >
                          {task.title}
                        </div>
                      ))}
                      {dayTasks.length > 3 && (
                        <div className="more-tasks">+{dayTasks.length - 3} more</div>
                      )}
                    </div>
                  </>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}

export default MonthView;
