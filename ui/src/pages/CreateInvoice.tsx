// src/pages/CreateInvoice.tsx
import React from 'react';
import InvoiceForm from '../components/Invoice/InvoiceForm'; // کامپوننت فرم شما

const CreateInvoice = () => {
  return (
    <div className="page-container" dir="rtl">
      <h2>صدور فاکتور جدید</h2>
      <InvoiceForm />
    </div>
  );
};

export default CreateInvoice;