import apiClient from "./client";

export const customersApi = {
  getAll: (search) => apiClient.get("/customers", { params: { search } }),
  search: (search) => apiClient.get("/customers", { params: { search } }),
  getById: (id) => apiClient.get(`/customers/${id}`),
  create: (data) => apiClient.post("/customers", data),
  update: (id, data) => apiClient.put(`/customers/${id}`, data),
  delete: (id) => apiClient.delete(`/customers/${id}`),
};