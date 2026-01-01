import { useState } from 'react';
import './YearView.css';

function YearView({ currentYear, onMonthClick, onViewChange, tasks = [] }) {
  const monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  const dayNames = ['S', 'M', 'T', 'W', 'T', 'F', 'S'];

  const getDaysInMonth = (year, month) => {
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
  const isToday = (year, month, day) => {
    return today.getFullYear() === year &&
           today.getMonth() === month &&
           today.getDate() === day;
  };

  const getMonthTaskCount = (year, month) => {
    return tasks.filter(task => {
      const taskDate = new Date(task.taskDate || task.startDate);
      return taskDate.getFullYear() === year && taskDate.getMonth() === month;
    }).length;
  };

  return (
    <div className="year-view">
      <div className="year-header">
        <h2>{currentYear}</h2>
      </div>

      <div className="year-grid">
        {monthNames.map((monthName, monthIndex) => {
          const days = getDaysInMonth(currentYear, monthIndex);
          const taskCount = getMonthTaskCount(currentYear, monthIndex);

          return (
            <div
              key={monthIndex}
              className="mini-month"
              onClick={() => onMonthClick(currentYear, monthIndex)}
            >
              <div className="mini-month-header">
                <h3>{monthName}</h3>
                {taskCount > 0 && (
                  <span className="task-badge">{taskCount}</span>
                )}
              </div>

              <div className="mini-calendar">
                <div className="mini-day-headers">
                  {dayNames.map(day => (
                    <div key={day} className="mini-day-header">{day}</div>
                  ))}
                </div>

                <div className="mini-days">
                  {days.map((day, index) => (
                    <div
                      key={index}
                      className={`mini-day ${!day ? 'empty' : ''} ${
                        day && isToday(currentYear, monthIndex, day) ? 'today' : ''
                      }`}
                    >
                      {day || ''}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export default YearView;
