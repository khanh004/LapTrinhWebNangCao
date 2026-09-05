import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import Layout from "../components/Layout";
import { roomsApi } from "../api/rooms";
import { bookingsApi } from "../api/bookings";

function StatCard({ label, value, color, bg, icon }) {
  return (
    <div className="card p-4 flex items-center gap-3.5">
      <div className={`w-11 h-11 rounded-xl ${bg} ${color} flex items-center justify-center shrink-0`}>
        {icon}
      </div>
      <div>
        <p className="text-2xl font-bold text-gray-900 font-display leading-none">{value}</p>
        <p className="text-xs text-gray-500 mt-1">{label}</p>
      </div>
    </div>
  );
}

function QuickLink({ to, title, description, icon }) {
  return (
    <Link
      to={to}
      className="card p-5 flex items-start gap-4 hover:shadow-md hover:-translate-y-0.5 transition-all"
    >
      <div className="w-10 h-10 rounded-lg bg-[#101a2f] text-[#d9b96a] flex items-center justify-center shrink-0">
        {icon}
      </div>
      <div>
        <p className="font-semibold text-gray-900">{title}</p>
        <p className="text-sm text-gray-500 mt-0.5">{description}</p>
      </div>
    </Link>
  );
}

export default function AdminDashboard() {
  const [roomStats, setRoomStats] = useState(null);
  const [bookingStats, setBookingStats] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const [roomsRes, bookingsRes] = await Promise.all([
          roomsApi.getAll({}),
          bookingsApi.getAll(undefined),
        ]);

        const rooms = roomsRes.data;
        const counts = { AVAILABLE: 0, RESERVED: 0, OCCUPIED: 0, CLEANING: 0, MAINTENANCE: 0 };
        rooms.forEach((r) => {
          counts[r.status] = (counts[r.status] || 0) + 1;
        });
        setRoomStats({ total: rooms.length, counts });

        const bookings = bookingsRes.data;
        const active = bookings.filter((b) =>
          ["PENDING", "CONFIRMED", "CHECKED_IN"].includes(b.status)
        ).length;
        setBookingStats({ total: bookings.length, active });
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  return (
    <Layout title="Tổng quan hệ thống">
      {loading ? (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
          {Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="card p-4 h-20 animate-pulse bg-gray-50" />
          ))}
        </div>
      ) : (
        <>
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
            <StatCard
              label="Tổng số phòng"
              value={roomStats.total}
              color="text-[#101a2f]"
              bg="bg-gray-100"
              icon={
                <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
              }
            />
            <StatCard
              label="Phòng trống"
              value={roomStats.counts.AVAILABLE}
              color="text-emerald-600"
              bg="bg-emerald-50"
              icon={
                <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M20 6 9 17l-5-5" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
              }
            />
            <StatCard
              label="Đang dọn dẹp"
              value={roomStats.counts.CLEANING}
              color="text-purple-600"
              bg="bg-purple-50"
              icon={
                <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                  <circle cx="12" cy="12" r="9" />
                  <path d="M12 7v5l3 2" strokeLinecap="round" />
                </svg>
              }
            />
            <StatCard
              label="Đặt phòng đang hoạt động"
              value={bookingStats.active}
              color="text-blue-600"
              bg="bg-blue-50"
              icon={
                <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                  <rect x="3" y="4" width="18" height="17" rx="2" />
                  <path d="M8 2v4M16 2v4M3 10h18" strokeLinecap="round" />
                </svg>
              }
            />
          </div>

          <p className="text-sm font-semibold text-gray-700 mb-3">Điều hướng nhanh</p>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            <QuickLink
              to="/reception"
              title="Bảng phòng"
              description="Xem tình trạng phòng, đặt phòng và hàng chờ."
              icon={
                <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                  <rect x="3" y="3" width="7" height="7" rx="1.5" />
                  <rect x="14" y="3" width="7" height="7" rx="1.5" />
                  <rect x="3" y="14" width="7" height="7" rx="1.5" />
                  <rect x="14" y="14" width="7" height="7" rx="1.5" />
                </svg>
              }
            />
            <QuickLink
              to="/bookings"
              title="Đặt phòng"
              description="Quản lý xác nhận, nhận/trả phòng, gia hạn."
              icon={
                <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                  <rect x="3" y="4" width="18" height="17" rx="2" />
                  <path d="M8 2v4M16 2v4M3 10h18" strokeLinecap="round" />
                </svg>
              }
            />
            <QuickLink
              to="/housekeeping"
              title="Buồng phòng"
              description="Xác nhận các phòng đã dọn dẹp xong."
              icon={
                <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                  <circle cx="12" cy="12" r="9" />
                  <path d="M12 7v5l3 2" strokeLinecap="round" />
                </svg>
              }
            />
            <QuickLink
              to="/admin/room-types"
              title="Loại phòng"
              description="Thêm/sửa/xóa loại phòng, giá và số khách tối đa."
              icon={
                <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M4 4h16v6H4zM4 14h7v6H4zM13 14h7v6h-7z" strokeLinejoin="round" />
                </svg>
              }
            />
            <QuickLink
              to="/admin/rooms"
              title="Phòng"
              description="Quản lý danh sách phòng và trạng thái từng phòng."
              icon={
                <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
              }
            />
            <QuickLink
              to="/admin/customers"
              title="Khách hàng"
              description="Xem, thêm mới và chỉnh sửa hồ sơ khách hàng."
              icon={
                <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                  <circle cx="12" cy="8" r="4" />
                  <path d="M4 21c0-4.4 3.6-8 8-8s8 3.6 8 8" strokeLinecap="round" />
                </svg>
              }
            />
          </div>
        </>
      )}
    </Layout>
  );
}