import api from './api';

export const todoService = {
  async getAllTodoLists(scope, date, spectateOwnerId = null) {
    if (spectateOwnerId) {
      const response = await api.get(`/todos/spectate/${spectateOwnerId}/lists`);
      return response.data;
    }

    const params = {};
    if (scope) params.scope = scope;
    if (date) params.date = date;

    const response = await api.get('/todos/lists', { params });
    return response.data;
  },

  async getTodoListById(id) {
    const response = await api.get(`/todos/lists/${id}`);
    return response.data;
  },

  async createTodoList(todoData) {
    const response = await api.post('/todos/lists', todoData);
    return response.data;
  },

  async deleteTodoList(id) {
    const response = await api.delete(`/todos/lists/${id}`);
    return response.data;
  },

  async addTodoItem(listId, itemData) {
    const response = await api.post(`/todos/lists/${listId}/items`, itemData);
    return response.data;
  },

  async deleteTodoItem(itemId) {
    const response = await api.delete(`/todos/items/${itemId}`);
    return response.data;
  },

  async toggleTodoItem(itemId) {
    const response = await api.patch(`/todos/items/${itemId}/toggle`);
    return response.data;
  }
};
