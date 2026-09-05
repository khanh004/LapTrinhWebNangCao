import { useEffect, useState } from "react";
import Layout from "../../components/Layout";
import { roomTypesApi } from "../../api/rooms";

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

function RoomTypeFormModal({ roomType, onClose, onDone }) {
  const isEdit = !!roomType;
  const [name, setName] = useState(roomType?.name ?? "");
  const [description, setDescription] = useState(roomType?.description ?? "");
  const [pricePerNight, setPricePerNight] = useState(roomType?.pricePerNight ?? "");
  const [maxGuests, setMaxGuests] = useState(roomType?.maxGuests ?? 2);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (!name.trim()) return setError("Vui lòng nhập tên loại phòng.");
    if (!pricePerNight || Number(pricePerNight) <= 0)
      return setError("Vui lòng nhập giá thuê hợp lệ.");

    const payload = {
      name: name.trim(),
      description: description.trim() || null,
      pricePerNight: Number(pricePerNight),
      maxGuests: Number(maxGuests),
    };

    setLoading(true);
    try {
      if (isEdit) await roomTypesApi.update(roomType.id, payload);
      else await roomTypesApi.create(payload);
      onDone?.();
      onClose();
    } catch (err) {
      setError(err.response?.data || "Lưu loại phòng thất bại.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 backdrop-blur-[2px] flex items-center justify-center z-50 p-4 modal-overlay">
      <form onSubmit={handleSubmit} className="bg-white rounded-2xl shadow-2xl w-full max-w-md modal-panel">
        <div className="flex justify-between items-center px-6 py-5 border-b border-gray-100">
          <h2 className="text-lg font-bold text-gray-900 font-display">
            {isEdit ? "Sửa loại phòng" : "Thêm loại phòng"}
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
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Tên loại phòng *</label>
          <input
            className="input px-3 py-2 text-sm mb-4"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="VD: Standard, Deluxe, Suite..."
          />

          <label className="block text-xs font-medium text-gray-500 mb-1.5">Mô tả</label>
          <textarea
            className="input px-3 py-2 text-sm mb-4"
            rows={2}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />

          <div className="grid grid-cols-2 gap-3 mb-4">
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1.5">Giá / đêm (VNĐ) *</label>
              <input
                type="number"
                min={0}
                step={1000}
                className="input px-3 py-2 text-sm"
                value={pricePerNight}
                onChange={(e) => setPricePerNight(e.target.value)}
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1.5">Số khách tối đa *</label>
              <input
                type="number"
                min={1}
                className="input px-3 py-2 text-sm"
                value={maxGuests}
                onChange={(e) => setMaxGuests(e.target.value)}
              />
            </div>
          </div>

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

export default function RoomTypesManagement() {
  const [roomTypes, setRoomTypes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [modal, setModal] = useState(null); // { mode: 'create' | 'edit', roomType }

  const load = async () => {
    setLoading(true);
    try {
      const res = await roomTypesApi.getAll();
      setRoomTypes(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (rt) => {
    if (!window.confirm(`Xóa loại phòng "${rt.name}"? Thao tác này không thể hoàn tác.`)) return;
    setError("");
    try {
      await roomTypesApi.delete(rt.id);
      load();
    } catch (err) {
      setError(err.response?.data || "Không thể xóa loại phòng (có thể đang có phòng sử dụng).");
    }
  };

  return (
    <Layout title="Quản lý loại phòng">
      <div className="flex justify-between items-center mb-5">
        <p className="text-sm text-gray-500">{roomTypes.length} loại phòng</p>
        <button onClick={() => setModal({ mode: "create" })} className="btn btn-gold px-4 py-2 text-sm">
          <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2.2">
            <path d="M12 5v14M5 12h14" strokeLinecap="round" />
          </svg>
          Thêm loại phòng
        </button>
      </div>

      <ErrorBanner>{error}</ErrorBanner>

      {loading ? (
        <div className="card p-6 space-y-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="h-10 bg-gray-50 rounded-lg animate-pulse" />
          ))}
        </div>
      ) : (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50/80 text-gray-500 text-xs uppercase tracking-wide">
                <tr>
                  <th className="text-left px-4 py-3 font-semibold">Tên</th>
                  <th className="text-left px-4 py-3 font-semibold">Mô tả</th>
                  <th className="text-left px-4 py-3 font-semibold">Giá / đêm</th>
                  <th className="text-left px-4 py-3 font-semibold">Số khách tối đa</th>
                  <th className="text-left px-4 py-3 font-semibold">Hành động</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {roomTypes.map((rt) => (
                  <tr key={rt.id} className="hover:bg-gray-50/60 transition-colors">
                    <td className="px-4 py-3 font-semibold text-gray-800">{rt.name}</td>
                    <td className="px-4 py-3 text-gray-500">{rt.description || "—"}</td>
                    <td className="px-4 py-3 font-semibold text-[#101a2f]">
                      {rt.pricePerNight.toLocaleString("vi-VN")}đ
                    </td>
                    <td className="px-4 py-3 text-gray-600">{rt.maxGuests}</td>
                    <td className="px-4 py-3">
                      <div className="flex gap-1.5">
                        <button
                          onClick={() => setModal({ mode: "edit", roomType: rt })}
                          className="chip-btn chip-btn-blue text-xs font-semibold px-2.5 py-1.5 rounded-lg"
                        >
                          Sửa
                        </button>
                        <button
                          onClick={() => handleDelete(rt)}
                          className="chip-btn chip-btn-red text-xs font-semibold px-2.5 py-1.5 rounded-lg"
                        >
                          Xóa
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}

                {roomTypes.length === 0 && (
                  <tr>
                    <td colSpan={5} className="text-center text-gray-400 py-14">
                      Chưa có loại phòng nào.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {modal && (
        <RoomTypeFormModal
          roomType={modal.mode === "edit" ? modal.roomType : null}
          onClose={() => setModal(null)}
          onDone={load}
        />
      )}
    </Layout>
  );
}