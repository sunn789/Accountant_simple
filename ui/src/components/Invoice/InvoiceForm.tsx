// src/components/Invoice/InvoiceForm.tsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Invoice, InvoiceItem, Party } from '../../types/invoice';
import { saveInvoice } from '../../services/api';

const InvoiceForm = () => {
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);

  // ۱. استیت اطلاعات پایه
  const [invoiceNumber, setInvoiceNumber] = useState('');
 const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const [paymentType, setPaymentType] = useState('cash');
  const [notes, setNotes] = useState('');

  // ۲. استیت اطلاعات خریدار
  const [buyer, setBuyer] = useState<Party>({
    name: '', economicCode: '', registrationNumber: '', nationalId: '',
    province: '', city: '', address: '', postalCode: '', phone: '', mobile: ''
  });

  // ۳. استیت اقلام فاکتور
  const [items, setItems] = useState<InvoiceItem[]>([
    { description: '', quantity: 1, unitPrice: 0, discount: 0, tax: 0 }
  ]);

  // هندلر تغییر فیلدهای خریدار
  const handleBuyerChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setBuyer(prev => ({ ...prev, [name]: value }));
  };

  // هندلر تغییر فیلدهای آیتم‌ها
  const handleItemChange = (index: number, field: keyof InvoiceItem, value: string | number) => {
    const newItems = [...items];
    newItems[index] = { ...newItems[index], [field]: value };
    setItems(newItems);
  };

  // افزودن ردیف جدید
  const addItem = () => {
    setItems([...items, { description: '', quantity: 1, unitPrice: 0, discount: 0, tax: 0 }]);
  };

  // حذف ردیف
  const removeItem = (index: number) => {
    if (items.length > 1) {
      setItems(items.filter((_, i) => i !== index));
    }
  };

  // محاسبه جمع کل یک ردیف
  const calculateRowTotal = (item: InvoiceItem) => {
    return (item.quantity * item.unitPrice) - item.discount + item.tax;
  };

  // محاسبه جمع کل فاکتور
  const calculateGrandTotal = () => {
    return items.reduce((total, item) => total + calculateRowTotal(item), 0);
  };

  // ارسال به بک‌اند
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    // فروشنده فیک (در نسخه واقعی باید از تنظیمات یا دیتابیس خوانده شود)
    const seller: Party = {
      name: 'شرکت من', economicCode: '123456789', registrationNumber: '111',
      nationalId: '1234567890', province: 'تهران', city: 'تهران',
      address: 'آدرس شرکت', postalCode: '1234567890', phone: '021-00000000', mobile: '09120000000'
    };

    const newInvoice: Invoice = {
      invoiceNumber: invoiceNumber || `INV-${Date.now()}`,
      date,
      seller,
      buyer,
      items: items.map(item => ({ ...item, totalPrice: calculateRowTotal(item) })),
      totalAmount: calculateGrandTotal(),
      paymentType,
      notes
    };

    try {
      await saveInvoice(newInvoice);
      alert('فاکتور با موفقیت ذخیره شد!');
      navigate('/invoices'); // هدایت به لیست فاکتورها
    } catch (error) {
      console.error(error);
      alert('خطا در ذخیره فاکتور. لطفا دوباره تلاش کنید.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="invoice-form" onSubmit={handleSubmit} dir="rtl">
      
      {/* بخش اطلاعات پایه */}
      <div className="form-row">
        <div className="form-group">
          <label>شماره فاکتور</label>
          <input type="text" value={invoiceNumber} onChange={(e) => setInvoiceNumber(e.target.value)} placeholder="مثلا 1001" />
        </div>
        <div className="form-group">
          <label>تاریخ</label>
          <input type="text" value={date} onChange={(e) => setDate(e.target.value)} />
        </div>
      </div>

      {/* بخش اطلاعات خریدار */}
      <fieldset className="form-section">
        <legend>مشخصات خریدار</legend>
        <div className="form-row">
          <div className="form-group">
            <label>نام خریدار *</label>
            <input required type="text" name="name" value={buyer.name} onChange={handleBuyerChange} />
          </div>
          <div className="form-group">
            <label>شماره اقتصادی</label>
            <input type="text" name="economicCode" value={buyer.economicCode} onChange={handleBuyerChange} />
          </div>
          <div className="form-group">
            <label>شناسه ملی</label>
            <input type="text" name="nationalId" value={buyer.nationalId} onChange={handleBuyerChange} />
          </div>
        </div>
        <div className="form-row">
          <div className="form-group">
            <label>استان</label>
            <input type="text" name="province" value={buyer.province} onChange={handleBuyerChange} />
          </div>
          <div className="form-group">
            <label>شهر</label>
            <input type="text" name="city" value={buyer.city} onChange={handleBuyerChange} />
          </div>
          <div className="form-group">
            <label>تلفن</label>
            <input type="text" name="phone" value={buyer.phone} onChange={handleBuyerChange} />
          </div>
        </div>
        <div className="form-group">
          <label>نشانی کامل</label>
          <input type="text" name="address" value={buyer.address} onChange={handleBuyerChange} />
        </div>
      </fieldset>

      {/* بخش اقلام فاکتور */}
      <fieldset className="form-section">
        <legend>اقلام فاکتور</legend>
        <table className="form-table">
          <thead>
            <tr>
              <th>شرح کالا/خدمات</th>
              <th style={{ width: '80px' }}>تعداد</th>
              <th style={{ width: '120px' }}>مبلغ واحد</th>
              <th style={{ width: '100px' }}>تخفیف</th>
              <th style={{ width: '100px' }}>مالیات</th>
              <th style={{ width: '120px' }}>جمع ردیف</th>
              <th style={{ width: '50px' }}>حذف</th>
            </tr>
          </thead>
          <tbody>
            {items.map((item, index) => (
              <tr key={index}>
                <td><input type="text" value={item.description} onChange={(e) => handleItemChange(index, 'description', e.target.value)} required /></td>
                <td><input type="number" min="1" value={item.quantity} onChange={(e) => handleItemChange(index, 'quantity', Number(e.target.value))} required /></td>
                <td><input type="number" min="0" value={item.unitPrice} onChange={(e) => handleItemChange(index, 'unitPrice', Number(e.target.value))} required /></td>
                <td><input type="number" min="0" value={item.discount} onChange={(e) => handleItemChange(index, 'discount', Number(e.target.value))} /></td>
                <td><input type="number" min="0" value={item.tax} onChange={(e) => handleItemChange(index, 'tax', Number(e.target.value))} /></td>
                <td className="row-total">{calculateRowTotal(item).toLocaleString('fa-IR')}</td>
                <td>
                  <button type="button" className="btn-danger" onClick={() => removeItem(index)}>✕</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        <button type="button" className="btn-secondary" onClick={addItem} style={{ marginTop: '10px' }}>
          + افزودن ردیف
        </button>
      </fieldset>

      {/* بخش پرداخت و توضیحات */}
      <div className="form-row" style={{ marginTop: '20px' }}>
        <div className="form-group">
          <label>نحوه پرداخت</label>
          <select value={paymentType} onChange={(e) => setPaymentType(e.target.value)}>
            <option value="cash">نقدی</option>
            <option value="credit">غیر نقدی</option>
          </select>
        </div>
        <div className="form-group" style={{ flex: 2 }}>
          <label>توضیحات</label>
          <input type="text" value={notes} onChange={(e) => setNotes(e.target.value)} />
        </div>
      </div>

      {/* جمع کل و ثبت */}
      <div className="form-footer">
        <div className="grand-total">
          جمع کل قابل پرداخت: <strong>{calculateGrandTotal().toLocaleString('fa-IR')} ریال</strong>
        </div>
        <button type="submit" className="btn-primary btn-large" disabled={isSubmitting}>
          {isSubmitting ? 'در حال ذخیره...' : 'ثبت و ذخیره فاکتور'}
        </button>
      </div>
    </form>
  );
};

export default InvoiceForm;