import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider, useAuth } from "./context/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import Login from "./pages/Login";
import Reception from "./pages/Reception";
import BookingsList from "./pages/BookingsList";
import Housekeeping from "./pages/Housekeeping";
import AdminDashboard from "./pages/AdminDashboard";
import RoomTypesManagement from "./pages/admin/RoomTypesManagement";
import RoomsManagement from "./pages/admin/RoomsManagement";
import CustomersManagement from "./pages/admin/CustomersManagement";

function HomeRedirect() {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  if (user.roles.includes("Admin")) return <Navigate to="/admin" replace />;
  if (user.roles.includes("Housekeeping")) return <Navigate to="/housekeeping" replace />;
  return <Navigate to="/reception" replace />;
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<HomeRedirect />} />
          <Route path="/login" element={<Login />} />

          <Route
            path="/reception"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Receptionist"]}>
                <Reception />
              </ProtectedRoute>
            }
          />

          <Route
            path="/bookings"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Receptionist"]}>
                <BookingsList />
              </ProtectedRoute>
            }
          />

          <Route
            path="/housekeeping"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Housekeeping"]}>
                <Housekeeping />
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <AdminDashboard />
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/room-types"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <RoomTypesManagement />
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/rooms"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <RoomsManagement />
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/customers"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <CustomersManagement />
              </ProtectedRoute>
            }
          />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}