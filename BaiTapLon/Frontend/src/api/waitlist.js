import apiClient from "./client";

export const waitlistApi = {
  getByRoom: (roomId) => apiClient.get(`/waitlist/room/${roomId}`),
  join: (data) => apiClient.post("/waitlist", data),
  convert: (id) => apiClient.post(`/waitlist/${id}/convert`),
  cancel: (id) => apiClient.delete(`/waitlist/${id}`),
};