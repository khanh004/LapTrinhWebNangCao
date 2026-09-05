import apiClient from "./client";

export const roomsApi = {
  getAll: (params) => apiClient.get("/rooms", { params }),
  getById: (id) => apiClient.get(`/rooms/${id}`),
  checkAvailability: (id, checkIn, checkOut) =>
    apiClient.get(`/rooms/${id}/check-availability`, {
      params: { checkIn, checkOut },
    }),
  create: (data) => apiClient.post("/rooms", data),
  update: (id, data) => apiClient.put(`/rooms/${id}`, data),
  updateStatus: (id, status) => apiClient.patch(`/rooms/${id}/status`, { status }),
  confirmCleaning: (id) => apiClient.patch(`/rooms/${id}/confirm-cleaning`),
  delete: (id) => apiClient.delete(`/rooms/${id}`),
};

export const roomTypesApi = {
  getAll: () => apiClient.get("/roomtypes"),
  getById: (id) => apiClient.get(`/roomtypes/${id}`),
  create: (data) => apiClient.post("/roomtypes", data),
  update: (id, data) => apiClient.put(`/roomtypes/${id}`, data),
  delete: (id) => apiClient.delete(`/roomtypes/${id}`),
};