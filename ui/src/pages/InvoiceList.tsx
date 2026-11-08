import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { FormEvent } from 'react';

import DatePickerModule from 'react-multi-date-picker';
import persian from 'react-date-object/calendars/persian';
import persian_fa from 'react-date-object/locales/persian_fa';
import gregorian from 'react-date-object/calendars/gregorian';
import DateObject from 'react-date-object';

import type { Invoice } from '../types/invoice';
import {
  getInvoices,
  getTodaySummary,
} from '../services/api';

const DatePickerComponent =
  (DatePickerModule as any).default ?? DatePickerModule;

interface TodaySummary {
  date: string;
  jalaliDate: string;
  invoiceCount: number;
  totalAmount: number;
}

// تبدیل تاریخ میلادی API به شمسی، بدون جابه‌جایی روز بر اثر timezone
const formatToJalali = (dateString: string) => {
  if (!dateString) return '-';

  try {
    const [datePart] = dateString.split('T');
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

// تبدیل تاریخ شمسی انتخاب‌شده به میلادی برای API
const convertToApiDate = (date: DateObject | null) => {
  if (!date) return undefined;

  const gregorianDate = date.convert(gregorian);

  return gregorianDate
    .format('YYYY-MM-DD')
    .replace(/[۰-۹]/g, (digit) =>
      String('۰۱۲۳۴۵۶۷۸۹'.indexOf(digit))
    )
    .replace(/[٠-٩]/g, (digit) =>
      String('٠١٢٣٤٥٦٧٨٩'.indexOf(digit))
    );
};

const formatAmount = (amount: number | undefined) =>
  (amount ?? 0).toLocaleString('fa-IR');

const InvoiceList = () => {
  const navigate = useNavigate();

  // اطلاعات جدول
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [loading, setLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');

  // آمار امروز
  const [todaySummary, setTodaySummary] =
    useState<TodaySummary | null>(null);
  const [summaryLoading, setSummaryLoading] = useState(true);
  const [summaryError, setSummaryError] = useState('');

  // صفحه‌بندی
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const [totalCount, setTotalCount] = useState(0);

  // وضعیت باز و بسته بودن فیلتر پیشرفته
  const [advancedFilterOpen, setAdvancedFilterOpen] = useState(false);

  // مقادیر واردشده در فرم فیلتر
  const [startDate, setStartDate] = useState<DateObject | null>(null);
  const [endDate, setEndDate] = useState<DateObject | null>(null);
  const [buyerName, setBuyerName] = useState('');
  const [minAmount, setMinAmount] = useState('');
  const [maxAmount, setMaxAmount] = useState('');

  // مقادیر اعمال‌شده برای جستجو
  const [filterStartDate, setFilterStartDate] =
    useState<DateObject | null>(null);
  const [filterEndDate, setFilterEndDate] =
    useState<DateObject | null>(null);
  const [filterBuyerName, setFilterBuyerName] = useState('');
  const [filterMinAmount, setFilterMinAmount] = useState('');
  const [filterMaxAmount, setFilterMaxAmount] = useState('');

  const totalPages = Math.ceil(totalCount / pageSize);

  // دریافت مستقل آمار امروز
  useEffect(() => {
    let cancelled = false;

    const fetchTodaySummary = async () => {
      setSummaryLoading(true);
      setSummaryError('');

      try {
        const data = await getTodaySummary();

        if (!cancelled) {
          setTodaySummary(data);
        }
      } catch (error) {
        console.error('خطا در دریافت آمار امروز:', error);

        if (!cancelled) {
          setSummaryError('دریافت آمار امروز با خطا مواجه شد.');
          setTodaySummary(null);
        }
      } finally {
        if (!cancelled) {
          setSummaryLoading(false);
        }
      }
    };

    fetchTodaySummary();

    return () => {
      cancelled = true;
    };
  }, []);

  // دریافت فاکتورها
  useEffect(() => {
    let cancelled = false;

    const fetchInvoices = async () => {
      setLoading(true);
      setErrorMsg('');

      try {
        const apiStartDate = convertToApiDate(filterStartDate);
        const apiEndDate = convertToApiDate(filterEndDate);

        const data = await getInvoices(
          page,
          pageSize,
          apiStartDate,
          apiEndDate,
          filterBuyerName || undefined,
          filterMinAmount ? Number(filterMinAmount) : undefined,
          filterMaxAmount ? Number(filterMaxAmount) : undefined
        );

        if (cancelled) return;

        setInvoices(data?.items ?? []);
        setTotalCount(data?.totalCount ?? 0);
      } catch (error: any) {
        if (cancelled) return;

        console.error('خطای دریافت فاکتورها:', error);
        console.error('Server Response:', error.response?.data);

        setErrorMsg(
          error.response?.data?.title ||
          error.response?.data?.message ||
          error.message ||
          'خطا در دریافت فاکتورها.'
        );

        setInvoices([]);
        setTotalCount(0);
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    fetchInvoices();

    return () => {
      cancelled = true;
    };
  }, [
    page,
    pageSize,
    filterStartDate,
    filterEndDate,
    filterBuyerName,
    filterMinAmount,
    filterMaxAmount,
  ]);

  // اعمال فیلترها
  const handleSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (startDate && endDate && startDate.toJulianDay() > endDate.toJulianDay()) {
      setErrorMsg('تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.');
      return;
    }

    const min = minAmount.trim() ? Number(minAmount) : undefined;
    const max = maxAmount.trim() ? Number(maxAmount) : undefined;

    if (min !== undefined && (!Number.isFinite(min) || min < 0)) {
      setErrorMsg('حداقل مبلغ معتبر نیست.');
      return;
    }

    if (max !== undefined && (!Number.isFinite(max) || max < 0)) {
      setErrorMsg('حداکثر مبلغ معتبر نیست.');
      return;
    }

    if (min !== undefined && max !== undefined && min > max) {
      setErrorMsg('حداقل مبلغ نمی‌تواند بیشتر از حداکثر مبلغ باشد.');
      return;
    }

    setErrorMsg('');
    setPage(1);

    setFilterStartDate(startDate);
    setFilterEndDate(endDate);
    setFilterBuyerName(buyerName.trim());
    setFilterMinAmount(minAmount.trim());
    setFilterMaxAmount(maxAmount.trim());
  };

  // حذف تمام فیلترها
  const clearFilters = () => {
    setStartDate(null);
    setEndDate(null);
    setBuyerName('');
    setMinAmount('');
    setMaxAmount('');

    setFilterStartDate(null);
    setFilterEndDate(null);
    setFilterBuyerName('');
    setFilterMinAmount('');
    setFilterMaxAmount('');

    setErrorMsg('');
    setPage(1);
  };

  const hasActiveFilters =
    filterStartDate !== null ||
    filterEndDate !== null ||
    filterBuyerName !== '' ||
    filterMinAmount !== '' ||
    filterMaxAmount !== '';

  return (
    <div className="page-container invoice-list-page">

      {/* عنوان صفحه */}
      <div className="list-header invoice-list-header">
        <div>
          <h2>لیست فاکتورهای صادرشده</h2>
          <p className="page-subtitle">
            مشاهده، جستجو و مدیریت فاکتورهای صادرشده
          </p>
        </div>

        <button
          type="button"
          className="btn-primary"
          onClick={() => navigate('/invoices/new')}
        >
          <span>+</span>
          صدور فاکتور جدید
        </button>
      </div>

      {/* خلاصه عملکرد امروز */}
      <section className="today-summary">

        <div className="summary-heading">
          <div>
            <h3>خلاصه عملکرد امروز</h3>

            <p>
              {todaySummary?.jalaliDate
                ? `امروز ${todaySummary.jalaliDate}`
                : 'آمار روز جاری'}
            </p>
          </div>

          <span className="summary-date-icon">◷</span>
        </div>

        {summaryLoading ? (
          <div className="summary-loading">
            در حال دریافت آمار امروز...
          </div>
        ) : summaryError ? (
          <div className="summary-error">
            {summaryError}
          </div>
        ) : (
          <div className="summary-cards">

            <div className="summary-card">
              <div className="summary-card-icon invoice-count-icon">
                <span>▤</span>
              </div>

              <div className="summary-card-content">
                <span className="summary-label">
                  تعداد فاکتورهای امروز
                </span>

                <strong>
                  {formatAmount(todaySummary?.invoiceCount)}
                </strong>

                <small>فاکتور صادرشده</small>
              </div>
            </div>

            <div className="summary-card">
              <div className="summary-card-icon invoice-amount-icon">
                <span>◈</span>
              </div>

              <div className="summary-card-content">
                <span className="summary-label">
                  مجموع مبلغ فاکتورهای امروز
                </span>

                <strong>
                  {formatAmount(todaySummary?.totalAmount)}
                </strong>

                <small>ریال</small>
              </div>
            </div>

          </div>
        )}

        {!summaryLoading &&
          !summaryError &&
          todaySummary?.invoiceCount === 0 && (
            <div className="summary-empty">
              هنوز فاکتوری برای امروز صادر نشده است.
            </div>
          )}
      </section>

      {/* فیلترها */}
      <section className="invoice-filter-section">

        <div className="filter-section-heading">
          <div>
            <h3>جستجو و فیلتر فاکتورها</h3>
            <p>برای یافتن فاکتورهای موردنظر از فیلترها استفاده کنید.</p>
          </div>

          {hasActiveFilters && (
            <span className="active-filter-label">
              فیلتر فعال
            </span>
          )}
        </div>

        <form onSubmit={handleSearch}>

          {/* فیلترهای اصلی */}
          <div className="basic-filters">

            <div className="search-group">
              <label>از تاریخ</label>

              <DatePickerComponent
                calendar={persian}
                locale={persian_fa}
                calendarPosition="bottom-right"
                format="YYYY/MM/DD"
                value={startDate}
                onChange={(date: DateObject | null) =>
                  setStartDate(date)
                }
                placeholder="انتخاب تاریخ شروع"
                inputClass="date-picker-input"
              />
            </div>

            <div className="search-group">
              <label>تا تاریخ</label>

              <DatePickerComponent
                calendar={persian}
                locale={persian_fa}
                calendarPosition="bottom-right"
                format="YYYY/MM/DD"
                value={endDate}
                onChange={(date: DateObject | null) =>
                  setEndDate(date)
                }
                placeholder="انتخاب تاریخ پایان"
                inputClass="date-picker-input"
                minDate={startDate || undefined}
              />
            </div>

          </div>

          {/* باز و بسته کردن فیلتر پیشرفته */}
          <button
            type="button"
            className="advanced-filter-toggle"
            onClick={() =>
              setAdvancedFilterOpen((previous) => !previous)
            }
            aria-expanded={advancedFilterOpen}
          >
            <span className="advanced-filter-toggle-icon">
              {advancedFilterOpen ? '−' : '+'}
            </span>

            <span>
              {advancedFilterOpen
                ? 'بستن فیلتر پیشرفته'
                : 'فیلتر پیشرفته'}
            </span>

            <span className="advanced-filter-description">
              نام خریدار و بازه مبلغ
            </span>

            <span className={`advanced-chevron ${advancedFilterOpen ? 'is-open' : ''}`}>
              ▾
            </span>
          </button>

          {/* فیلترهای پیشرفته */}
          {advancedFilterOpen && (
            <div className="advanced-filters">

              <div className="search-group buyer-filter">
                <label htmlFor="buyerName">
                  نام خریدار
                </label>

                <input
                  id="buyerName"
                  type="text"
                  value={buyerName}
                  onChange={(event) =>
                    setBuyerName(event.target.value)
                  }
                  placeholder="بخشی از نام خریدار را وارد کنید"
                  className="filter-text-input"
                />
              </div>

              <div className="search-group">
                <label htmlFor="minAmount">
                  حداقل مبلغ (ریال)
                </label>

                <input
                  id="minAmount"
                  type="number"
                  min="0"
                  value={minAmount}
                  onChange={(event) =>
                    setMinAmount(event.target.value)
                  }
                  placeholder="مثلاً ۱۰۰۰۰۰۰"
                  className="filter-text-input"
                />
              </div>

              <div className="search-group">
                <label htmlFor="maxAmount">
                  حداکثر مبلغ (ریال)
                </label>

                <input
                  id="maxAmount"
                  type="number"
                  min="0"
                  value={maxAmount}
                  onChange={(event) =>
                    setMaxAmount(event.target.value)
                  }
                  placeholder="مثلاً ۵۰۰۰۰۰۰۰"
                  className="filter-text-input"
                />
              </div>

            </div>
          )}

          {/* دکمه‌های عملیات فیلتر */}
          <div className="filter-actions">

            <button type="submit" className="btn-search">
              <span>جستجو</span>
              <span>⌕</span>
            </button>

            <button
              type="button"
              className="btn-clear"
              onClick={clearFilters}
            >
              پاک کردن فیلترها
              <span>×</span>
            </button>

          </div>

        </form>
      </section>

      {/* پیام خطا */}
      {errorMsg && (
        <div className="invoice-error-message">
          {errorMsg}
        </div>
      )}

      {/* جدول فاکتورها */}
      <section className="invoice-table-section">

        <div className="table-section-heading">
          <div>
            <h3>فاکتورها</h3>
            <p>
              تعداد نتایج: {formatAmount(totalCount)}
            </p>
          </div>
        </div>

        {loading ? (
          <div className="invoice-loading">
            در حال دریافت فاکتورها...
          </div>
        ) : (
          <>
            <div className="table-container">
              <table className="data-table">

                <thead>
                  <tr>
                    <th>شماره فاکتور</th>
                    <th>تاریخ (شمسی)</th>
                    <th>خریدار</th>
                    <th>مبلغ کل (ریال)</th>
                    <th>نوع پرداخت</th>
                    <th>عملیات</th>
                  </tr>
                </thead>

                <tbody>
                  {invoices.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="empty-table-message">
                        فاکتوری با مشخصات جستجو شده یافت نشد.
                      </td>
                    </tr>
                  ) : (
                    invoices.map((inv) => (
                      <tr
                        key={inv.id}
                        onClick={() =>
                          navigate(`/invoices/${inv.id}`)
                        }
                        className="invoice-table-row"
                      >
                        <td>{inv.invoiceNumber || '-'}</td>

                        <td>{formatToJalali(inv.date)}</td>

                        <td>{inv.buyer?.name ?? '-'}</td>

                        <td className="invoice-amount-cell">
                          {formatAmount(inv.totalAmount)}
                        </td>

                        <td>
                          <span
                            className={
                              inv.paymentType === 'cash'
                                ? 'payment-badge payment-cash'
                                : 'payment-badge payment-noncash'
                            }
                          >
                            {inv.paymentType === 'cash'
                              ? 'نقدی'
                              : 'غیر نقدی'}
                          </span>
                        </td>

                        <td>
                          <button
                            type="button"
                            className="invoice-view-button"
                            onClick={(event) => {
                              event.stopPropagation();
                              navigate(`/invoices/${inv.id}`);
                            }}
                          >
                            مشاهده
                          </button>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>

              </table>
            </div>

            {/* صفحه‌بندی */}
            <div className="pagination invoice-pagination">

              <button
                type="button"
                disabled={page === 1}
                onClick={() => setPage((current) => current - 1)}
                className="btn-page"
              >
                قبلی
              </button>

              <span>
                صفحه {page.toLocaleString('fa-IR')} از{' '}
                {(totalPages || 1).toLocaleString('fa-IR')}
              </span>

              <button
                type="button"
                disabled={page >= totalPages || totalPages === 0}
                onClick={() => setPage((current) => current + 1)}
                className="btn-page"
              >
                بعدی
              </button>

            </div>
          </>
        )}

      </section>
    </div>
  );
};

export default InvoiceList;