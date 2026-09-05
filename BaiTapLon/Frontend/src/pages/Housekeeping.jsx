import { useEffect, useState } from "react";
import Layout, { StatusBadge } from "../components/Layout";
import { roomsApi } from "../api/rooms";

export default function Housekeeping() {
  const [rooms, setRooms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [confirmingId, setConfirmingId] = useState(null);

  const load = async () => {
    setLoading(true);
    try {
      const res = await roomsApi.getAll({ status: "CLEANING" });
      setRooms(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleConfirm = async (roomId) => {
    setError("");
    setConfirmingId(roomId);
    try {
      await roomsApi.confirmCleaning(roomId);
      await load();
    } catch (err) {
      setError(err.response?.data || "Xác nhận thất bại.");
    } finally {
      setConfirmingId(null);
    }
  };

  return (
    <Layout title="Danh sách phòng cần dọn dẹp">
      {error && (
        <div className="flex items-start gap-2 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg px-3.5 py-2.5 mb-4">
          <svg viewBox="0 0 24 24" className="w-4 h-4 mt-0.5 shrink-0" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="12" cy="12" r="10" />
            <path d="M12 8v5M12 16h.01" strokeLinecap="round" />
          </svg>
          <span>{error}</span>
        </div>
      )}

      {loading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="card p-4 h-32 animate-pulse bg-gray-50" />
          ))}
        </div>
      ) : rooms.length === 0 ? (
        <div className="card flex flex-col items-center justify-center text-center py-20 px-6">
          <div className="w-16 h-16 rounded-2xl bg-emerald-50 flex items-center justify-center mb-4">
            <svg viewBox="0 0 24 24" className="w-8 h-8 text-emerald-500" fill="none" stroke="currentColor" strokeWidth="1.6">
              <path d="M20 6 9 17l-5-5" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
          </div>
          <h2 className="text-lg font-bold text-gray-900 font-display mb-1.5">
            Không có phòng nào cần dọn
          </h2>
          <p className="text-sm text-gray-500 max-w-sm">
            Tất cả phòng hiện đã sẵn sàng hoặc đang có khách. Danh sách sẽ tự hiện khi có phòng vừa
            được trả.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {rooms.map((room) => (
            <div key={room.id} className="card p-4 fade-up">
              <div className="flex justify-between items-start mb-3">
                <div>
                  <p className="font-bold text-gray-900">Phòng {room.roomNumber}</p>
                  <p className="text-xs text-gray-400">
                    {room.roomTypeName} · Tầng {room.floor ?? "-"}
                  </p>
                </div>
                <StatusBadge status={room.status} />
              </div>
              <button
                onClick={() => handleConfirm(room.id)}
                disabled={confirmingId === room.id}
                className="btn btn-primary w-full py-2 text-sm disabled:opacity-50"
              >
                {confirmingId === room.id ? "Đang xác nhận..." : "Xác nhận đã dọn xong"}
              </button>
            </div>
          ))}
        </div>
      )}
    </Layout>
  );
}