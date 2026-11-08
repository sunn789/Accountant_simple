// src/services/api.ts
import axios from 'axios';
import type { Invoice } from '../types/invoice';

const api = axios.create({
  baseURL: '/api', 
});

export const saveInvoice = async (invoice: Invoice) => {
  // آدرس نهایی می‌شود /api/Invoices
  const response = await api.post('/Invoices', invoice); 
  return response.data;
};

export interface PagedResult {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  items: Invoice[];
}

export interface TodaySummary {
  date: string;
  jalaliDate: string;
  invoiceCount: number;
  totalAmount: number;
}

export const getTodaySummary = async (): Promise<TodaySummary> => {
  const response = await api.get('/Invoices/today-summary');
  return response.data;
};

export const getInvoices = async (
  page: number = 1,
  pageSize: number = 10,
  startDate?: string,
  endDate?: string,
  buyerName?: string,
  minAmount?: number,
  maxAmount?: number
): Promise<PagedResult> => {
  const params: Record<string, string | number> = {
    page,
    pageSize,
  };

  if (startDate) params.startDate = startDate;
  if (endDate) params.endDate = endDate;
  if (buyerName) params.buyerName = buyerName;
  if (minAmount !== undefined) params.minAmount = minAmount;
  if (maxAmount !== undefined) params.maxAmount = maxAmount;

  const response = await api.get('/Invoices', { params });

  return response.data;
};


export interface DashboardStats {
  totalInvoices: number;
  totalRevenue: number;
  todayInvoices: number;
  todayRevenue: number;
  totalCustomers: number;
}

export const getDashboardStats = async (): Promise<DashboardStats> => {
  const response = await api.get('/Dashboard/stats');
  return response.data;
};


export const getInvoiceById = async (id: number) => {
  const response = await api.get(`/Invoices/${id}`);
  return response.data;
};
