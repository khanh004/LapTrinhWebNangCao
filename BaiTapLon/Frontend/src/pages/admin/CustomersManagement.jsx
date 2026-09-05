import { useEffect, useState } from "react";
import Layout from "../../components/Layout";
import { customersApi } from "../../api/customers";

const GENDER_LABELS = { MALE: "Nam", FEMALE: "Nữ", OTHER: "Khác" };

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

function CustomerFormModal({ customer, onClose, onDone }) {
  const isEdit = !!customer;
  const [fullName, setFullName] = useState(customer?.fullName ?? "");
  const [phone, setPhone] = useState(customer?.phone ?? "");
  const [email, setEmail] = useState(customer?.email ?? "");
  const [identityNumber, setIdentityNumber] = useState(customer?.identityNumber ?? "");
  const [dateOfBirth, setDateOfBirth] = useState(customer?.dateOfBirth ?? "");
  const [gender, setGender] = useState(customer?.gender ?? "");
  const [address, setAddress] = useState(customer?.address ?? "");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (!fullName.trim()) return setError("Vui lòng nhập họ tên khách hàng.");

    const payload = {
      fullName: fullName.trim(),
      phone: phone.trim() || null,
      email: email.trim() || null,
      identityNumber: identityNumber.trim() || null,
      dateOfBirth: dateOfBirth || null,
      gender: gender || null,
      address: address.trim() || null,
    };

    setLoading(true);
    try {
      if (isEdit) await customersApi.update(customer.id, payload);
      else await customersApi.create(payload);
      onDone?.();
      onClose();
    } catch (err) {
      setError(err.response?.data || "Lưu khách hàng thất bại.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 backdrop-blur-[2px] flex items-center justify-center z-50 p-4 modal-overlay">
      <form
        onSubmit={handleSubmit}
        className="bg-white rounded-2xl shadow-2xl w-full max-w-lg max-h-[90vh] overflow-y-auto modal-panel"
      >
        <div className="flex justify-between items-center px-6 py-5 border-b border-gray-100 sticky top-0 bg-white rounded-t-2xl z-10">
          <h2 className="text-lg font-bold text-gray-900 font-display">
            {isEdit ? "Sửa thông tin khách hàng" : "Thêm khách hàng"}
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
          <label className="block text-xs font-medium text-gray-500 mb-1.5">Họ tên *</label>
          <input
            className="input px-3 py-2 text-sm mb-4"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
          />

          <div className="grid grid-cols-2 gap-3 mb-4">
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1.5">Số điện thoại</label>
              <input
                className="input px-3 py-2 text-sm"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1.5">Email</label>
              <input
                type="email"
                className="input px-3 py-2 text-sm"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3 mb-4">
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1.5">CCCD / CMND</label>
              <input
                className="input px-3 py-2 text-sm"
                value={identityNumber}
                onChange={(e) => setIdentityNumber(e.target.value)}
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1.5">Ngày sinh</label>
              <input
                type="date"
                className="input px-3 py-2 text-sm"
                value={dateOfBirth}
                onChange={(e) => setDateOfBirth(e.target.value)}
              />
            </div>
          </div>

          <label className="block text-xs font-medium text-gray-500 mb-1.5">Giới tính</label>
          <select
            className="input px-3 py-2 text-sm mb-4"
            value={gender}
            onChange={(e) => setGender(e.target.value)}
          >
            <option value="">Không xác định</option>
            <option value="MALE">Nam</option>
            <option value="FEMALE">Nữ</option>
            <option value="OTHER">Khác</option>
          </select>

          <label className="block text-xs font-medium text-gray-500 mb-1.5">Địa chỉ</label>
          <input
            className="input px-3 py-2 text-sm mb-4"
            value={address}
            onChange={(e) => setAddress(e.target.value)}
          />

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

export default function CustomersManagement() {
  const [customers, setCustomers] = useState([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [modal, setModal] = useState(null); // { mode: 'create' | 'edit', customer }

  const load = async () => {
    setLoading(true);
    try {
      const res = await customersApi.getAll(search || undefined);
      setCustomers(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    load();
  };

  const handleDelete = async (c) => {
    if (!window.confirm(`Xóa khách hàng "${c.fullName}"? Thao tác này không thể hoàn tác.`)) return;
    setError("");
    try {
      await customersApi.delete(c.id);
      load();
    } catch (err) {
      setError(err.response?.data || "Không thể xóa khách hàng (có thể đang có đặt phòng liên quan).");
    }
  };

  return (
    <Layout title="Quản lý khách hàng">
      <div className="card p-4 mb-5 flex flex-wrap items-center gap-3">
        <form onSubmit={handleSearchSubmit} className="flex gap-2">
          <input
            className="input px-3 py-2 text-sm w-56"
            placeholder="Tìm theo tên/SĐT/CCCD..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <button type="submit" className="btn-search px-4 py-2 text-sm rounded-lg">
            Tìm
          </button>
        </form>

        <button onClick={() => setModal({ mode: "create" })} className="btn btn-gold px-4 py-2 text-sm ml-auto">
          <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2.2">
            <path d="M12 5v14M5 12h14" strokeLinecap="round" />
          </svg>
          Thêm khách hàng
        </button>
      </div>

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
                  <th className="text-left px-4 py-3 font-semibold">Họ tên</th>
                  <th className="text-left px-4 py-3 font-semibold">SĐT</th>
                  <th className="text-left px-4 py-3 font-semibold">Email</th>
                  <th className="text-left px-4 py-3 font-semibold">CCCD</th>
                  <th className="text-left px-4 py-3 font-semibold">Giới tính</th>
                  <th className="text-left px-4 py-3 font-semibold">Hành động</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {customers.map((c) => (
                  <tr key={c.id} className="hover:bg-gray-50/60 transition-colors">
                    <td className="px-4 py-3 font-semibold text-gray-800">{c.fullName}</td>
                    <td className="px-4 py-3 text-gray-500">{c.phone || "—"}</td>
                    <td className="px-4 py-3 text-gray-500">{c.email || "—"}</td>
                    <td className="px-4 py-3 text-gray-500">{c.identityNumber || "—"}</td>
                    <td className="px-4 py-3 text-gray-500">{GENDER_LABELS[c.gender] || "—"}</td>
                    <td className="px-4 py-3">
                      <div className="flex gap-1.5">
                        <button
                          onClick={() => setModal({ mode: "edit", customer: c })}
                          className="chip-btn chip-btn-blue text-xs font-semibold px-2.5 py-1.5 rounded-lg"
                        >
                          Sửa
                        </button>
                        <button
                          onClick={() => handleDelete(c)}
                          className="chip-btn chip-btn-red text-xs font-semibold px-2.5 py-1.5 rounded-lg"
                        >
                          Xóa
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}

                {customers.length === 0 && (
                  <tr>
                    <td colSpan={6} className="text-center text-gray-400 py-14">
                      Không tìm thấy khách hàng nào.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {modal && (
        <CustomerFormModal
          customer={modal.mode === "edit" ? modal.customer : null}
          onClose={() => setModal(null)}
          onDone={load}
        />
      )}
    </Layout>
  );
}