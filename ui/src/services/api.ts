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

export const getInvoices = async () => {
  const response = await api.get('/Invoices');
  return response.data;
};