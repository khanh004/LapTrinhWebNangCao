import apiClient from "./client";

export const bookingsApi = {
  getAll: (status) => apiClient.get("/bookings", { params: { status } }),
  getById: (id) => apiClient.get(`/bookings/${id}`),
  getByCustomer: (customerId) => apiClient.get(`/bookings/by-customer/${customerId}`),
  getLogs: (id) => apiClient.get(`/bookings/${id}/logs`),
  create: (data) => apiClient.post("/bookings", data),
  confirm: (id) => apiClient.patch(`/bookings/${id}/confirm`),
  checkIn: (id) => apiClient.patch(`/bookings/${id}/check-in`),
  checkOut: (id, data) => apiClient.patch(`/bookings/${id}/check-out`, data),
  extend: (id, data) => apiClient.patch(`/bookings/${id}/extend`, data),
  cancel: (id) => apiClient.patch(`/bookings/${id}/cancel`),
  noteLateArrival: (id, data) => apiClient.patch(`/bookings/${id}/note-late-arrival`, data),
};