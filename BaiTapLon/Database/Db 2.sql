-- Thêm trạng thái CLEANING cho phòng
ALTER TABLE rooms DROP CONSTRAINT IF EXISTS rooms_status_check;
ALTER TABLE rooms ADD CONSTRAINT rooms_status_check
    CHECK (status IN ('AVAILABLE','RESERVED','OCCUPIED','MAINTENANCE','CLEANING'));

-- Hàng chờ đặt phòng
CREATE TABLE room_waitlist (
    id                 UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id        UUID NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    room_id            UUID NOT NULL REFERENCES rooms(id) ON DELETE CASCADE,
    desired_check_in   DATE NOT NULL,
    desired_check_out  DATE NOT NULL,
    status             VARCHAR(20) NOT NULL DEFAULT 'WAITING'
                       CHECK (status IN ('WAITING','NOTIFIED','CONVERTED','CANCELLED')),
    queue_position     INT NOT NULL DEFAULT 1,
    created_at         TIMESTAMP NOT NULL DEFAULT now(),
    notified_at        TIMESTAMP
);
CREATE INDEX idx_waitlist_room_status ON room_waitlist(room_id, status);

-- Nhật ký thao tác đặt phòng (audit log)
CREATE TABLE booking_logs (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    booking_id    UUID NOT NULL REFERENCES bookings(id) ON DELETE CASCADE,
    action        VARCHAR(30) NOT NULL,
    performed_by  UUID NOT NULL REFERENCES employees(id),
    note          VARCHAR(500),
    created_at    TIMESTAMP NOT NULL DEFAULT now()
);
CREATE INDEX idx_booking_logs_booking_id ON booking_logs(booking_id);

-- Role Housekeeping (lao công) + quyền
INSERT INTO roles (name, description) VALUES
    ('Housekeeping', 'Nhân viên buồng phòng - xác nhận dọn phòng xong');

INSERT INTO permissions (code, description) VALUES
    ('ROOM_CLEANING_CONFIRM', 'Xác nhận dọn phòng xong'),
    ('WAITLIST_MANAGE', 'Quản lý hàng chờ đặt phòng');

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p ON p.code = 'ROOM_CLEANING_CONFIRM'
WHERE r.name = 'Housekeeping';

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p ON p.code = 'WAITLIST_MANAGE'
WHERE r.name = 'Receptionist';