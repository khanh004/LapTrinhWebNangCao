import { useAuth } from "../context/AuthContext";
import { useNavigate, Link, useLocation } from "react-router-dom";

const STATUS_COLORS = {
  AVAILABLE: "bg-emerald-600 text-white",
  RESERVED: "bg-amber-500 text-white",
  OCCUPIED: "bg-blue-600 text-white",
  CLEANING: "bg-purple-600 text-white",
  MAINTENANCE: "bg-red-600 text-white",
  PENDING: "bg-amber-500 text-white",
  CONFIRMED: "bg-blue-600 text-white",
  CHECKED_IN: "bg-emerald-600 text-white",
  CHECKED_OUT: "bg-gray-500 text-white",
  CANCELLED: "bg-red-600 text-white",
};
const STATUS_DOT = {
  AVAILABLE: "bg-emerald-500",
  RESERVED: "bg-amber-500",
  OCCUPIED: "bg-blue-500",
  CLEANING: "bg-purple-500",
  MAINTENANCE: "bg-red-500",
  PENDING: "bg-amber-500",
  CONFIRMED: "bg-blue-500",
  CHECKED_IN: "bg-emerald-500",
  CHECKED_OUT: "bg-gray-400",
  CANCELLED: "bg-red-500",
};

const STATUS_LABELS = {
  AVAILABLE: "Trống",
  RESERVED: "Đã đặt trước",
  OCCUPIED: "Đang có khách",
  CLEANING: "Đang dọn dẹp",
  MAINTENANCE: "Bảo trì",
  PENDING: "Chờ xác nhận",
  CONFIRMED: "Đã xác nhận",
  CHECKED_IN: "Đang lưu trú",
  CHECKED_OUT: "Đã trả phòng",
  CANCELLED: "Đã hủy",
};

export function StatusBadge({ status }) {
  const cls = STATUS_COLORS[status] || "bg-gray-500 text-white";
  return (
    <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-bold whitespace-nowrap shadow-sm ${cls}`}>
      {STATUS_LABELS[status] || status}
    </span>
  );
}

function NavTab({ to, label, icon }) {
  const location = useLocation();
  const active = location.pathname === to;
  return (
    <Link
      to={to}
      className={`flex items-center gap-2 px-3.5 py-2 rounded-lg text-sm font-medium transition-colors ${
        active
          ? "bg-white/10 text-white shadow-inner"
          : "text-slate-300 hover:bg-white/5 hover:text-white"
      }`}
    >
      {icon}
      {label}
    </Link>
  );
}

function LogoMark() {
  return (
    <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-[#d9b96a] to-[#c9a24b] flex items-center justify-center shadow-sm shrink-0">
      <svg viewBox="0 0 24 24" className="w-5 h-5 text-[#101a2f]" fill="none" stroke="currentColor" strokeWidth="2">
        <path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6M9 9h.01M15 9h.01M9 13h.01M15 13h.01" strokeLinecap="round" strokeLinejoin="round" />
      </svg>
    </div>
  );
}

export default function Layout({ title, children }) {
  const { user, logout, hasRole } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  const initials = (user?.fullName || user?.username || "?")
    .split(" ")
    .filter(Boolean)
    .slice(-2)
    .map((s) => s[0])
    .join("")
    .toUpperCase();

  return (
    <div className="min-h-screen">
      <header className="sticky top-0 z-40 bg-gradient-to-r from-[#0b1220] to-[#16213f] shadow-lg shadow-black/10">
        <div className="max-w-7xl mx-auto px-4 sm:px-6">
          <div className="flex items-center justify-between py-3.5 border-b border-white/10">
            <div className="flex items-center gap-3">
              <LogoMark />
              <div>
                <p className="text-[10px] uppercase tracking-widest text-[#d9b96a] font-semibold leading-none mb-1">
                  Grand Hotel PMS
                </p>
                <h1 className="text-base sm:text-lg font-bold text-white font-display leading-tight">
                  {title}
                </h1>
              </div>
            </div>

            <div className="flex items-center gap-3">
              <div className="hidden sm:flex flex-col items-end leading-tight">
                <span className="text-sm font-semibold text-white">{user?.fullName}</span>
                <span className="text-xs text-slate-400">{user?.roles?.join(", ")}</span>
              </div>
              <div className="w-9 h-9 rounded-full bg-white/10 text-white flex items-center justify-center text-sm font-bold ring-1 ring-white/20">
                {initials}
              </div>
              <button
                onClick={handleLogout}
                title="Đăng xuất"
                className="flex items-center gap-1.5 text-sm text-slate-300 hover:text-white bg-white/5 hover:bg-white/10 px-3 py-2 rounded-lg transition-colors"
              >
                <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
                <span className="hidden md:inline">Đăng xuất</span>
              </button>
            </div>
          </div>

          {hasRole("Admin") && (
  <nav className="flex gap-1.5 py-2.5 overflow-x-auto">
    <NavTab
      to="/admin"
      label="Tổng quan"
      icon={
        <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
          <rect x="3" y="3" width="7" height="9" rx="1.5" />
          <rect x="14" y="3" width="7" height="5" rx="1.5" />
          <rect x="14" y="12" width="7" height="9" rx="1.5" />
          <rect x="3" y="16" width="7" height="5" rx="1.5" />
        </svg>
      }
    />
    <NavTab
      to="/reception"
      label="Bảng phòng"
      icon={
        <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
          <rect x="3" y="3" width="7" height="7" rx="1.5" />
          <rect x="14" y="3" width="7" height="7" rx="1.5" />
          <rect x="3" y="14" width="7" height="7" rx="1.5" />
          <rect x="14" y="14" width="7" height="7" rx="1.5" />
        </svg>
      }
    />
    
    <NavTab
      to="/bookings"
      label="Đặt phòng"
      icon={
        <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
          <rect x="3" y="4" width="18" height="17" rx="2" />
          <path d="M8 2v4M16 2v4M3 10h18" strokeLinecap="round" />
        </svg>
      }
    />

    <NavTab
  to="/customers"
  label="Tra cứu khách"
  icon={
    <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
      <circle cx="11" cy="11" r="7" />
      <path d="m21 21-4.3-4.3" strokeLinecap="round" />
    </svg>
  }
/>
    <NavTab
      to="/housekeeping"
      label="Buồng phòng"
      icon={
        <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
          <circle cx="12" cy="12" r="9" />
          <path d="M12 7v5l3 2" strokeLinecap="round" />
        </svg>
      }
    />
    <span className="w-px bg-white/10 my-1.5 mx-0.5" />
    <NavTab
      to="/admin/room-types"
      label="Loại phòng"
      icon={
        <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M4 4h16v6H4zM4 14h7v6H4zM13 14h7v6h-7z" strokeLinejoin="round" />
        </svg>
      }
    />
    <NavTab
      to="/admin/rooms"
      label="Phòng"
      icon={
        <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      }
    />
    <NavTab
      to="/admin/customers"
      label="Khách hàng"
      icon={
        <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
          <circle cx="12" cy="8" r="4" />
          <path d="M4 21c0-4.4 3.6-8 8-8s8 3.6 8 8" strokeLinecap="round" />
        </svg>
      }
    />
  </nav>
)}

{!hasRole("Admin") && hasRole("Receptionist") && (
  <nav className="flex gap-1.5 py-2.5">
    <NavTab
      to="/reception"
      label="Bảng phòng"
      icon={
        <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
          <rect x="3" y="3" width="7" height="7" rx="1.5" />
          <rect x="14" y="3" width="7" height="7" rx="1.5" />
          <rect x="3" y="14" width="7" height="7" rx="1.5" />
          <rect x="14" y="14" width="7" height="7" rx="1.5" />
        </svg>
      }
    />
    <NavTab
      to="/bookings"
      label="Đặt phòng"
      icon={
        <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
          <rect x="3" y="4" width="18" height="17" rx="2" />
          <path d="M8 2v4M16 2v4M3 10h18" strokeLinecap="round" />
        </svg>
      }
    />
    <NavTab
      to="/customers"
      label="Tra cứu khách"
      icon={
        <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
          <circle cx="11" cy="11" r="7" />
          <path d="m21 21-4.3-4.3" strokeLinecap="round" />
        </svg>
      }
    />
  </nav>
)}
        </div>
      </header>
      <main className="max-w-7xl mx-auto p-4 sm:p-6">{children}</main>
    </div>
  );
}