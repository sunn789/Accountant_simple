
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import InvoiceTemplate from '../components/Invoice/InvoiceTemplate';
import { getInvoiceById } from '../services/api';

interface InvoiceDetailData {
  id: number;
  invoiceNumber: string;
  date: string;
  seller: {
    name: string;
    economicCode?: string;
    registrationNumber?: string;
    nationalId?: string;
    province?: string;
    city?: string;
    address?: string;
    postalCode?: string;
    phone?: string;
    mobile?: string;
  };
  buyer: {
    name: string;
    economicCode?: string;
    registrationNumber?: string;
    nationalId?: string;
    province?: string;
    city?: string;
    address?: string;
    postalCode?: string;
    phone?: string;
    mobile?: string;
  };
  items: {
    id?: number;
    description: string;
    quantity: number;
    unitPrice: number;
    discount: number;
    tax: number;
    totalPrice?: number;
  }[];
  totalAmount: number;
  paymentType: string;
  notes?: string;
}

const formatToJalali = (dateString: string) => {
  if (!dateString) return '-';

  try {
    const datePart = dateString.split('T')[0];
    const [year, month, day] = datePart.split('-').map(Number);

    if (!year || !month || !day) return '-';

    const date = new Date(Date.UTC(year, month - 1, day));

    return new Intl.DateTimeFormat('fa-IR-u-ca-persian', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      timeZone: 'UTC',
    }).format(date);
  } catch {
    return '-';
  }
};

const InvoiceDetail = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [invoice, setInvoice] = useState<InvoiceDetailData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let isMounted = true;

    const loadInvoice = async () => {
      const invoiceId = Number(id);

      if (!id || !Number.isInteger(invoiceId) || invoiceId <= 0) {
        setError('شناسه فاکتور معتبر نیست.');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError('');

        const data = await getInvoiceById(invoiceId);

        if (isMounted) {
          setInvoice({
            ...data,
            date: formatToJalali(data.date),
          });
        }
      } catch (err: any) {
        if (!isMounted) return;

        if (err.response?.status === 404) {
          setError('فاکتور مورد نظر پیدا نشد.');
        } else {
          setError(
            err.response?.data?.message ||
            'دریافت اطلاعات فاکتور با خطا مواجه شد.'
          );
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    loadInvoice();

    return () => {
      isMounted = false;
    };
  }, [id]);

  if (loading) {
    return (
      <div className="invoice-detail-state">
        <div className="invoice-detail-spinner" />
        <span>در حال دریافت اطلاعات فاکتور...</span>
      </div>
    );
  }

  if (error || !invoice) {
    return (
      <div className="invoice-detail-state">
        <div className="invoice-detail-error">
          <span className="invoice-detail-error-icon">!</span>
          <h3>امکان نمایش فاکتور وجود ندارد</h3>
          <p>{error || 'اطلاعات فاکتور در دسترس نیست.'}</p>

          <button
            className="invoice-detail-back"
            onClick={() => navigate('/invoices')}
          >
            بازگشت به لیست فاکتورها
          </button>
        </div>
      </div>
    );
  }

  return (
    <main className="invoice-detail-page">
      <header className="invoice-detail-header no-print">
        <div className="invoice-detail-heading">
          <button
            className="invoice-detail-back"
            onClick={() => navigate('/invoices')}
          >
            <span>→</span>
            بازگشت به فاکتورها
          </button>

          <div>
            <h1>جزئیات فاکتور</h1>
            <p>
              شماره فاکتور: <strong>{invoice.invoiceNumber || invoice.id}</strong>
            </p>
          </div>
        </div>

        <button
          className="invoice-detail-print"
          onClick={() => window.print()}
        >
          <span>▤</span>
          چاپ فاکتور
        </button>
      </header>

      <section className="invoice-detail-document">
        <InvoiceTemplate invoice={invoice as any} />
      </section>
    </main>
  );
};

export default InvoiceDetail;
