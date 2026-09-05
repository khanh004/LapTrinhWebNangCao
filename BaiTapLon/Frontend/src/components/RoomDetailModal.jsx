import { useEffect, useState } from "react";
import { roomsApi } from "../api/rooms";
import { bookingsApi } from "../api/bookings";
import { customersApi } from "../api/customers";
import { waitlistApi } from "../api/waitlist";
import { StatusBadge } from "./Layout";

const todayStr = () => new Date().toISOString().split("T")[0];
const tomorrowStr = () => {
  const d = new Date();
  d.setDate(d.getDate() + 1);
  return d.toISOString().split("T")[0];
};

export default function RoomDetailModal({ roomId, onClose, onChanged }) {
  const [checkIn, setCheckIn] = useState(todayStr());
  const [checkOut, setCheckOut] = useState(tomorrowStr());
  const [availability, setAvailability] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  // Tìm khách hàng
  const [customerSearch, setCustomerSearch] = useState("");
  const [customerResults, setCustomerResults] = useState([]);
  const [selectedCustomer, setSelectedCustomer] = useState(null);
  const [isNewCustomer, setIsNewCustomer] = useState(false);
  const [newCustomerName, setNewCustomerName] = useState("");
  const [newCustomerPhone, setNewCustomerPhone] = useState("");

  const loadAvailability = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await roomsApi.checkAvailability(roomId, checkIn, checkOut);
      setAvailability(res.data);
    } catch (err) {
      setError(err.response?.data || "Không kiểm tra được tình trạng phòng.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAvailability();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [checkIn, checkOut]);

  const searchCustomer = async () => {
    if (!customerSearch.trim()) return;
    const res = await customersApi.search(customerSearch);
    setCustomerResults(res.data);
  };

  const resetCustomerSelection = () => {
    setSelectedCustomer(null);
    setIsNewCustomer(false);
    setCustomerResults([]);
    setCustomerSearch("");
    setNewCustomerName("");
    setNewCustomerPhone("");
  };

  const handleCreateBooking = async () => {
    setError("");
    setSuccess("");

    let customerId = selectedCustomer?.id;

    try {
      if (isNewCustomer) {
        if (!newCustomerName.trim()) {
          setError("Vui lòng nhập họ tên khách hàng mới.");
          return;
        }
        const res = await customersApi.create({
          fullName: newCustomerName,
          phone: newCustomerPhone,
        });
        customerId = res.data.id;
      }

      if (!customerId) {
        setError("Vui lòng chọn hoặc tạo khách hàng trước.");
        return;
      }

      await bookingsApi.create({
        customerId,
        roomId,
        checkInDate: checkIn,
        checkOutDate: checkOut,
      });

      setSuccess("Đặt phòng thành công!");
      onChanged?.();
      setTimeout(onClose, 1000);
    } catch (err) {
      setError(err.response?.data || "Đặt phòng thất bại.");
    }
  };

  const handleJoinWaitlist = async () => {
    setError("");
    setSuccess("");

    let customerId = selectedCustomer?.id;

    try {
      const payload = { roomId, desiredCheckIn: checkIn, desiredCheckOut: checkOut };

      if (isNewCustomer) {
        if (!newCustomerName.trim()) {
          setError("Vui lòng nhập họ tên khách hàng mới.");
          return;
        }
        payload.newCustomer = { fullName: newCustomerName, phone: newCustomerPhone };
      } else {
        if (!customerId) {
          setError("Vui lòng chọn khách hàng trước.");
          return;
        }
        payload.customerId = customerId;
      }

      await waitlistApi.join(payload);
      setSuccess("Đã thêm khách vào hàng chờ!");
      onChanged?.();
      setTimeout(onClose, 1000);
    } catch (err) {
      setError(err.response?.data || "Thêm vào hàng chờ thất bại.");
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 backdrop-blur-[2px] flex items-center justify-center z-50 p-4 modal-overlay">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-lg max-h-[90vh] overflow-y-auto modal-panel">
        <div className="flex justify-between items-center px-6 py-5 border-b border-gray-100 sticky top-0 bg-white rounded-t-2xl z-10">
          <div>
            <p className="text-xs text-gray-400 font-medium">Chi tiết phòng</p>
            <h2 className="text-lg font-bold text-gray-900 font-display">
              Phòng {availability?.roomNumber ?? "..."}
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
          {availability && (
            <div className="mb-4">
              <StatusBadge status={availability.currentStatus} />
            </div>
          )}

          <div className="grid grid-cols-2 gap-3 mb-4">
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1.5">Ngày nhận phòng</label>
              <input
                type="date"
                className="input px-3 py-2 text-sm"
                value={checkIn}
                onChange={(e) => setCheckIn(e.target.value)}
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1.5">Ngày trả phòng</label>
              <input
                type="date"
                className="input px-3 py-2 text-sm"
                value={checkOut}
                onChange={(e) => setCheckOut(e.target.value)}
              />
            </div>
          </div>

          {loading && (
            <div className="flex items-center gap-2 text-gray-500 text-sm mb-4">
              <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
                <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" opacity="0.25" />
                <path d="M22 12a10 10 0 0 1-10 10" stroke="currentColor" strokeWidth="4" strokeLinecap="round" />
              </svg>
              Đang kiểm tra tình trạng phòng...
            </div>
          )}

          {!loading && availability && (
            <div className="bg-gray-50 rounded-xl p-3.5 mb-4 text-sm border border-gray-100">
              {availability.canBookImmediately && (
                <p className="text-emerald-700 font-medium flex items-center gap-1.5">
                  <svg viewBox="0 0 24 24" className="w-4 h-4 shrink-0" fill="none" stroke="currentColor" strokeWidth="2.5">
                    <path d="M20 6 9 17l-5-5" strokeLinecap="round" strokeLinejoin="round" />
                  </svg>
                  Phòng trống, có thể đặt ngay cho khoảng ngày này.
                </p>
              )}

              {availability.conflictUntil && (
                <p className="text-orange-700">
                  ⚠ Phòng đang có khách / đã có người đặt đến hết ngày{" "}
                  <strong>{availability.conflictUntil}</strong>. Nếu khách chấp nhận đợi, thêm vào
                  hàng chờ bên dưới.
                </p>
              )}

              {availability.estimatedCleaningReadyAt && (
                <p className="text-purple-700">
                  🧹 Phòng đang dọn dẹp, dự kiến sẵn sàng lúc{" "}
                  <strong>
                    {new Date(availability.estimatedCleaningReadyAt).toLocaleTimeString("vi-VN")}
                  </strong>{" "}
                  (thời gian ước lượng — trạng thái thực tế phụ thuộc lao công xác nhận).
                </p>
              )}

              {availability.currentStatus === "MAINTENANCE" && (
                <p className="text-red-700">✕ Phòng đang bảo trì, không thể đặt.</p>
              )}

              {availability.suggestedAlternativeRooms?.length > 0 && (
                <div className="mt-2">
                  <p className="text-gray-600 text-xs mb-1.5">Phòng thay thế cùng loại còn trống:</p>
                  <div className="flex flex-wrap gap-1.5">
                    {availability.suggestedAlternativeRooms.map((r) => (
                      <span
                        key={r.id}
                        className="bg-white border border-gray-200 px-2.5 py-1 rounded-lg text-xs font-medium text-gray-700"
                      >
                        Phòng {r.roomNumber}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}

          {/* ---------- Chọn khách hàng ---------- */}
          <div className="border-t border-gray-100 pt-4 mb-4">
            <p className="text-sm font-semibold text-gray-700 mb-2.5">Thông tin khách hàng</p>

            {!selectedCustomer && !isNewCustomer && (
              <>
                <div className="flex gap-2 mb-2">
                  <input
                    className="input flex-1 px-3 py-2 text-sm"
                    placeholder="Tìm theo tên/SĐT/CCCD..."
                    value={customerSearch}
                    onChange={(e) => setCustomerSearch(e.target.value)}
                    onKeyDown={(e) => e.key === "Enter" && searchCustomer()}
                  />
                  <button onClick={searchCustomer} className="btn btn-ghost px-4 py-2 text-sm">
                    Tìm
                  </button>
                </div>

                {customerResults.length > 0 && (
                  <div className="border border-gray-200 rounded-xl divide-y divide-gray-100 mb-2 max-h-32 overflow-y-auto">
                    {customerResults.map((c) => (
                      <div
                        key={c.id}
                        onClick={() => setSelectedCustomer(c)}
                        className="px-3 py-2 text-sm hover:bg-gray-50 cursor-pointer"
                      >
                        {c.fullName} — {c.phone || "(chưa có SĐT)"}
                      </div>
                    ))}
                  </div>
                )}

                <button
                  onClick={() => setIsNewCustomer(true)}
                  className="text-sm text-[#101a2f] font-medium hover:underline"
                >
                  + Khách hàng mới (chưa có trong hệ thống)
                </button>
              </>
            )}

            {selectedCustomer && (
              <div className="flex justify-between items-center bg-blue-50 rounded-xl px-3.5 py-2.5 text-sm border border-blue-100">
                <span>
                  Khách: <strong>{selectedCustomer.fullName}</strong>
                </span>
                <button onClick={resetCustomerSelection} className="text-xs text-gray-500 font-medium hover:text-gray-700">
                  Đổi
                </button>
              </div>
            )}

            {isNewCustomer && (
              <div className="space-y-2">
                <input
                  className="input px-3 py-2 text-sm"
                  placeholder="Họ tên khách mới *"
                  value={newCustomerName}
                  onChange={(e) => setNewCustomerName(e.target.value)}
                />
                <input
                  className="input px-3 py-2 text-sm"
                  placeholder="Số điện thoại"
                  value={newCustomerPhone}
                  onChange={(e) => setNewCustomerPhone(e.target.value)}
                />
                <button onClick={resetCustomerSelection} className="text-xs text-gray-500 hover:text-gray-700">
                  Hủy, chọn khách có sẵn thay vào
                </button>
              </div>
            )}
          </div>

          {error && (
            <div className="flex items-start gap-2 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg px-3.5 py-2.5 mb-3">
              <svg viewBox="0 0 24 24" className="w-4 h-4 mt-0.5 shrink-0" fill="none" stroke="currentColor" strokeWidth="2">
                <circle cx="12" cy="12" r="10" />
                <path d="M12 8v5M12 16h.01" strokeLinecap="round" />
              </svg>
              <span>{error}</span>
            </div>
          )}
          {success && (
            <div className="flex items-start gap-2 bg-emerald-50 border border-emerald-200 text-emerald-700 text-sm rounded-lg px-3.5 py-2.5 mb-3">
              <svg viewBox="0 0 24 24" className="w-4 h-4 mt-0.5 shrink-0" fill="none" stroke="currentColor" strokeWidth="2.5">
                <path d="M20 6 9 17l-5-5" strokeLinecap="round" strokeLinejoin="round" />
              </svg>
              <span>{success}</span>
            </div>
          )}

          <div className="flex gap-2">
            {availability?.canBookImmediately && (
              <button onClick={handleCreateBooking} className="btn btn-primary flex-1 py-2.5 text-sm">
                Đặt phòng ngay
              </button>
            )}

            {!availability?.canBookImmediately && availability?.currentStatus !== "MAINTENANCE" && (
              <button onClick={handleJoinWaitlist} className="btn btn-gold flex-1 py-2.5 text-sm">
                Thêm vào hàng chờ
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}