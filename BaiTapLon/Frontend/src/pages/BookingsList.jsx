import { useEffect, useState } from "react";
import Layout, { StatusBadge } from "../components/Layout";
import { bookingsApi } from "../api/bookings";
import CheckOutModal from "../components/CheckOutModal";
import ExtendModal from "../components/ExtendModal";
import NoteLateArrivalModal from "../components/NoteLateArrivalModal";
import BookingLogsModal from "../components/BookingLogsModal";

const STATUS_FILTERS = [
  { value: "", label: "Tất cả" },
  { value: "PENDING", label: "Chờ xác nhận" },
  { value: "CONFIRMED", label: "Đã xác nhận" },
  { value: "CHECKED_IN", label: "Đang lưu trú" },
  { value: "CHECKED_OUT", label: "Đã trả phòng" },
  { value: "CANCELLED", label: "Đã hủy" },
];

function ActionBtn({ onClick, tone, children }) {
  return (
    <button
      onClick={onClick}
      className={`chip-btn chip-btn-${tone} text-xs font-semibold px-2.5 py-1.5 rounded-lg`}
    >
      {children}
    </button>
  );
}

export default function BookingsList() {
  const [bookings, setBookings] = useState([]);
  const [statusFilter, setStatusFilter] = useState("");
  const [loading, setLoading] = useState(true);
  const [actionError, setActionError] = useState("");

  const [modal, setModal] = useState(null); // { type: 'checkout'|'extend'|'note'|'logs', booking }

  const load = async () => {
    setLoading(true);
    try {
      const res = await bookingsApi.getAll(statusFilter || undefined);
      setBookings(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [statusFilter]);

  const runAction = async (fn) => {
    setActionError("");
    try {
      await fn();
      load();
    } catch (err) {
      setActionError(err.response?.data || "Thao tác thất bại.");
    }
  };

  return (
    <Layout title="Danh sách đặt phòng">
      <div className="card p-4 mb-5 flex flex-wrap gap-1.5">
        {STATUS_FILTERS.map((f) => (
          <button
            key={f.value}
            onClick={() => setStatusFilter(f.value)}
            className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
              statusFilter === f.value
                ? "bg-[#101a2f] text-white"
                : "bg-gray-100 text-gray-600 hover:bg-gray-200"
            }`}
          >
            {f.label}
          </button>
        ))}
      </div>

      {actionError && (
        <div className="flex items-start gap-2 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg px-3.5 py-2.5 mb-4">
          <svg viewBox="0 0 24 24" className="w-4 h-4 mt-0.5 shrink-0" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="12" cy="12" r="10" />
            <path d="M12 8v5M12 16h.01" strokeLinecap="round" />
          </svg>
          <span>{actionError}</span>
        </div>
      )}

      {loading ? (
        <div className="card p-6 space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-10 bg-gray-50 rounded-lg animate-pulse" />
          ))}
        </div>
      ) : (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50/80 text-gray-500 text-xs uppercase tracking-wide">
                <tr>
                  <th className="text-left px-4 py-3 font-semibold">Mã đặt phòng</th>
                  <th className="text-left px-4 py-3 font-semibold">Khách hàng</th>
                  <th className="text-left px-4 py-3 font-semibold">Phòng</th>
                  <th className="text-left px-4 py-3 font-semibold">Nhận / Trả</th>
                  <th className="text-left px-4 py-3 font-semibold">Trạng thái</th>
                  <th className="text-left px-4 py-3 font-semibold">Hành động</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {bookings.map((b) => (
                  <tr key={b.id} className="hover:bg-gray-50/60 transition-colors">
                    <td className="px-4 py-3 text-gray-400 font-mono text-xs">{b.bookingCode}</td>
                    <td className="px-4 py-3 font-semibold text-gray-800">{b.customerName}</td>
                    <td className="px-4 py-3 text-gray-600">{b.roomNumber}</td>
                    <td className="px-4 py-3 text-gray-500 whitespace-nowrap">
                      {b.checkInDate} → {b.checkOutDate}
                    </td>
                    <td className="px-4 py-3">
                      <StatusBadge status={b.status} />
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex flex-wrap gap-1">
                        {(b.status === "PENDING" || b.status === "CONFIRMED") && (
                          <>
                            {b.status === "PENDING" && (
                              <ActionBtn tone="blue" onClick={() => runAction(() => bookingsApi.confirm(b.id))}>
                                Xác nhận
                              </ActionBtn>
                            )}
                            <ActionBtn tone="green" onClick={() => runAction(() => bookingsApi.checkIn(b.id))}>
                              Nhận phòng
                            </ActionBtn>
                            <ActionBtn tone="orange" onClick={() => setModal({ type: "note", booking: b })}>
                              Ghi chú hẹn
                            </ActionBtn>
                            <ActionBtn tone="red" onClick={() => runAction(() => bookingsApi.cancel(b.id))}>
                              Hủy
                            </ActionBtn>
                          </>
                        )}

                        {b.status === "CHECKED_IN" && (
                          <>
                            <ActionBtn tone="blue" onClick={() => setModal({ type: "checkout", booking: b })}>
                              Trả phòng
                            </ActionBtn>
                            <ActionBtn tone="purple" onClick={() => setModal({ type: "extend", booking: b })}>
                              Gia hạn
                            </ActionBtn>
                          </>
                        )}

                        <ActionBtn tone="gray" onClick={() => setModal({ type: "logs", booking: b })}>
                          Nhật ký
                        </ActionBtn>
                      </div>
                    </td>
                  </tr>
                ))}

                {bookings.length === 0 && (
                  <tr>
                    <td colSpan={6} className="text-center text-gray-400 py-14">
                      Không có đặt phòng nào.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {modal?.type === "checkout" && (
        <CheckOutModal booking={modal.booking} onClose={() => setModal(null)} onDone={load} />
      )}
      {modal?.type === "extend" && (
        <ExtendModal booking={modal.booking} onClose={() => setModal(null)} onDone={load} />
      )}
      {modal?.type === "note" && (
        <NoteLateArrivalModal booking={modal.booking} onClose={() => setModal(null)} onDone={load} />
      )}
      {modal?.type === "logs" && (
        <BookingLogsModal booking={modal.booking} onClose={() => setModal(null)} />
      )}
    </Layout>
  );
}