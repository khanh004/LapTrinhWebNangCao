import { useEffect, useState } from "react";
import Layout, { StatusBadge } from "../components/Layout";
import { roomsApi } from "../api/rooms";
import { useAuth } from "../context/AuthContext";

export default function Housekeeping() {
  const { user } = useAuth();
  const [rooms, setRooms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [claimingId, setClaimingId] = useState(null);
  const [processingId, setProcessingId] = useState(null);

  const load = async () => {
    try {
      const res = await roomsApi.getAll({ status: "CLEANING" });
      setRooms(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
    const interval = setInterval(load, 15000); // tự cập nhật để thấy phòng người khác vừa nhận
    return () => clearInterval(interval);
  }, []);

  const handleClaim = async (roomId) => {
    setError("");
    setClaimingId(roomId);
    try {
      await roomsApi.claimCleaning(roomId);
      await load();
    } catch (err) {
      setError(err.response?.data || "Nhận dọn thất bại - có thể người khác vừa nhận trước.");
    } finally {
      setClaimingId(null);
    }
  };

  const handleConfirm = async (roomId) => {
    setError("");
    setProcessingId(roomId);
    try {
      await roomsApi.confirmCleaning(roomId);
      await load();
    } catch (err) {
      setError(err.response?.data || "Xác nhận thất bại.");
    } finally {
      setProcessingId(null);
    }
  };

  const handleRelease = async (roomId) => {
    setError("");
    setProcessingId(roomId);
    try {
      await roomsApi.releaseCleaningClaim(roomId);
      await load();
    } catch (err) {
      setError(err.response?.data || "Hủy nhận thất bại.");
    } finally {
      setProcessingId(null);
    }
  };

  const unclaimed = rooms.filter((r) => !r.cleaningClaimedByEmployeeId);
  const mine = rooms.filter((r) => r.cleaningClaimedByEmployeeId === user?.employeeId);

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
            <div key={i} className="card h-32 animate-pulse bg-gray-50" />
          ))}
        </div>
      ) : (
        <>
          {mine.length > 0 && (
            <>
              <p className="text-sm font-semibold text-gray-700 mb-3 flex items-center gap-1.5">
                <span className="w-2 h-2 rounded-full bg-blue-500" />
                Phòng bạn đang dọn ({mine.length})
              </p>
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mb-8">
                {mine.map((room) => (
                  <div key={room.id} className="card p-4 fade-up border-2 border-blue-200">
                    <div className="flex justify-between items-start mb-3">
                      <div>
                        <p className="font-bold text-gray-900">Phòng {room.roomNumber}</p>
                        <p className="text-xs text-gray-400">
                          {room.roomTypeName} · Tầng {room.floor ?? "-"}
                        </p>
                      </div>
                      <StatusBadge status={room.status} />
                    </div>
                    <div className="flex gap-2">
                      <button
                        onClick={() => handleConfirm(room.id)}
                        disabled={processingId === room.id}
                        className="btn btn-primary flex-1 py-2 text-sm disabled:opacity-50"
                      >
                        Xác nhận dọn xong
                      </button>
                      <button
                        onClick={() => handleRelease(room.id)}
                        disabled={processingId === room.id}
                        className="btn btn-outline px-3 py-2 text-sm disabled:opacity-50"
                        title="Hủy nhận, trả phòng về danh sách chung"
                      >
                        Hủy nhận
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </>
          )}

          <p className="text-sm font-semibold text-gray-700 mb-3">
            Phòng chưa ai nhận dọn ({unclaimed.length})
          </p>

          {unclaimed.length === 0 ? (
            <div className="card flex flex-col items-center justify-center text-center py-16 px-6">
              <div className="w-16 h-16 rounded-2xl bg-emerald-50 flex items-center justify-center mb-4">
                <svg viewBox="0 0 24 24" className="w-8 h-8 text-emerald-500" fill="none" stroke="currentColor" strokeWidth="1.6">
                  <path d="M20 6 9 17l-5-5" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
              </div>
              <h2 className="text-lg font-bold text-gray-900 font-display mb-1.5">
                Không còn phòng nào chờ nhận
              </h2>
              <p className="text-sm text-gray-500 max-w-sm">
                Mọi phòng cần dọn đều đã có người nhận hoặc đã hoàn tất.
              </p>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {unclaimed.map((room) => (
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
                    onClick={() => handleClaim(room.id)}
                    disabled={claimingId === room.id}
                    className="btn btn-gold w-full py-2 text-sm disabled:opacity-50"
                  >
                    {claimingId === room.id ? "Đang nhận..." : "Nhận dọn phòng này"}
                  </button>
                </div>
              ))}
            </div>
          )}
        </>
      )}
    </Layout>
  );
}