// src/pages/Dashboard.tsx
import { useEffect, useState } from 'react';
import { getDashboardStats } from '../services/api';
import type { DashboardStats } from '../services/api';

const Dashboard = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getDashboardStats()
      .then((data) => {
        setStats(data);
        setLoading(false);
      })
      .catch((err) => {
        console.error("خطا در دریافت اطلاعات داشبورد:", err);
        setLoading(false);
      });
  }, []);

  if (loading) return <div className="page-container">در حال بارگذاری...</div>;
  if (!stats) return <div className="page-container">خطا در بارگذاری اطلاعات</div>;

  return (
    <div className="page-container">
      <h1>داشبورد مدیریت</h1>
      <p style={{ color: '#64748b', marginBottom: '20px' }}>خلاصه وضعیت سیستم فاکتورهای شما</p>
      
      <div className="dashboard-grid">
        <div className="stat-card">
          <h3>تعداد کل فاکتورها</h3>
          <div className="value">{stats.totalInvoices.toLocaleString('fa-IR')}</div>
        </div>

        <div className="stat-card">
          <h3>مجموع فروش کل (ریال)</h3>
          <div className="value">{stats.totalRevenue.toLocaleString('fa-IR')}</div>
        </div>

        <div className="stat-card">
          <h3>فاکتورهای امروز</h3>
          <div className="value">{stats.todayInvoices.toLocaleString('fa-IR')}</div>
        </div>

        <div className="stat-card">
          <h3>فروش امروز (ریال)</h3>
          <div className="value">{stats.todayRevenue.toLocaleString('fa-IR')}</div>
        </div>

        <div className="stat-card">
          <h3>مشتریان فعال</h3>
          <div className="value">{stats.totalCustomers.toLocaleString('fa-IR')}</div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;