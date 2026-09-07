import { useEffect, useState } from "react";
import Layout, { StatusBadge } from "../components/Layout";
import { roomsApi } from "../api/rooms";
import RoomDetailModal from "../components/RoomDetailModal";
import { getRoomImageUrl } from "../utils/roomImage";

const STATUS_FILTERS = [
  { value: "", label: "Tất cả" },
  { value: "AVAILABLE", label: "Trống" },
  { value: "RESERVED", label: "Đã đặt trước" },
  { value: "OCCUPIED", label: "Đang có khách" },
  { value: "CLEANING", label: "Đang dọn dẹp" },
  { value: "MAINTENANCE", label: "Bảo trì" },
];

const todayStr = () => new Date().toISOString().split("T")[0];
const tomorrowStr = () => {
  const d = new Date();
  d.setDate(d.getDate() + 1);
  return d.toISOString().split("T")[0];
};

export default function Reception() {
  const [rooms, setRooms] = useState([]);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [loading, setLoading] = useState(true);
  const [selectedRoomId, setSelectedRoomId] = useState(null);

  const [showRangeSearch, setShowRangeSearch] = useState(false);
  const [rangeCheckIn, setRangeCheckIn] = useState(todayStr());
  const [rangeCheckOut, setRangeCheckOut] = useState(tomorrowStr());
  const [rangeResults, setRangeResults] = useState(null);
  const [rangeLoading, setRangeLoading] = useState(false);
  const [rangeError, setRangeError] = useState("");

  const loadRooms = async () => {
    setLoading(true);
    try {
      const res = await roomsApi.getAll({
        search: search || undefined,
        status: statusFilter || undefined,
      });
      setRooms(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRooms();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [statusFilter]);

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    loadRooms();
  };

  const handleRangeSearch = async (e) => {
    e?.preventDefault?.();
    setRangeError("");
    if (rangeCheckOut <= rangeCheckIn) {
      setRangeError("Ngày trả phải sau ngày nhận.");
      return;
    }
    setRangeLoading(true);
    try {
      const res = await roomsApi.searchAvailable(rangeCheckIn, rangeCheckOut);
      setRangeResults(res.data);
    } catch (err) {
      setRangeError(err.response?.data || "Tìm kiếm thất bại.");
    } finally {
      setRangeLoading(false);
    }
  };

  const clearRangeSearch = () => {
    setRangeResults(null);
    setShowRangeSearch(false);
  };

  const displayedRooms = rangeResults !== null ? rangeResults : rooms;

  return (
    <Layout title="Bảng phòng">
      <div className="card p-4 mb-4 flex flex-wrap items-center gap-3">
        <form onSubmit={handleSearchSubmit} className="flex gap-2">
          <input
            className="input px-3 py-2 text-sm w-48"
            placeholder="Tìm theo số phòng..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <button type="submit" className="btn-search px-4 py-2 text-sm rounded-lg">
            Tìm
          </button>
        </form>

        <button
          onClick={() => setShowRangeSearch((v) => !v)}
          className={`btn px-4 py-2 text-sm ${showRangeSearch ? "btn-primary" : "btn-outline"}`}
        >
          <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
            <rect x="3" y="4" width="18" height="17" rx="2" />
            <path d="M8 2v4M16 2v4M3 10h18" strokeLinecap="round" />
          </svg>
          Tìm phòng trống theo lịch
        </button>

        <div className="flex flex-wrap gap-1.5 ml-auto">
          {STATUS_FILTERS.map((f) => (
            <button
              key={f.value}
              onClick={() => setStatusFilter(f.value)}
              className={`filter-chip px-3 py-1.5 rounded-lg text-xs ${
                statusFilter === f.value ? "filter-chip-active" : "filter-chip-inactive"
              }`}
            >
              {f.label}
            </button>
          ))}
        </div>
      </div>

      {showRangeSearch && (
        <div className="card p-4 mb-5 fade-up">
          <p className="text-sm font-semibold text-gray-700 mb-3">
            Tìm phòng còn trống trong khoảng thời gian
          </p>
          <form onSubmit={handleRangeSearch} className="flex flex-wrap items-end gap-3">
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1.5">Từ ngày</label>
              <input
                type="date"
                className="input px-3 py-2 text-sm"
                value={rangeCheckIn}
                onChange={(e) => setRangeCheckIn(e.target.value)}
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1.5">Đến ngày</label>
              <input
                type="date"
                className="input px-3 py-2 text-sm"
                value={rangeCheckOut}
                onChange={(e) => setRangeCheckOut(e.target.value)}
              />
            </div>
            <button type="submit" disabled={rangeLoading} className="btn btn-primary px-5 py-2 text-sm disabled:opacity-50">
              {rangeLoading ? "Đang tìm..." : "Tìm phòng trống"}
            </button>
            {rangeResults !== null && (
              <button type="button" onClick={clearRangeSearch} className="btn btn-ghost px-4 py-2 text-sm">
                Bỏ tìm kiếm, xem tất cả
              </button>
            )}
          </form>
          {rangeError && <p className="text-red-600 text-sm mt-2">{rangeError}</p>}
        </div>
      )}

      {rangeResults !== null && (
        <div className="flex items-center gap-2 bg-emerald-50 border border-emerald-200 text-emerald-700 text-sm rounded-lg px-3.5 py-2.5 mb-4">
          <svg viewBox="0 0 24 24" className="w-4 h-4 shrink-0" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M20 6 9 17l-5-5" strokeLinecap="round" strokeLinejoin="round" />
          </svg>
          <span>
            Có <strong>{rangeResults.length}</strong> phòng trống từ <strong>{rangeCheckIn}</strong> đến{" "}
            <strong>{rangeCheckOut}</strong>.
          </span>
        </div>
      )}

      {(loading && rangeResults === null) || rangeLoading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="card h-56 animate-pulse bg-gray-50" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          {displayedRooms.map((room) => (
            <div
              key={room.id}
              onClick={() => setSelectedRoomId(room.id)}
              className="card-interactive overflow-hidden fade-up"
            >
              <div className="relative">
                <img
                  src={getRoomImageUrl(room.roomTypeName)}
                  alt={`Phòng ${room.roomNumber}`}
                  className="w-full h-32 object-cover"
                  loading="lazy"
                />
                <div className="absolute top-2 right-2">
                  <StatusBadge status={room.status} />
                </div>
              </div>

              <div className="p-4">
                <p className="font-bold text-gray-900 leading-tight">Phòng {room.roomNumber}</p>
                <p className="text-xs text-gray-400 mb-2">
                  {room.roomTypeName} · Tầng {room.floor ?? "-"}
                </p>

                {room.currentBookingCheckIn && (
                  <p className="text-xs text-gray-500 border-t border-gray-100 pt-2">
                    Đang giữ chỗ: {room.currentBookingCheckIn} → {room.currentBookingCheckOut}
                  </p>
                )}

                {room.waitingCustomers?.length > 0 && (
                  <p className="text-xs text-[#c9a24b] font-semibold mt-1.5 flex items-center gap-1">
                    <svg viewBox="0 0 24 24" className="w-3.5 h-3.5" fill="none" stroke="currentColor" strokeWidth="2">
                      <circle cx="12" cy="8" r="4" />
                      <path d="M4 21c0-4.4 3.6-8 8-8s8 3.6 8 8" strokeLinecap="round" />
                    </svg>
                    {room.waitingCustomers.length} khách đang chờ phòng này
                  </p>
                )}
              </div>
            </div>
          ))}

          {displayedRooms.length === 0 && (
            <div className="card col-span-full text-center text-gray-400 py-14">
              {rangeResults !== null
                ? "Không có phòng nào trống trong khoảng ngày này."
                : "Không tìm thấy phòng nào."}
            </div>
          )}
        </div>
      )}

      {selectedRoomId && (
        <RoomDetailModal
          roomId={selectedRoomId}
          initialCheckIn={rangeResults !== null ? rangeCheckIn : undefined}
          initialCheckOut={rangeResults !== null ? rangeCheckOut : undefined}
          onClose={() => setSelectedRoomId(null)}
          onChanged={() => {
            loadRooms();
            if (rangeResults !== null) handleRangeSearch();
          }}
        />
      )}
    </Layout>
  );
}