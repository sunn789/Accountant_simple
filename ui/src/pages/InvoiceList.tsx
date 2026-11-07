import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Invoice } from '../types/invoice';
import { getInvoices } from '../services/api';

const InvoiceList = () => {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    getInvoices()
      .then((data) => { setInvoices(data); setLoading(false); })
      .catch(() => setLoading(false));
  }, []);

  if (loading) return <div className="page-container">در حال بارگذاری...</div>;

  return (
    <div className="page-container">
      <div className="list-header">
        <h2>لیست فاکتورهای صادر شده</h2>
        <button className="btn-primary" onClick={() => navigate('/invoices/new')}>
          + صدور فاکتور جدید
        </button>
      </div>

      <table className="data-table">
        <thead>
          <tr>
            <th>شماره فاکتور</th>
            <th>تاریخ</th>
            <th>خریدار</th>
            <th>مبلغ کل (ریال)</th>
            <th>نوع پرداخت</th>
            <th>عملیات</th>
          </tr>
        </thead>
        <tbody>
          {invoices.length === 0 ? (
            <tr><td colSpan={6} style={{ textAlign: 'center', padding: '30px' }}>هیچ فاکتوری یافت نشد.</td></tr>
          ) : (
            invoices.map((inv) => (
              <tr key={inv.id} onClick={() => navigate(`/invoices/${inv.id}`)}>
                <td>{inv.invoiceNumber}</td>
                <td>{inv.date}</td>
                <td>{inv.buyer?.name}</td>
                <td>{inv.totalAmount?.toLocaleString('fa-IR')}</td>
                <td>{inv.paymentType === 'cash' ? 'نقدی' : 'غیر نقدی'}</td>
                <td><span style={{ color: '#3b82f6' }}>مشاهده</span></td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
};

export default InvoiceList;