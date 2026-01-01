import { useState } from 'react';
import './AddTaskModal.css';

function AddTaskModal({ isOpen, onClose, onSubmit, selectedDate }) {
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    taskDate: selectedDate ? selectedDate.toISOString().split('T')[0] : '',
    startTime: '09:00',
    endTime: '10:00'
  });

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    // Combine date and time into datetime strings
    const eventData = {
      title: formData.title,
      description: formData.description,
      taskDate: formData.taskDate,
      startDate: `${formData.taskDate}T${formData.startTime}:00`,
      endDate: `${formData.taskDate}T${formData.endTime}:00`,
      scope: 'Day' // Always Day scope for events
    };

    await onSubmit(eventData);
    setFormData({
      title: '',
      description: '',
      taskDate: '',
      startTime: '09:00',
      endTime: '10:00'
    });
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Add New Event</h2>
          <button className="close-btn" onClick={onClose}>×</button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="title">Event Title *</label>
            <input
              type="text"
              id="title"
              name="title"
              value={formData.title}
              onChange={handleChange}
              required
              placeholder="Enter event title"
            />
          </div>

          <div className="form-group">
            <label htmlFor="description">Description</label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              placeholder="Enter event description (optional)"
              rows="3"
            />
          </div>

          <div className="form-group">
            <label htmlFor="taskDate">Event Date *</label>
            <input
              type="date"
              id="taskDate"
              name="taskDate"
              value={formData.taskDate}
              onChange={handleChange}
              required
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="startTime">Start Time *</label>
              <input
                type="time"
                id="startTime"
                name="startTime"
                value={formData.startTime}
                onChange={handleChange}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="endTime">End Time *</label>
              <input
                type="time"
                id="endTime"
                name="endTime"
                value={formData.endTime}
                onChange={handleChange}
                required
              />
            </div>
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-cancel" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn-submit">
              Add Event
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default AddTaskModal;
