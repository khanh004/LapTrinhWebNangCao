import { useEffect, useState } from "react";
import Layout, { StatusBadge } from "../../components/Layout";
import { roomsApi, roomTypesApi } from "../../api/rooms";

const STATUS_OPTIONS = ["AVAILABLE", "RESERVED", "OCCUPIED", "CLEANING", "MAINTENANCE"];

function ErrorBanner({ children }) {
  if (!children) return null;
  return (
    <div className="flex items-start gap-2 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg px-3.5 py-2.5 mb-4">
      <svg viewBox="0 0 24 24" className="w-4 h-4 mt-0.5 shrink-0" fill="none" stroke="currentColor" strokeWidth="2">
        <circle cx="12" cy="12" r="10" />
        <path d="M12 8v5M12 16h.01" strokeLinecap="round" />
      </svg>
      <span>{children}</span>
    </div>
  );
}

function RoomFormModal({ room, roomTypes, onClose, onDone }) {
  const isEdit = !!room;
  const [roomNumber, setRoomNumber] = useState(room?.roomNumber ?? "");
  const [roomTypeId, setRoomTypeId] = useState(room?.roomTypeId ?? roomTypes?.[0]?.id ?? "");
  const [floor, setFloor] = useState(room?.floor ?? "");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (!roomNumber.trim()) return setError("Vui lòng nhập số phòng.");
    if (!roomTypeId) return setError("Vui lòng chọn loại phòng.");

    const payload = {
      roomNumber: roomNumber.trim(),
      roomTypeId,
      floor: floor === "" ? null : Number(floor),
    };

    setLoading(true);
    try {
      if (isEdit) await roomsApi.update(room.id, payload);
      else await roomsApi.create(payload);
      onDone?.();
      onClose();
    } catch (err) {
      setError(err.response?.data || "Lưu phòng thất bại.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 backdrop-blur-[2px] flex items-center justify-center z-50 p-4 modal-overlay">
      <form onSubmit={handleSubmit} className="bg-white rounded-2xl shadow-2xl w-full max-w-md modal-panel">
        <div className="flex justify-between items-center px-6 py-5 border-b border-gray-100">
          <h2 className="text-lg font-bold text-gray-900 font-display">
            {isEdit ? "Sửa phòng" : "Thêm phòng"}
          </h2>
          <button
            type="button"
            onClick={onClose}
            className="w-8 h-8 rounded-lg flex items-center justify-center text-gray-400 hover:bg-gray-100 hover:text-gray-700 transition-colors"
          >
            <svg viewBox="0 0 24 24" className="w-4.5 h-4.5" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M18 6 6 18M6 6l12 12" strokeLinecap="round" />
            </svg>
          </button>
        </div>

        <div className="p-6">
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Số phòng *</label>
          <input
            className="input px-3 py-2 text-sm mb-4"
            value={roomNumber}
            onChange={(e) => setRoomNumber(e.target.value)}
            placeholder="VD: 101, 202..."
          />

          <label className="block text-xs font-medium text-gray-500 mb-1.5">Loại phòng *</label>
          <select
            className="input px-3 py-2 text-sm mb-4"
            value={roomTypeId}
            onChange={(e) => setRoomTypeId(e.target.value)}
          >
            {roomTypes.map((rt) => (
              <option key={rt.id} value={rt.id}>
                {rt.name} — {rt.pricePerNight.toLocaleString("vi-VN")}đ/đêm
              </option>
            ))}
          </select>

          <label className="block text-xs font-medium text-gray-500 mb-1.5">Tầng</label>
          <input
            type="number"
            className="input px-3 py-2 text-sm mb-4"
            value={floor}
            onChange={(e) => setFloor(e.target.value)}
          />

          {isEdit && (
            <p className="text-xs text-gray-400 mb-4">
              Trạng thái phòng hiện tại được đổi trực tiếp ở bảng danh sách, không sửa ở đây.
            </p>
          )}

          <ErrorBanner>{error}</ErrorBanner>

          <div className="flex gap-2">
            <button type="button" onClick={onClose} className="btn btn-outline flex-1 py-2.5 text-sm">
              Hủy
            </button>
            <button type="submit" disabled={loading} className="btn btn-primary flex-1 py-2.5 text-sm">
              {loading ? "Đang lưu..." : "Lưu"}
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}

export default function RoomsManagement() {
  const [rooms, setRooms] = useState([]);
  const [roomTypes, setRoomTypes] = useState([]);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [modal, setModal] = useState(null); // { mode: 'create' | 'edit', room }

  const load = async () => {
    setLoading(true);
    try {
      const [roomsRes, typesRes] = await Promise.all([
        roomsApi.getAll({ search: search || undefined, status: statusFilter || undefined }),
        roomTypesApi.getAll(),
      ]);
      setRooms(roomsRes.data);
      setRoomTypes(typesRes.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [statusFilter]);

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    load();
  };

  const handleStatusChange = async (room, status) => {
    setError("");
    try {
      await roomsApi.updateStatus(room.id, status);
      load();
    } catch (err) {
      setError(err.response?.data || "Đổi trạng thái phòng thất bại.");
    }
  };

  const handleDelete = async (room) => {
    if (!window.confirm(`Xóa phòng ${room.roomNumber}? Thao tác này không thể hoàn tác.`)) return;
    setError("");
    try {
      await roomsApi.delete(room.id);
      load();
    } catch (err) {
      setError(err.response?.data || "Không thể xóa phòng (có thể đang có đặt phòng liên quan).");
    }
  };

  return (
    <Layout title="Quản lý phòng">
      <div className="card p-4 mb-5 flex flex-wrap items-center gap-3">
        <form onSubmit={handleSearchSubmit} className="flex gap-2">
          <input
            className="input px-3 py-2 text-sm w-44"
            placeholder="Tìm theo số phòng..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <button type="submit" className="btn-search px-4 py-2 text-sm rounded-lg">
            Tìm
          </button>
        </form>

        <select
          className="input px-3 py-2 text-sm w-44"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          <option value="">Tất cả trạng thái</option>
          {STATUS_OPTIONS.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>

        <button
          onClick={() => setModal({ mode: "create" })}
          disabled={roomTypes.length === 0}
          className="btn btn-gold px-4 py-2 text-sm ml-auto disabled:opacity-50"
        >
          <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2.2">
            <path d="M12 5v14M5 12h14" strokeLinecap="round" />
          </svg>
          Thêm phòng
        </button>
      </div>

      {roomTypes.length === 0 && !loading && (
        <div className="flex items-start gap-2 bg-amber-50 border border-amber-200 text-amber-800 text-sm rounded-lg px-3.5 py-2.5 mb-4">
          <span>⚠ Chưa có loại phòng nào — hãy tạo loại phòng trước khi thêm phòng mới.</span>
        </div>
      )}

      <ErrorBanner>{error}</ErrorBanner>

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
                  <th className="text-left px-4 py-3 font-semibold">Số phòng</th>
                  <th className="text-left px-4 py-3 font-semibold">Loại phòng</th>
                  <th className="text-left px-4 py-3 font-semibold">Tầng</th>
                  <th className="text-left px-4 py-3 font-semibold">Trạng thái</th>
                  <th className="text-left px-4 py-3 font-semibold">Hành động</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {rooms.map((room) => (
                  <tr key={room.id} className="hover:bg-gray-50/60 transition-colors">
                    <td className="px-4 py-3 font-semibold text-gray-800">{room.roomNumber}</td>
                    <td className="px-4 py-3 text-gray-600">{room.roomTypeName}</td>
                    <td className="px-4 py-3 text-gray-500">{room.floor ?? "—"}</td>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2">
                        <StatusBadge status={room.status} />
                        <select
                          className="border border-gray-200 rounded-md text-xs px-1.5 py-1 text-gray-500 focus:outline-none focus:ring-2 focus:ring-[#101a2f]/10"
                          value={room.status}
                          onChange={(e) => handleStatusChange(room, e.target.value)}
                        >
                          {STATUS_OPTIONS.map((s) => (
                            <option key={s} value={s}>
                              {s}
                            </option>
                          ))}
                        </select>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex gap-1.5">
                        <button
                          onClick={() => setModal({ mode: "edit", room })}
                          className="chip-btn chip-btn-blue text-xs font-semibold px-2.5 py-1.5 rounded-lg"
                        >
                          Sửa
                        </button>
                        <button
                          onClick={() => handleDelete(room)}
                          className="chip-btn chip-btn-red text-xs font-semibold px-2.5 py-1.5 rounded-lg"
                        >
                          Xóa
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}

                {rooms.length === 0 && (
                  <tr>
                    <td colSpan={5} className="text-center text-gray-400 py-14">
                      Không tìm thấy phòng nào.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {modal && (
        <RoomFormModal
          room={modal.mode === "edit" ? modal.room : null}
          roomTypes={roomTypes}
          onClose={() => setModal(null)}
          onDone={load}
        />
      )}
    </Layout>
  );
}