import { useState } from "react";
import Layout, { StatusBadge } from "../components/Layout";
import { customersApi } from "../api/customers";
import { bookingsApi } from "../api/bookings";

export default function CustomerLookup() {
  const [search, setSearch] = useState("");
  const [customers, setCustomers] = useState([]);
  const [selectedCustomer, setSelectedCustomer] = useState(null);
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleSearch = async (e) => {
    e.preventDefault();
    if (!search.trim()) return;
    setError("");
    setSelectedCustomer(null);
    setBookings([]);
    try {
      const res = await customersApi.search(search);
      setCustomers(res.data);
      if (res.data.length === 0) setError("Không tìm thấy khách hàng nào.");
    } catch {
      setError("Tìm kiếm thất bại.");
    }
  };

  const selectCustomer = async (customer) => {
    setSelectedCustomer(customer);
    setLoading(true);
    try {
      const res = await bookingsApi.getByCustomer(customer.id);
      setBookings(res.data);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Layout title="Tra cứu khách hàng">
      <div className="card p-4 mb-5">
        <form onSubmit={handleSearch} className="flex gap-2">
          <input
            className="input flex-1 px-3 py-2 text-sm"
            placeholder="Nhập tên, số điện thoại hoặc CCCD của khách..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <button type="submit" className="btn-search px-5 py-2 text-sm rounded-lg">
            Tìm
          </button>
        </form>
      </div>

      {error && (
        <div className="flex items-start gap-2 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg px-3.5 py-2.5 mb-4">
          <svg viewBox="0 0 24 24" className="w-4 h-4 mt-0.5 shrink-0" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="12" cy="12" r="10" />
            <path d="M12 8v5M12 16h.01" strokeLinecap="round" />
          </svg>
          <span>{error}</span>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div className="lg:col-span-1">
          <div className="card divide-y divide-gray-100 overflow-hidden">
            {customers.map((c) => (
              <div
                key={c.id}
                onClick={() => selectCustomer(c)}
                className={`px-4 py-3 cursor-pointer hover:bg-gray-50 transition-colors ${
                  selectedCustomer?.id === c.id ? "bg-blue-50" : ""
                }`}
              >
                <p className="font-semibold text-gray-800 text-sm">{c.fullName}</p>
                <p className="text-xs text-gray-500">{c.phone || "(chưa có SĐT)"}</p>
              </div>
            ))}
            {customers.length === 0 && (
              <p className="text-center text-gray-400 text-sm py-8 px-4">
                Nhập từ khóa và bấm Tìm để tra cứu.
              </p>
            )}
          </div>
        </div>

        <div className="lg:col-span-2">
          {!selectedCustomer && (
            <div className="card flex items-center justify-center text-center py-16 px-6 text-gray-400 text-sm">
              Chọn 1 khách hàng bên trái để xem lịch sử đặt phòng.
            </div>
          )}

          {selectedCustomer && (
            <div className="card overflow-hidden">
              <div className="px-4 py-3 border-b border-gray-100 bg-gray-50/60">
                <p className="font-semibold text-gray-800">{selectedCustomer.fullName}</p>
                <p className="text-xs text-gray-500">
                  {selectedCustomer.phone}
                  {selectedCustomer.identityNumber ? ` · CCCD: ${selectedCustomer.identityNumber}` : ""}
                </p>
              </div>

              {loading ? (
                <div className="p-6 space-y-2">
                  {Array.from({ length: 3 }).map((_, i) => (
                    <div key={i} className="h-12 bg-gray-50 rounded-lg animate-pulse" />
                  ))}
                </div>
              ) : (
                <div className="divide-y divide-gray-100">
                  {bookings.map((b) => (
                    <div key={b.id} className="px-4 py-3 flex items-center justify-between">
                      <div>
                        <p className="text-sm font-semibold text-gray-800">
                          Phòng {b.roomNumber} · {b.bookingCode}
                        </p>
                        <p className="text-xs text-gray-500">
                          {b.checkInDate} → {b.checkOutDate}
                        </p>
                      </div>
                      <div className="flex flex-col items-end gap-1">
                        <StatusBadge status={b.status} />
                        <div className="flex items-center gap-1 text-[10px] text-gray-400">
                          <span>Phòng hiện:</span>
                          <StatusBadge status={b.roomStatus} />
                        </div>
                      </div>
                    </div>
                  ))}

                  {bookings.length === 0 && (
                    <p className="text-center text-gray-400 text-sm py-8">
                      Khách hàng này chưa có đặt phòng nào.
                    </p>
                  )}
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
}