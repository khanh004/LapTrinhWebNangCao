import { useState } from "react";
import { bookingsApi } from "../api/bookings";

export default function CheckOutModal({ booking, onClose, onDone }) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [result, setResult] = useState(null);

  const handleCheckOut = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await bookingsApi.checkOut(booking.id, {});
      setResult(res.data);
    } catch (err) {
      setError(err.response?.data || "Trả phòng thất bại.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 backdrop-blur-[2px] flex items-center justify-center z-50 p-4 modal-overlay">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md modal-panel">
        <div className="px-6 py-5 border-b border-gray-100">
          <p className="text-xs text-gray-400 font-medium">Trả phòng</p>
          <h2 className="text-lg font-bold text-gray-900 font-display">Phòng {booking.roomNumber}</h2>
          <p className="text-sm text-gray-500 mt-0.5">Khách: {booking.customerName}</p>
        </div>

        <div className="p-6">
          {!result && (
            <>
              <p className="text-sm text-gray-600 mb-5">
                Hệ thống sẽ tự tính tiền theo thời điểm trả phòng hiện tại (sớm/đúng giờ/trễ giờ/trễ
                ngày) và tạo hóa đơn.
              </p>
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
                <button onClick={handleCheckOut} disabled={loading} className="btn btn-primary flex-1 py-2.5 text-sm">
                  {loading ? "Đang xử lý..." : "Xác nhận trả phòng"}
                </button>
              </div>
            </>
          )}

          {result && (
            <>
              <div className="bg-gray-50 border border-gray-100 rounded-xl p-4 mb-5">
                <p className="text-sm font-semibold text-gray-800 mb-2">Chi tiết hóa đơn:</p>
                <pre className="text-sm text-gray-700 whitespace-pre-wrap font-sans">
                  {result.invoiceDescription}
                </pre>
              </div>
              <button
                onClick={() => {
                  onDone?.();
                  onClose();
                }}
                className="btn w-full py-2.5 text-sm bg-emerald-600 text-white hover:bg-emerald-700"
              >
                Đóng
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  );
}