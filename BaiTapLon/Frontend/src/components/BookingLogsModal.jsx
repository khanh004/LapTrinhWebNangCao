import { useEffect, useState } from "react";
import { bookingsApi } from "../api/bookings";

export default function BookingLogsModal({ booking, onClose }) {
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    bookingsApi
      .getLogs(booking.id)
      .then((res) => setLogs(res.data))
      .finally(() => setLoading(false));
  }, [booking.id]);

  return (
    <div className="fixed inset-0 bg-black/50 backdrop-blur-[2px] flex items-center justify-center z-50 p-4 modal-overlay">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-lg max-h-[80vh] overflow-y-auto modal-panel">
        <div className="flex justify-between items-center px-6 py-5 border-b border-gray-100 sticky top-0 bg-white rounded-t-2xl">
          <div>
            <p className="text-xs text-gray-400 font-medium">Nhật ký</p>
            <h2 className="text-lg font-bold text-gray-900 font-display">
              Phòng {booking.roomNumber}
            </h2>
          </div>
          <button
            onClick={onClose}
            className="w-8 h-8 rounded-lg flex items-center justify-center text-gray-400 hover:bg-gray-100 hover:text-gray-700 transition-colors"
          >
            <svg viewBox="0 0 24 24" className="w-4.5 h-4.5" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M18 6 6 18M6 6l12 12" strokeLinecap="round" />
            </svg>
          </button>
        </div>

        <div className="p-6">
          {loading && (
            <div className="flex items-center gap-2 text-gray-500 text-sm">
              <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
                <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" opacity="0.25" />
                <path d="M22 12a10 10 0 0 1-10 10" stroke="currentColor" strokeWidth="4" strokeLinecap="round" />
              </svg>
              Đang tải...
            </div>
          )}

          <div className="space-y-4">
            {logs.map((log, i) => (
              <div key={i} className="relative pl-5 border-l-2 border-[#c9a24b]/40">
                <span className="absolute -left-[5px] top-1 w-2 h-2 rounded-full bg-[#c9a24b]" />
                <p className="text-sm font-semibold text-gray-800">{log.action}</p>
                <p className="text-xs text-gray-500 mt-0.5">
                  {log.performedByName} — {new Date(log.createdAt).toLocaleString("vi-VN")}
                </p>
                {log.note && <p className="text-xs text-gray-600 mt-1.5 bg-gray-50 rounded-lg px-2.5 py-1.5">{log.note}</p>}
              </div>
            ))}
            {!loading && logs.length === 0 && (
              <p className="text-gray-400 text-sm text-center py-6">Chưa có nhật ký nào.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}