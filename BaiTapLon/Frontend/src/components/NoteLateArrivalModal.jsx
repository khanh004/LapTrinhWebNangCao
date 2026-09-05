import { useState } from "react";
import { bookingsApi } from "../api/bookings";

const defaultDeadline = () => {
  const d = new Date();
  d.setHours(d.getHours() + 2);
  return d.toISOString().slice(0, 16);
};

export default function NoteLateArrivalModal({ booking, onClose, onDone }) {
  const [newDeadline, setNewDeadline] = useState(defaultDeadline());
  const [note, setNote] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async () => {
    setLoading(true);
    setError("");
    try {
      await bookingsApi.noteLateArrival(booking.id, {
        newArrivalDeadline: new Date(newDeadline).toISOString(),
        note,
      });
      onDone?.();
      onClose();
    } catch (err) {
      setError(err.response?.data || "Ghi chú thất bại.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 backdrop-blur-[2px] flex items-center justify-center z-50 p-4 modal-overlay">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md modal-panel">
        <div className="px-6 py-5 border-b border-gray-100">
          <p className="text-xs text-gray-400 font-medium">Ghi chú</p>
          <h2 className="text-lg font-bold text-gray-900 font-display">Khách hẹn đến trễ</h2>
          <p className="text-sm text-gray-500 mt-0.5">
            Phòng {booking.roomNumber} — {booking.customerName}
          </p>
        </div>

        <div className="p-6">
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Khách hẹn đến lúc</label>
          <input
            type="datetime-local"
            className="input px-3 py-2 text-sm mb-4"
            value={newDeadline}
            onChange={(e) => setNewDeadline(e.target.value)}
          />

          <label className="block text-xs font-medium text-gray-500 mb-1.5">Ghi chú (tùy chọn)</label>
          <textarea
            className="input px-3 py-2 text-sm mb-4"
            rows={3}
            value={note}
            onChange={(e) => setNote(e.target.value)}
            placeholder="VD: Khách kẹt xe, hẹn lại 16h..."
          />

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
              {loading ? "Đang lưu..." : "Lưu ghi chú"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}