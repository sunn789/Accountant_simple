import { useParams, useNavigate } from 'react-router-dom';
import InvoiceTemplate from '../components/Invoice/InvoiceTemplate';

const InvoiceDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  // (در اینجا باید اطلاعات فاکتور را با آیدی id از API بگیرید)
  const dummyInvoice = {
    invoiceNumber: id, date: '1403/08/15',
    seller: { name: 'شرکت من', economicCode: '123', registrationNumber: '1', nationalId: '1', province: 'تهران', city: 'تهران', address: 'آدرس', postalCode: '1', phone: '1', mobile: '1' },
    buyer: { name: 'خریدار', economicCode: '123', registrationNumber: '1', nationalId: '1', province: 'تهران', city: 'تهران', address: 'آدرس', postalCode: '1', phone: '1', mobile: '1' },
    items: [{ description: 'کالا', quantity: 1, unitPrice: 1000, discount: 0, tax: 0 }],
    totalAmount: 1000, paymentType: 'cash', notes: ''
  };

  return (
    <div>
      <div className="no-print" style={{ maxWidth: '1200px', margin: '0 auto', padding: '20px' }}>
        <button className="btn-secondary" onClick={() => navigate('/invoices')} style={{ padding: '8px 16px', borderRadius: '4px', cursor: 'pointer' }}>
          &rarr; بازگشت به لیست
        </button>
      </div>
      <div className="invoice-print-wrapper">
        <InvoiceTemplate invoice={dummyInvoice as any} />
      </div>
    </div>
  );
};

export default InvoiceDetail;

