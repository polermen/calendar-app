import { useState, useEffect } from 'react';
import { shareService } from '../../services/shareService';
import './SpectateView.css';

function SpectateView({ onSelectCalendar }) {
  const [spectatingCalendars, setSpectatingCalendars] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    loadSpectatingCalendars();
  }, []);

  const loadSpectatingCalendars = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await shareService.getSpectatingCalendars();
      setSpectatingCalendars(data);
    } catch (err) {
      setError('Failed to load calendars. Please try again.');
      console.error('Error loading spectating calendars:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCalendarClick = (calendar) => {
    if (onSelectCalendar) {
      onSelectCalendar(calendar);
    }
  };

  if (loading) {
    return (
      <div className="spectate-container">
        <div className="spectate-header">
          <h2>Calendars Shared With You</h2>
        </div>
        <div className="spectate-loading">
          <div className="spectate-loading-spinner"></div>
          <p>Loading calendars...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="spectate-container">
        <div className="spectate-header">
          <h2>Calendars Shared With You</h2>
        </div>
        <div className="spectate-error">
          <p>{error}</p>
          <button className="spectate-btn-retry" onClick={loadSpectatingCalendars}>
            Try Again
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="spectate-container">
      <div className="spectate-header">
        <h2>Calendars Shared With You</h2>
        <p className="spectate-subtitle">
          View calendars that others have shared with you
        </p>
      </div>

      {spectatingCalendars.length === 0 ? (
        <div className="spectate-empty-state">
          <div className="spectate-empty-icon">📅</div>
          <h3>No Shared Calendars</h3>
          <p>You don't have access to any shared calendars yet.</p>
          <p className="spectate-empty-hint">
            When someone shares their calendar with you, it will appear here.
          </p>
        </div>
      ) : (
        <div className="spectate-grid">
          {spectatingCalendars.map((calendar) => (
            <div
              key={calendar.ownerId}
              className="spectate-card"
              onClick={() => handleCalendarClick(calendar)}
            >
              <div className="spectate-card-header">
                <div className="spectate-card-icon">👤</div>
              </div>
              <div className="spectate-card-body">
                <h3 className="spectate-card-username">
                  {calendar.ownerUsername || 'Unknown User'}
                </h3>
                <p className="spectate-card-email">
                  {calendar.ownerEmail || 'No email'}
                </p>
              </div>
              <div className="spectate-card-footer">
                <span className="spectate-view-label">Click to view</span>
                <span className="spectate-arrow">→</span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default SpectateView;
