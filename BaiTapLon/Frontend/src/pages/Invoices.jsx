import { useEffect, useState } from "react";
import Layout from "../components/Layout";
import { invoicesApi } from "../api/invoices";
import InvoiceDetailModal from "../components/InvoiceDetailModal";

const FILTERS = [
  { value: "", label: "Tất cả" },
  { value: "UNPAID", label: "Chưa thanh toán" },
  { value: "PAID", label: "Đã thanh toán" },
];

const formatMoney = (n) => new Intl.NumberFormat("vi-VN").format(n) + "đ";

export default function Invoices() {
  const [invoices, setInvoices] = useState([]);
  const [filter, setFilter] = useState("");
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState(null);

  const load = async () => {
    setLoading(true);
    try {
      const res = await invoicesApi.getAll(filter || undefined);
      setInvoices(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  const handleMarkPaid = async () => {
    if (!selected) return;
    await invoicesApi.markPaid(selected.id);
    setSelected(null);
    load();
  };

  const totalUnpaid = invoices
    .filter((i) => i.paymentStatus === "UNPAID")
    .reduce((sum, i) => sum + i.totalAmount, 0);

  return (
    <Layout title="Hóa đơn">
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-5">
        <div className="card p-4">
          <p className="text-xs text-gray-500 mb-1">Tổng số hóa đơn</p>
          <p className="text-2xl font-bold text-gray-900 font-display">{invoices.length}</p>
        </div>
        <div className="card p-4">
          <p className="text-xs text-gray-500 mb-1">Chưa thanh toán</p>
          <p className="text-2xl font-bold text-amber-600 font-display">
            {invoices.filter((i) => i.paymentStatus === "UNPAID").length}
          </p>
        </div>
        <div className="card p-4">
          <p className="text-xs text-gray-500 mb-1">Tổng tiền chưa thu</p>
          <p className="text-xl font-bold text-red-600 font-display">{formatMoney(totalUnpaid)}</p>
        </div>
      </div>

      <div className="card p-4 mb-5 flex flex-wrap gap-1.5">
        {FILTERS.map((f) => (
          <button
            key={f.value}
            onClick={() => setFilter(f.value)}
            className={`filter-chip px-3 py-1.5 rounded-lg text-xs ${
              filter === f.value ? "filter-chip-active" : "filter-chip-inactive"
            }`}
          >
            {f.label}
          </button>
        ))}
      </div>

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
                  <th className="text-left px-4 py-3 font-semibold">Mã hóa đơn</th>
                  <th className="text-left px-4 py-3 font-semibold">Khách hàng</th>
                  <th className="text-left px-4 py-3 font-semibold">Phòng</th>
                  <th className="text-left px-4 py-3 font-semibold">Tổng tiền</th>
                  <th className="text-left px-4 py-3 font-semibold">Trạng thái</th>
                  <th className="text-left px-4 py-3 font-semibold">Ngày xuất</th>
                  <th className="text-left px-4 py-3 font-semibold"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {invoices.map((inv) => (
                  <tr key={inv.id} className="hover:bg-gray-50/60 transition-colors">
                    <td className="px-4 py-3 font-mono text-xs text-gray-500">{inv.invoiceCode}</td>
                    <td className="px-4 py-3 font-semibold text-gray-800">{inv.customerName}</td>
                    <td className="px-4 py-3 text-gray-600">{inv.roomNumber}</td>
                    <td className="px-4 py-3 font-semibold text-gray-900">
                      {formatMoney(inv.totalAmount)}
                    </td>
                    <td className="px-4 py-3">
                      <span
                        className={`px-2.5 py-1 rounded-full text-xs font-bold ${
                          inv.paymentStatus === "PAID"
                            ? "bg-emerald-600 text-white"
                            : "bg-amber-500 text-white"
                        }`}
                      >
                        {inv.paymentStatus === "PAID" ? "Đã thanh toán" : "Chưa thanh toán"}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-500 whitespace-nowrap">
                      {new Date(inv.issuedAt).toLocaleDateString("vi-VN")}
                    </td>
                    <td className="px-4 py-3">
                      <button
                        onClick={() => setSelected(inv)}
                        className="chip-btn chip-btn-blue text-xs px-2.5 py-1.5 rounded-lg"
                      >
                        Xem chi tiết
                      </button>
                    </td>
                  </tr>
                ))}

                {invoices.length === 0 && (
                  <tr>
                    <td colSpan={7} className="text-center text-gray-400 py-14">
                      Chưa có hóa đơn nào.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {selected && (
        <InvoiceDetailModal
          invoice={selected}
          onClose={() => setSelected(null)}
          onMarkPaid={handleMarkPaid}
        />
      )}
    </Layout>
  );
}