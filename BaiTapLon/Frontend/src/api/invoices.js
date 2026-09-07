import apiClient from "./client";

export const invoicesApi = {
  getAll: (paymentStatus) => apiClient.get("/invoices", { params: { paymentStatus } }),
  getById: (id) => apiClient.get(`/invoices/${id}`),
  markPaid: (id) => apiClient.patch(`/invoices/${id}/mark-paid`),
};