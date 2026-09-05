import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function ProtectedRoute({ children, allowedRoles }) {
  const { user, hasRole } = useAuth();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles && !hasRole(...allowedRoles)) {
    return (
      <div className="flex items-center justify-center h-screen bg-[#f3f4f7] px-4">
        <div className="card p-10 text-center max-w-sm">
          <div className="w-14 h-14 rounded-2xl bg-red-50 text-red-500 flex items-center justify-center mx-auto mb-4">
            <svg viewBox="0 0 24 24" className="w-7 h-7" fill="none" stroke="currentColor" strokeWidth="1.8">
              <path d="M12 9v4M12 17h.01" strokeLinecap="round" />
              <path d="M10.3 3.9 1.9 18a1.8 1.8 0 0 0 1.6 2.7h17a1.8 1.8 0 0 0 1.6-2.7L13.7 3.9a1.8 1.8 0 0 0-3.4 0Z" strokeLinejoin="round" />
            </svg>
          </div>
          <p className="text-lg font-bold text-gray-900 font-display">Không có quyền truy cập</p>
          <p className="text-gray-500 text-sm mt-2">Tài khoản của bạn không đủ quyền xem trang này.</p>
        </div>
      </div>
    );
  }

  return children;
}