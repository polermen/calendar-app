import api from './api';

export const taskService = {
  async getAllTasks(startDate, endDate, spectateOwnerId = null) {
    if (spectateOwnerId) {
      const response = await api.get(`/tasks/spectate/${spectateOwnerId}`);
      return response.data;
    }

    const params = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;

    const response = await api.get('/tasks', { params });
    return response.data;
  },

  async getTaskById(id) {
    const response = await api.get(`/tasks/${id}`);
    return response.data;
  },

  async createTask(taskData) {
    const response = await api.post('/tasks', taskData);
    return response.data;
  },

  async deleteTask(id) {
    const response = await api.delete(`/tasks/${id}`);
    return response.data;
  },

  async markTaskComplete(id) {
    const response = await api.patch(`/tasks/${id}/complete`);
    return response.data;
  }
};
