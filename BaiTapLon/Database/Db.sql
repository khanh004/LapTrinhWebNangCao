-- =====================================================================
-- HOTEL MANAGEMENT SYSTEM - DATABASE SCHEMA
-- Database: PostgreSQL
-- Dựa trên "Báo cáo sơ bộ đề xuất đề tài: Hệ thống quản lý khách sạn"
-- =====================================================================
 
-- BƯỚC 1: Tạo database (chạy lệnh này khi đang kết nối tới database mặc định,
-- ví dụ "postgres". PostgreSQL KHÔNG hỗ trợ lệnh USE như MySQL/SQL Server).
-- Nếu chạy bằng psql, chạy riêng lệnh này trước, sau đó gõ: \c hotel_management_db
-- Nếu dùng pgAdmin/DBeaver, chỉ cần tạo database rồi kết nối/connect trực tiếp vào nó.
 
CREATE DATABASE hotel_management_db;
 
-- Sau khi tạo xong, hãy CHUYỂN kết nối sang database "hotel_management_db"
-- rồi mới chạy tiếp các lệnh phía dưới (từ CREATE EXTENSION trở xuống).
-- Trong psql: \c hotel_management_db
 
-- =====================================================================
-- BƯỚC 2: Tạo bảng, index, dữ liệu mẫu (chạy sau khi đã kết nối đúng database)
-- =====================================================================
 
-- Bật extension để sinh UUID tự động
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
 
-- Xoá schema cũ nếu chạy lại (bỏ comment nếu cần reset khi dev)
-- DROP SCHEMA public CASCADE;
-- CREATE SCHEMA public;
 
-- =====================================================================
-- 1. NHÓM RBAC: users, roles, permissions, user_roles, role_permissions
-- =====================================================================
 
CREATE TABLE roles (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(50)  NOT NULL UNIQUE,      -- Admin, Receptionist, Service Staff, Manager
    description VARCHAR(255),
    created_at  TIMESTAMP NOT NULL DEFAULT now()
);
 
CREATE TABLE permissions (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code        VARCHAR(100) NOT NULL UNIQUE,      -- vd: ROOM_CREATE, BOOKING_UPDATE
    description VARCHAR(255),
    created_at  TIMESTAMP NOT NULL DEFAULT now()
);
 
CREATE TABLE employees (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    full_name   VARCHAR(150) NOT NULL,
    phone       VARCHAR(20),
    email       VARCHAR(150) UNIQUE,
    position    VARCHAR(100),
    hire_date   DATE,
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMP NOT NULL DEFAULT now()
);
 
CREATE TABLE users (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username       VARCHAR(50)  NOT NULL UNIQUE,
    password_hash  VARCHAR(255) NOT NULL,
    employee_id    UUID NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    is_active      BOOLEAN NOT NULL DEFAULT TRUE,
    created_at     TIMESTAMP NOT NULL DEFAULT now()
);
 
-- Bảng nối User - Role (khóa chính ghép)
CREATE TABLE user_roles (
    user_id  UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id  UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, role_id)
);
 
-- Bảng nối Role - Permission (khóa chính ghép)
CREATE TABLE role_permissions (
    role_id       UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    permission_id UUID NOT NULL REFERENCES permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
);
 
-- =====================================================================
-- 2. NHÓM PHÒNG: room_types, rooms
-- =====================================================================
 
CREATE TABLE room_types (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name             VARCHAR(50) NOT NULL UNIQUE,   -- Standard, Deluxe, Suite, VIP
    description      TEXT,
    price_per_night  DECIMAL(12,2) NOT NULL CHECK (price_per_night >= 0),
    max_guests       INT NOT NULL CHECK (max_guests > 0),
    created_at       TIMESTAMP NOT NULL DEFAULT now()
);
 
CREATE TABLE rooms (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    room_number   VARCHAR(20) NOT NULL UNIQUE,
    room_type_id  UUID NOT NULL REFERENCES room_types(id) ON DELETE RESTRICT,
    status        VARCHAR(20) NOT NULL DEFAULT 'AVAILABLE'
                  CHECK (status IN ('AVAILABLE','RESERVED','OCCUPIED','MAINTENANCE')),
    floor         INT,
    created_at    TIMESTAMP NOT NULL DEFAULT now()
);
 
-- =====================================================================
-- 3. NHÓM KHÁCH HÀNG
-- =====================================================================
 
CREATE TABLE customers (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    full_name        VARCHAR(150) NOT NULL,
    date_of_birth    DATE,
    gender           VARCHAR(10) CHECK (gender IN ('MALE','FEMALE','OTHER')),
    phone            VARCHAR(20),
    email            VARCHAR(150),
    identity_number  VARCHAR(30) UNIQUE,             -- CCCD/CMND
    address          VARCHAR(255),
    created_at       TIMESTAMP NOT NULL DEFAULT now()
);
 
-- =====================================================================
-- 4. NHÓM ĐẶT PHÒNG / NHẬN PHÒNG / TRẢ PHÒNG
-- =====================================================================
 
CREATE TABLE bookings (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id     UUID NOT NULL REFERENCES customers(id) ON DELETE RESTRICT,
    room_id         UUID NOT NULL REFERENCES rooms(id) ON DELETE RESTRICT,
    booking_code    VARCHAR(30) NOT NULL UNIQUE,
    check_in_date   DATE NOT NULL,
    check_out_date  DATE NOT NULL,
    status          VARCHAR(20) NOT NULL DEFAULT 'PENDING'
                    CHECK (status IN ('PENDING','CONFIRMED','CHECKED_IN','CHECKED_OUT','CANCELLED')),
    created_at      TIMESTAMP NOT NULL DEFAULT now(),
    CHECK (check_out_date > check_in_date)
);
 
CREATE TABLE check_ins (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    booking_id        UUID NOT NULL REFERENCES bookings(id) ON DELETE CASCADE,
    actual_check_in   TIMESTAMP NOT NULL DEFAULT now(),
    employee_id       UUID NOT NULL REFERENCES employees(id) ON DELETE RESTRICT
);
 
CREATE TABLE check_outs (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    booking_id        UUID NOT NULL REFERENCES bookings(id) ON DELETE CASCADE,
    actual_check_out  TIMESTAMP NOT NULL DEFAULT now(),
    employee_id       UUID NOT NULL REFERENCES employees(id) ON DELETE RESTRICT
);
 
-- =====================================================================
-- 5. NHÓM DỊCH VỤ
-- =====================================================================
 
CREATE TABLE services (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL,      -- Ăn sáng, Giặt ủi, Đồ uống, Dọn phòng, Spa, Thuê xe...
    description TEXT,
    price       DECIMAL(12,2) NOT NULL CHECK (price >= 0),
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMP NOT NULL DEFAULT now()
);
 
-- Bảng nối N-N giữa bookings và services
CREATE TABLE booking_services (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    booking_id  UUID NOT NULL REFERENCES bookings(id) ON DELETE CASCADE,
    service_id  UUID NOT NULL REFERENCES services(id) ON DELETE RESTRICT,
    quantity    INT NOT NULL DEFAULT 1 CHECK (quantity > 0),
    unit_price  DECIMAL(12,2) NOT NULL CHECK (unit_price >= 0),
    used_at     TIMESTAMP NOT NULL DEFAULT now()
);
 
-- =====================================================================
-- 6. NHÓM HÓA ĐƠN
-- =====================================================================
 
CREATE TABLE invoices (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    booking_id      UUID NOT NULL REFERENCES book	ings(id) ON DELETE RESTRICT,
    invoice_code    VARCHAR(30) NOT NULL UNIQUE,
    room_amount     DECIMAL(14,2) NOT NULL DEFAULT 0 CHECK (room_amount >= 0),
    service_amount  DECIMAL(14,2) NOT NULL DEFAULT 0 CHECK (service_amount >= 0),
    surcharge       DECIMAL(14,2) NOT NULL DEFAULT 0 CHECK (surcharge >= 0),
    total_amount    DECIMAL(14,2) NOT NULL DEFAULT 0 CHECK (total_amount >= 0),
    payment_status  VARCHAR(20) NOT NULL DEFAULT 'UNPAID'
                    CHECK (payment_status IN ('UNPAID','PAID')),
    issued_at       TIMESTAMP NOT NULL DEFAULT now()
);
 
-- =====================================================================
-- 7. INDEX PHỤ TRỢ (tăng tốc truy vấn / tra cứu thường dùng)
-- =====================================================================
 
CREATE INDEX idx_rooms_status              ON rooms(status);
CREATE INDEX idx_rooms_room_type_id        ON rooms(room_type_id);
 
CREATE INDEX idx_customers_phone           ON customers(phone);
CREATE INDEX idx_customers_full_name       ON customers(full_name);
 
CREATE INDEX idx_bookings_customer_id      ON bookings(customer_id);
CREATE INDEX idx_bookings_room_id          ON bookings(room_id);
CREATE INDEX idx_bookings_status           ON bookings(status);
CREATE INDEX idx_bookings_dates            ON bookings(check_in_date, check_out_date);
 
CREATE INDEX idx_booking_services_booking  ON booking_services(booking_id);
CREATE INDEX idx_booking_services_service  ON booking_services(service_id);
 
CREATE INDEX idx_invoices_booking_id       ON invoices(booking_id);
CREATE INDEX idx_invoices_payment_status   ON invoices(payment_status);
 
CREATE INDEX idx_users_employee_id         ON users(employee_id);
 
-- =====================================================================
-- 8. DỮ LIỆU MẪU BAN ĐẦU (seed) — Roles & Permissions cơ bản (RBAC)
-- =====================================================================
 
INSERT INTO roles (name, description) VALUES
    ('Admin',          'Quản trị viên - toàn quyền hệ thống'),
    ('Receptionist',   'Nhân viên lễ tân'),
    ('Service Staff',  'Nhân viên quản lý dịch vụ'),
    ('Manager',        'Quản lý khách sạn - xem báo cáo, thống kê');
 
INSERT INTO permissions (code, description) VALUES
    ('USER_MANAGE',      'Quản lý tài khoản người dùng'),
    ('ROLE_MANAGE',      'Quản lý vai trò và quyền'),
    ('EMPLOYEE_MANAGE',  'Quản lý nhân viên'),
    ('ROOM_MANAGE',      'Quản lý phòng và loại phòng'),
    ('CUSTOMER_MANAGE',  'Quản lý khách hàng'),
    ('BOOKING_MANAGE',   'Quản lý đặt phòng'),
    ('CHECKIN_MANAGE',   'Thực hiện nhận phòng'),
    ('CHECKOUT_MANAGE',  'Thực hiện trả phòng'),
    ('SERVICE_MANAGE',   'Quản lý dịch vụ khách sạn'),
    ('INVOICE_MANAGE',   'Quản lý hóa đơn, thanh toán'),
    ('REPORT_VIEW',      'Xem báo cáo, thống kê');
 
-- Gán toàn bộ quyền cho Admin
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r CROSS JOIN permissions p WHERE r.name = 'Admin';
 
-- Receptionist: khách hàng, đặt phòng, nhận/trả phòng, hóa đơn
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p
  ON p.code IN ('CUSTOMER_MANAGE','BOOKING_MANAGE','CHECKIN_MANAGE','CHECKOUT_MANAGE','INVOICE_MANAGE')
WHERE r.name = 'Receptionist';
 
-- Service Staff: quản lý dịch vụ
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p
  ON p.code IN ('SERVICE_MANAGE')
WHERE r.name = 'Service Staff';
 
-- Manager: chỉ xem báo cáo
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p
  ON p.code IN ('REPORT_VIEW')
WHERE r.name = 'Manager';
 
-- Loại phòng mẫu
INSERT INTO room_types (name, description, price_per_night, max_guests) VALUES
    ('Standard', 'Phòng tiêu chuẩn', 500000, 2),
    ('Deluxe',   'Phòng cao cấp',    800000, 2),
    ('Suite',    'Phòng suite',      1500000, 3),
    ('VIP',      'Phòng VIP',        2500000, 4);