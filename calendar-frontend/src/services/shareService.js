import api from './api';

export const shareService = {
  async getMyShares() {
    const response = await api.get('/share/my-shares');
    return response.data;
  },

  async getSpectatingCalendars() {
    const response = await api.get('/share/spectating');
    return response.data;
  },

  async createShare(spectatorEmail) {
    const response = await api.post('/share', { spectatorEmail });
    return response.data;
  },

  async deleteShare(shareId) {
    const response = await api.delete(`/share/${shareId}`);
    return response.data;
  }
};
