import { useEffect, useState } from "react";
import Layout, { StatusBadge } from "../components/Layout";
import { roomsApi } from "../api/rooms";
import RoomDetailModal from "../components/RoomDetailModal";

const STATUS_FILTERS = [
  { value: "", label: "Tất cả" },
  { value: "AVAILABLE", label: "Trống" },
  { value: "RESERVED", label: "Đã đặt trước" },
  { value: "OCCUPIED", label: "Đang có khách" },
  { value: "CLEANING", label: "Đang dọn dẹp" },
  { value: "MAINTENANCE", label: "Bảo trì" },
];

function RoomIcon() {
  return (
    <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6M9 9h.01M15 9h.01M9 13h.01M15 13h.01" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

export default function Reception() {
  const [rooms, setRooms] = useState([]);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [loading, setLoading] = useState(true);
  const [selectedRoomId, setSelectedRoomId] = useState(null);

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

  return (
    <Layout title="Bảng phòng">
      <div className="card p-4 mb-5 flex flex-wrap items-center gap-3">
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

      {loading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="card p-4 h-28 animate-pulse bg-gray-50" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          {rooms.map((room) => (
            <div
              key={room.id}
              onClick={() => setSelectedRoomId(room.id)}
              className="card p-4 cursor-pointer hover:shadow-md hover:-translate-y-0.5 transition-all fade-up"
            >
              <div className="flex justify-between items-start mb-3">
                <div className="flex items-center gap-2.5">
                  <div className="w-9 h-9 rounded-lg bg-gray-50 flex items-center justify-center text-gray-400">
                    <RoomIcon />
                  </div>
                  <div>
                    <p className="font-bold text-gray-900 leading-tight">Phòng {room.roomNumber}</p>
                    <p className="text-xs text-gray-400">
                      {room.roomTypeName} · Tầng {room.floor ?? "-"}
                    </p>
                  </div>
                </div>
                <StatusBadge status={room.status} />
              </div>

              {room.currentBookingCheckIn && (
                <p className="text-xs text-gray-500 border-t border-gray-100 pt-2 mt-1">
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
          ))}

          {rooms.length === 0 && (
            <div className="card col-span-full text-center text-gray-400 py-14">
              Không tìm thấy phòng nào.
            </div>
          )}
        </div>
      )}

      {selectedRoomId && (
        <RoomDetailModal
          roomId={selectedRoomId}
          onClose={() => setSelectedRoomId(null)}
          onChanged={loadRooms}
        />
      )}
    </Layout>
  );
}