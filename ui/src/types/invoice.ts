// src/types/invoice.ts

export interface Party {
  id?: number;
  name: string;
  economicCode: string;
  registrationNumber: string;
  nationalId: string;
  province: string;
  city: string;
  address: string;
  postalCode: string;
  phone: string;
  mobile: string;
}

export interface InvoiceItem {
  id?: number;
  description: string;
  quantity: number;
  unitPrice: number;
  discount: number;
  tax: number;
  totalPrice?: number;
}

export interface Invoice {
  id?: number;
  invoiceNumber: string;
  date: string;
  seller: Party;
  buyer: Party;
  items: InvoiceItem[];
  totalAmount?: number;
  paymentType: string; // 'cash' | 'credit'
  notes: string;
}