import { useState } from "react";
import { bookingsApi } from "../api/bookings";

export default function ExtendModal({ booking, onClose, onDone }) {
  const [type, setType] = useState("DAYS");
  const [newDate, setNewDate] = useState(booking.checkOutDate);
  const [hours, setHours] = useState(2);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async () => {
    setLoading(true);
    setError("");
    try {
      const payload =
        type === "DAYS"
          ? { extensionType: "DAYS", newCheckOutDate: newDate }
          : { extensionType: "HOURS", additionalHours: Number(hours) };

      await bookingsApi.extend(booking.id, payload);
      onDone?.();
      onClose();
    } catch (err) {
      setError(err.response?.data || "Gia hạn thất bại.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 backdrop-blur-[2px] flex items-center justify-center z-50 p-4 modal-overlay">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md modal-panel">
        <div className="px-6 py-5 border-b border-gray-100">
          <p className="text-xs text-gray-400 font-medium">Gia hạn lưu trú</p>
          <h2 className="text-lg font-bold text-gray-900 font-display">Phòng {booking.roomNumber}</h2>
          <p className="text-sm text-gray-500 mt-0.5">Khách: {booking.customerName}</p>
        </div>

        <div className="p-6">
          <div className="flex gap-2 mb-4 bg-gray-100 rounded-xl p-1">
            <button
              onClick={() => setType("DAYS")}
              className={`flex-1 py-2 rounded-lg text-sm font-medium transition-colors ${
                type === "DAYS" ? "bg-white text-gray-900 shadow-sm" : "text-gray-500"
              }`}
            >
              Thêm đêm
            </button>
            <button
              onClick={() => setType("HOURS")}
              className={`flex-1 py-2 rounded-lg text-sm font-medium transition-colors ${
                type === "HOURS" ? "bg-white text-gray-900 shadow-sm" : "text-gray-500"
              }`}
            >
              Thêm giờ (trong ngày trả)
            </button>
          </div>

          {type === "DAYS" ? (
            <div className="mb-4">
              <label className="block text-xs font-medium text-gray-500 mb-1.5">Ngày trả phòng mới</label>
              <input
                type="date"
                className="input px-3 py-2 text-sm"
                value={newDate}
                onChange={(e) => setNewDate(e.target.value)}
              />
            </div>
          ) : (
            <div className="mb-4">
              <label className="block text-xs font-medium text-gray-500 mb-1.5">Số giờ thêm (tối đa 6 giờ)</label>
              <input
                type="number"
                min={1}
                max={6}
                className="input px-3 py-2 text-sm"
                value={hours}
                onChange={(e) => setHours(e.target.value)}
              />
              <p className="text-xs text-gray-400 mt-1.5">
                Hệ thống sẽ tự kiểm tra không có khách khác nhận phòng cùng ngày trước khi duyệt.
              </p>
            </div>
          )}

          {error && (
            <div className="flex items-start gap-2 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg px-3.5 py-2.5 mb-4">
              <svg viewBox="0 0 24 24" className="w-4 h-4 mt-0.5 shrink-0" fill="none" stroke="currentColor" strokeWidth="2">
                <circle cx="12" cy="12" r="10" />
                <path d="M12 8v5M12 16h.01" strokeLinecap="round" />
              </svg>
              <span>{error}</span>
            </div>
          )}

          <div className="flex gap-2">
            <button onClick={onClose} className="btn btn-outline flex-1 py-2.5 text-sm">
              Hủy
            </button>
            <button onClick={handleSubmit} disabled={loading} className="btn btn-primary flex-1 py-2.5 text-sm">
              {loading ? "Đang xử lý..." : "Xác nhận gia hạn"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}