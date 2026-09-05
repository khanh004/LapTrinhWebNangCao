import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const user = await login(username, password);
      if (user.roles.includes("Admin")) navigate("/admin");
      else if (user.roles.includes("Housekeeping")) navigate("/housekeeping");
      else navigate("/reception");
    } catch (err) {
      setError(err.response?.data || "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex bg-[#0b1220]">
      {/* Left branding panel */}
      <div className="hidden lg:flex w-1/2 relative overflow-hidden flex-col justify-between p-12 bg-gradient-to-br from-[#0b1220] via-[#101a2f] to-[#16213f]">
        <div
          className="absolute inset-0 opacity-[0.07]"
          style={{
            backgroundImage:
              "radial-gradient(circle at 2px 2px, white 1px, transparent 0)",
            backgroundSize: "28px 28px",
          }}
        />
        <div className="absolute -top-24 -right-24 w-96 h-96 rounded-full bg-[#c9a24b]/10 blur-3xl" />
        <div className="absolute -bottom-32 -left-16 w-96 h-96 rounded-full bg-blue-500/10 blur-3xl" />

        <div className="relative flex items-center gap-3">
          <div className="w-11 h-11 rounded-xl bg-gradient-to-br from-[#d9b96a] to-[#c9a24b] flex items-center justify-center shadow-lg">
            <svg viewBox="0 0 24 24" className="w-6 h-6 text-[#101a2f]" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6M9 9h.01M15 9h.01M9 13h.01M15 13h.01" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
          </div>
          <span className="text-white font-display font-bold text-xl">Grand Hotel PMS</span>
        </div>

        <div className="relative">
          <h2 className="text-4xl font-display font-bold text-white leading-tight mb-4">
            Quản lý khách sạn<br />gọn gàng &amp; chuyên nghiệp.
          </h2>
          <p className="text-slate-400 text-base max-w-md leading-relaxed">
            Theo dõi tình trạng phòng theo thời gian thực, quản lý đặt phòng, nhận/trả phòng và
            hàng chờ khách hàng — tất cả trong một nơi.
          </p>

          <div className="flex gap-6 mt-10">
            {[
              { label: "Bảng phòng trực quan" },
              { label: "Đặt phòng & gia hạn" },
              { label: "Nhật ký minh bạch" },
            ].map((f) => (
              <div key={f.label} className="flex items-center gap-2 text-slate-300 text-sm">
                <svg viewBox="0 0 24 24" className="w-4 h-4 text-[#d9b96a] shrink-0" fill="none" stroke="currentColor" strokeWidth="2.5">
                  <path d="M20 6 9 17l-5-5" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
                {f.label}
              </div>
            ))}
          </div>
        </div>

        <p className="relative text-slate-500 text-xs">© {new Date().getFullYear()} Grand Hotel PMS</p>
      </div>

      {/* Right form panel */}
      <div className="flex-1 flex items-center justify-center bg-[#f3f4f7] px-6 py-12">
        <div className="w-full max-w-sm fade-up">
          <div className="lg:hidden flex items-center gap-2.5 mb-8 justify-center">
            <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-[#d9b96a] to-[#c9a24b] flex items-center justify-center">
              <svg viewBox="0 0 24 24" className="w-5 h-5 text-[#101a2f]" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6" strokeLinecap="round" strokeLinejoin="round" />
              </svg>
            </div>
            <span className="font-display font-bold text-lg text-[#101a2f]">Grand Hotel PMS</span>
          </div>

          <div className="card p-8">
            <h1 className="text-xl font-display font-bold text-gray-900 mb-1">
              Chào mừng trở lại
            </h1>
            <p className="text-sm text-gray-500 mb-6">
              Đăng nhập để tiếp tục vào hệ thống quản lý khách sạn.
            </p>

            <form onSubmit={handleSubmit}>
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Tài khoản</label>
              <div className="relative mb-4">
                <svg viewBox="0 0 24 24" className="w-4 h-4 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" fill="none" stroke="currentColor" strokeWidth="2">
                  <circle cx="12" cy="8" r="4" />
                  <path d="M4 21c0-4.4 3.6-8 8-8s8 3.6 8 8" strokeLinecap="round" />
                </svg>
                <input
                  className="input pl-9 pr-3 py-2.5 text-sm"
                  placeholder="Nhập tài khoản của bạn"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  autoFocus
                  required
                />
              </div>

              <label className="block text-sm font-medium text-gray-700 mb-1.5">Mật khẩu</label>
              <div className="relative mb-2">
                <svg viewBox="0 0 24 24" className="w-4 h-4 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" fill="none" stroke="currentColor" strokeWidth="2">
                  <rect x="4" y="10" width="16" height="10" rx="2" />
                  <path d="M8 10V7a4 4 0 0 1 8 0v3" />
                </svg>
                <input
                  type={showPassword ? "text" : "password"}
                  className="input pl-9 pr-10 py-2.5 text-sm"
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword((s) => !s)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                  tabIndex={-1}
                >
                  {showPassword ? (
                    <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M3 3l18 18M10.6 10.6a2 2 0 0 0 2.8 2.8M9.4 5.5A9.9 9.9 0 0 1 12 5c5 0 9 4.5 10 7-.6 1.2-1.5 2.5-2.7 3.6M6.6 6.6C4.5 8 3 10 2 12c1 2.5 5 7 10 7 1.4 0 2.7-.3 3.9-.8" strokeLinecap="round" strokeLinejoin="round" />
                    </svg>
                  ) : (
                    <svg viewBox="0 0 24 24" className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7Z" />
                      <circle cx="12" cy="12" r="3" />
                    </svg>
                  )}
                </button>
              </div>

              {error && (
                <div className="flex items-start gap-2 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg px-3 py-2.5 mt-3">
                  <svg viewBox="0 0 24 24" className="w-4 h-4 mt-0.5 shrink-0" fill="none" stroke="currentColor" strokeWidth="2">
                    <circle cx="12" cy="12" r="10" />
                    <path d="M12 8v5M12 16h.01" strokeLinecap="round" />
                  </svg>
                  <span>{error}</span>
                </div>
              )}

              <button
                type="submit"
                disabled={loading}
                className="btn btn-primary w-full py-2.5 text-sm mt-5"
              >
                {loading ? (
                  <>
                    <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
                      <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" opacity="0.25" />
                      <path d="M22 12a10 10 0 0 1-10 10" stroke="currentColor" strokeWidth="4" strokeLinecap="round" />
                    </svg>
                    Đang đăng nhập...
                  </>
                ) : (
                  "Đăng nhập"
                )}
              </button>
            </form>
          </div>

          <p className="text-center text-xs text-gray-400 mt-6">
            Cần hỗ trợ? Liên hệ quản trị viên hệ thống của bạn.
          </p>
        </div>
      </div>
    </div>
  );
}