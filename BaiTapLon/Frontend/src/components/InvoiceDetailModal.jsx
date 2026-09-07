export default function InvoiceDetailModal({ invoice, onClose, onMarkPaid }) {
  return (
    <div className="fixed inset-0 bg-black/50 backdrop-blur-[2px] flex items-center justify-center z-50 p-4 modal-overlay">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md p-6 modal-panel">
        <div className="flex justify-between items-start mb-4">
          <div>
            <p className="text-xs text-gray-400 font-mono">{invoice.invoiceCode}</p>
            <h2 className="text-lg font-bold text-gray-900 font-display">
              Hóa đơn — Phòng {invoice.roomNumber}
            </h2>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-700">
            <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M18 6 6 18M6 6l12 12" strokeLinecap="round" />
            </svg>
          </button>
        </div>

        <div className="text-sm text-gray-500 mb-4 space-y-0.5">
          <p>Khách hàng: <span className="text-gray-800 font-medium">{invoice.customerName}</span></p>
          <p>Mã đặt phòng: <span className="text-gray-800 font-medium">{invoice.bookingCode}</span></p>
          <p>Ngày xuất: {new Date(invoice.issuedAt).toLocaleString("vi-VN")}</p>
        </div>

        <div className="bg-gray-50 border border-gray-100 rounded-xl p-4 mb-4">
          <pre className="text-sm text-gray-700 whitespace-pre-wrap font-sans leading-relaxed">
            {invoice.description}
          </pre>
        </div>

        <div className="flex items-center justify-between mb-5">
          <span className="text-sm text-gray-500">Trạng thái thanh toán</span>
          <span
            className={`px-3 py-1 rounded-full text-xs font-bold ${
              invoice.paymentStatus === "PAID"
                ? "bg-emerald-600 text-white"
                : "bg-amber-500 text-white"
            }`}
          >
            {invoice.paymentStatus === "PAID" ? "Đã thanh toán" : "Chưa thanh toán"}
          </span>
        </div>

        {invoice.paymentStatus === "UNPAID" && (
          <button onClick={onMarkPaid} className="btn btn-primary w-full py-2.5 text-sm">
            Đánh dấu đã thanh toán
          </button>
        )}
      </div>
    </div>
  );
}