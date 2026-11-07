// src/components/Invoice/InvoiceTemplate.tsx
import React from 'react';
import type { Invoice } from '../../types/invoice';

interface Props {
  invoice: Invoice;
}

const InvoiceTemplate: React.FC<Props> = ({ invoice }) => {
  // تابع برای فرمت اعداد با جداکننده هزارگان
  const formatNumber = (num: number) => num.toLocaleString('fa-IR');

  return (
    <div className="invoice-container" dir="rtl">
      <div className="header">
        <h2>صورتحساب فروش کالا و خدمات</h2>
        <div className="date">تاریخ: {invoice.date}</div>
        <div className="invoice-number">شماره: {invoice.invoiceNumber}</div>
      </div>

      {/* مشخصات فروشنده */}
      <table>
        <thead>
          <tr>
            <th colSpan={4} className="section-title">مشخصات فروشنده</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>نام شخص حقیقی/حقوقی: {invoice.seller.name}</td>
            <td>شماره اقتصادی: {invoice.seller.economicCode}</td>
            <td>شماره ثبت: {invoice.seller.registrationNumber}</td>
            <td>شناسه ملی: {invoice.seller.nationalId}</td>
          </tr>
          <tr>
            <td>استان: {invoice.seller.province}</td>
            <td>شهر: {invoice.seller.city}</td>
            <td colSpan={2}>نشانی کامل: {invoice.seller.address}</td>
          </tr>
          <tr>
            <td>کد پستی: {invoice.seller.postalCode}</td>
            <td>تلفن: {invoice.seller.phone}</td>
            <td colSpan={2}>همراه: {invoice.seller.mobile}</td>
          </tr>
        </tbody>
      </table>

      {/* مشخصات خریدار */}
      <table>
        <thead>
          <tr>
            <th colSpan={4} className="section-title">مشخصات خریدار</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>نام شخص حقیقی/حقوقی: {invoice.buyer.name}</td>
            <td>شماره اقتصادی: {invoice.buyer.economicCode}</td>
            <td>شماره ثبت: {invoice.buyer.registrationNumber}</td>
            <td>شناسه ملی: {invoice.buyer.nationalId}</td>
          </tr>
          <tr>
            <td>استان: {invoice.buyer.province}</td>
            <td>شهر: {invoice.buyer.city}</td>
            <td colSpan={2}>نشانی کامل: {invoice.buyer.address}</td>
          </tr>
          <tr>
            <td>کد پستی: {invoice.buyer.postalCode}</td>
            <td>تلفن: {invoice.buyer.phone}</td>
            <td colSpan={2}>همراه: {invoice.buyer.mobile}</td>
          </tr>
        </tbody>
      </table>

      {/* جدول کالاها */}
      <table className="items-table">
        <thead>
          <tr>
            <th>ردیف</th>
            <th>شرح کالا یا خدمات</th>
            <th>تعداد</th>
            <th>مبلغ واحد</th>
            <th>تخفیف</th>
            <th>مالیات</th>
            <th>جمع کل</th>
          </tr>
        </thead>
        <tbody>
          {invoice.items.map((item, index) => {
            const total = (item.quantity * item.unitPrice) - item.discount + item.tax;
            return (
              <tr key={index}>
                <td>{index + 1}</td>
                <td>{item.description}</td>
                <td>{item.quantity}</td>
                <td>{formatNumber(item.unitPrice)}</td>
                <td>{formatNumber(item.discount)}</td>
                <td>{formatNumber(item.tax)}</td>
                <td>{formatNumber(total)}</td>
              </tr>
            );
          })}
        </tbody>
        <tfoot>
          <tr>
            <td colSpan={6} className="total-label">جمع کل قابل پرداخت:</td>
            <td className="total-amount">{formatNumber(invoice.totalAmount || 0)}</td>
          </tr>
        </tfoot>
      </table>

      {/* فوتر و امضاها */}
      <table className="footer-table">
        <tbody>
          <tr>
            <td colSpan={2} className="payment-type">
              شرایط و نحوه فروش: 
              <span className="checkbox-item">
                <input type="checkbox" checked={invoice.paymentType === 'cash'} readOnly /> نقدی
              </span>
              <span className="checkbox-item">
                <input type="checkbox" checked={invoice.paymentType === 'credit'} readOnly /> غیر نقدی
              </span>
            </td>
          </tr>
          <tr>
            <td colSpan={2}>توضیحات: {invoice.notes}</td>
          </tr>
          <tr>
            <td className="signature">مهر و امضاء فروشنده:</td>
            <td className="signature">مهر و امضاء خریدار:</td>
          </tr>
        </tbody>
      </table>

      {/* دکمه چاپ (در چاپ دیده نمی‌شود) */}
      <div className="no-print print-button-container">
        <button onClick={() => window.print()} className="btn-print">
          🖨️ چاپ فاکتور
        </button>
      </div>
    </div>
  );
};

export default InvoiceTemplate;