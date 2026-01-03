import { useState, useEffect } from 'react';
import { shareService } from '../../services/shareService';
import './ShareModal.css';

function ShareModal({ isOpen, onClose }) {
  const [spectatorEmail, setSpectatorEmail] = useState('');
  const [shares, setShares] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [loadingShares, setLoadingShares] = useState(false);

  useEffect(() => {
    if (isOpen) {
      loadShares();
    }
  }, [isOpen]);

  const loadShares = async () => {
    setLoadingShares(true);
    setError('');
    try {
      const data = await shareService.getMyShares();
      setShares(data);
    } catch (err) {
      setError('Failed to load shares. Please try again.');
      console.error('Error loading shares:', err);
    } finally {
      setLoadingShares(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!spectatorEmail.trim()) {
      setError('Please enter an email address');
      return;
    }

    setLoading(true);
    setError('');

    try {
      await shareService.createShare(spectatorEmail.trim());
      setSpectatorEmail('');
      await loadShares();
    } catch (err) {
      if (err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Failed to create share. Please try again.');
      }
      console.error('Error creating share:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (shareId) => {
    if (!window.confirm('Are you sure you want to remove this share?')) {
      return;
    }

    setError('');
    try {
      await shareService.deleteShare(shareId);
      await loadShares();
    } catch (err) {
      setError('Failed to delete share. Please try again.');
      console.error('Error deleting share:', err);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="share-modal-overlay" onClick={onClose}>
      <div className="share-modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="share-modal-header">
          <h2>Share Your Calendar</h2>
          <button className="share-close-btn" onClick={onClose}>×</button>
        </div>

        <div className="share-modal-body">
          {error && (
            <div className="share-error-message">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="share-form">
            <div className="share-form-group">
              <label htmlFor="spectatorEmail">Spectator Email</label>
              <div className="share-input-group">
                <input
                  type="email"
                  id="spectatorEmail"
                  value={spectatorEmail}
                  onChange={(e) => setSpectatorEmail(e.target.value)}
                  placeholder="Enter email address"
                  disabled={loading}
                />
                <button
                  type="submit"
                  className="share-btn-add"
                  disabled={loading || !spectatorEmail.trim()}
                >
                  {loading ? 'Sharing...' : 'Share'}
                </button>
              </div>
            </div>
          </form>

          <div className="share-list-section">
            <h3>People with Access</h3>

            {loadingShares ? (
              <div className="share-loading">Loading shares...</div>
            ) : shares.length === 0 ? (
              <div className="share-empty-state">
                <p>You haven't shared your calendar with anyone yet.</p>
                <p className="share-empty-hint">Enter an email above to start sharing!</p>
              </div>
            ) : (
              <div className="share-list">
                {shares.map((share) => (
                  <div key={share.calendarShareId} className="share-item">
                    <div className="share-item-info">
                      <div className="share-item-email">
                        {share.spectatorEmail || 'Unknown User'}
                      </div>
                      <div className="share-item-username">
                        @{share.spectatorUsername || 'unknown'}
                      </div>
                    </div>
                    <button
                      className="share-btn-delete"
                      onClick={() => handleDelete(share.calendarShareId)}
                      title="Remove access"
                    >
                      Remove
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

export default ShareModal;
