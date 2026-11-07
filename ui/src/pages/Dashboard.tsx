const Dashboard = () => {
  return (
    <div className="page-container">
      <h1>داشبورد مدیریت</h1>
      <p style={{ color: '#64748b' }}>به سیستم مدیریت فاکتور خوش آمدید.</p>
      
      <div className="dashboard-grid">
        <div className="stat-card">
          <h3>تعداد کل فاکتورها</h3>
          <div className="value">۱۲</div>
        </div>
        <div className="stat-card">
          <h3>فروش امروز</h3>
          <div className="value">۴,۵۰۰,۰۰۰</div>
        </div>
        <div className="stat-card">
          <h3>مشتریان فعال</h3>
          <div className="value">۸</div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;