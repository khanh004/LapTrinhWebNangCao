import { createContext, useContext, useState } from "react";
import apiClient from "../api/client";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const saved = localStorage.getItem("user");
    return saved ? JSON.parse(saved) : null;
  });

  const login = async (username, password) => {
  const res = await apiClient.post("/auth/login", { username, password });
  const { token, username: uname, fullName, roles, employeeId } = res.data;

  localStorage.setItem("token", token);
  const userData = { username: uname, fullName, roles, employeeId };
  localStorage.setItem("user", JSON.stringify(userData));
  setUser(userData);

  return userData;
};

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    setUser(null);
  };

  const hasRole = (...allowedRoles) =>
    user?.roles?.some((r) => allowedRoles.includes(r)) ?? false;

  return (
    <AuthContext.Provider value={{ user, login, logout, hasRole }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}