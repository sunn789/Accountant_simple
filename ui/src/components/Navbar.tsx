import { NavLink } from 'react-router-dom';

const Navbar = () => {
  return (
    <nav className="navbar">
      <div className="navbar-brand">سیستم فاکتور من</div>
      <ul className="navbar-links">
        <li><NavLink to="/" className={({ isActive }) => isActive ? 'active' : ''}>داشبورد</NavLink></li>
        <li><NavLink to="/invoices" className={({ isActive }) => isActive ? 'active' : ''}>لیست فاکتورها</NavLink></li>
        <li><NavLink to="/invoices/new" className="btn-create-nav">+ فاکتور جدید</NavLink></li>
      </ul>
    </nav>
  );
};

export default Navbar;