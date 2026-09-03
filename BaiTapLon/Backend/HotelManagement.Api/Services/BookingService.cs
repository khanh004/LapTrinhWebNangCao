using HotelManagement.Api.Data;
using HotelManagement.Api.DTOs;
using HotelManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Api.Services;

public interface IBookingService
{
    Task<List<BookingDto>> GetAllAsync(string? status);
    Task<BookingDto?> GetByIdAsync(Guid id);
    Task<List<BookingLogDto>> GetLogsAsync(Guid bookingId);

    Task<(BookingDto? Booking, string? Error)> CreateAsync(CreateBookingDto dto, Guid performedBy);
    Task<(bool Success, string? Error)> ConfirmAsync(Guid id, Guid performedBy);
    Task<(bool Success, string? Error)> CheckInAsync(Guid id, Guid performedBy);
    Task<(bool Success, string? Error, decimal? TotalAmount, string? InvoiceDescription)> CheckOutAsync(Guid id, CheckOutRequestDto dto, Guid performedBy);
    Task<(bool Success, string? Error)> ExtendAsync(Guid id, ExtendBookingDto dto, Guid performedBy);
    Task<(bool Success, string? Error)> CancelAsync(Guid id, Guid performedBy);
}

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(ApplicationDbContext context)
    {
        _context = context;
    }

    private static BookingDto ToDto(Booking b) => new()
    {
        Id = b.Id,
        CustomerId = b.CustomerId,
        CustomerName = b.Customer?.FullName ?? string.Empty,
        RoomId = b.RoomId,
        RoomNumber = b.Room?.RoomNumber ?? string.Empty,
        BookingCode = b.BookingCode,
        CheckInDate = b.CheckInDate,
        CheckOutDate = b.CheckOutDate,
        OriginalCheckOutDate = b.OriginalCheckOutDate,
        ApprovedExtraHours = b.ApprovedExtraHours,
        Status = b.Status.ToString(),
        ActualCheckIn = b.CheckIn?.ActualCheckIn,
        ActualCheckOut = b.CheckOut?.ActualCheckOut
    };

    private async Task LogAsync(Guid bookingId, BookingAction action, Guid performedBy, string? note = null)
    {
        _context.BookingLogs.Add(new BookingLog
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            Action = action,
            PerformedBy = performedBy,
            Note = note,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<List<BookingDto>> GetAllAsync(string? status)
    {
        var query = _context.Bookings
            .Include(b => b.Customer).Include(b => b.Room)
            .Include(b => b.CheckIn).Include(b => b.CheckOut)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var s))
            query = query.Where(b => b.Status == s);

        return await query.OrderByDescending(b => b.CreatedAt).Select(b => ToDto(b)).ToListAsync();
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id)
    {
        var b = await _context.Bookings
            .Include(x => x.Customer).Include(x => x.Room)
            .Include(x => x.CheckIn).Include(x => x.CheckOut)
            .FirstOrDefaultAsync(x => x.Id == id);
        return b is null ? null : ToDto(b);
    }

    public async Task<List<BookingLogDto>> GetLogsAsync(Guid bookingId)
    {
        return await _context.BookingLogs
            .Include(l => l.Employee)
            .Where(l => l.BookingId == bookingId)
            .OrderBy(l => l.CreatedAt)
            .Select(l => new BookingLogDto
            {
                Action = l.Action.ToString(),
                PerformedByName = l.Employee.FullName,
                Note = l.Note,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();
    }

    private async Task<bool> IsRoomAvailableAsync(Guid roomId, DateOnly checkIn, DateOnly checkOut, Guid? excludeBookingId = null)
    {
        var query = _context.Bookings.Where(b =>
            b.RoomId == roomId &&
            b.Status != BookingStatus.CANCELLED && b.Status != BookingStatus.CHECKED_OUT &&
            b.CheckInDate < checkOut && b.CheckOutDate > checkIn);

        if (excludeBookingId.HasValue)
            query = query.Where(b => b.Id != excludeBookingId.Value);

        return !await query.AnyAsync();
    }

    // Kiểm tra có khách nào đang xếp hàng chờ (WAITING/NOTIFIED) trùng khoảng ngày này không
    private async Task<bool> HasWaitlistConflictAsync(Guid roomId, DateOnly rangeStart, DateOnly rangeEnd)
    {
        return await _context.RoomWaitlist.AnyAsync(w =>
            w.RoomId == roomId &&
            (w.Status == WaitlistStatus.WAITING || w.Status == WaitlistStatus.NOTIFIED) &&
            w.DesiredCheckIn < rangeEnd && w.DesiredCheckOut > rangeStart);
    }

    public async Task<(BookingDto? Booking, string? Error)> CreateAsync(CreateBookingDto dto, Guid performedBy)
    {
        if (dto.CheckOutDate <= dto.CheckInDate)
            return (null, "Ngày trả phòng phải sau ngày nhận phòng.");

        var room = await _context.Rooms.FindAsync(dto.RoomId);
        if (room is null) return (null, "Phòng không tồn tại.");
        if (room.Status == RoomStatus.MAINTENANCE)
            return (null, "Phòng đang bảo trì, không thể đặt.");
        if (room.Status == RoomStatus.CLEANING)
            return (null, "Phòng đang dọn dẹp, chưa thể đặt.");

        if (!await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId))
            return (null, "Khách hàng không tồn tại.");

        if (!await IsRoomAvailableAsync(dto.RoomId, dto.CheckInDate, dto.CheckOutDate))
            return (null, "Phòng đã có người đặt trong khoảng thời gian này. Có thể thêm khách vào hàng chờ (waitlist).");

        var entity = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            RoomId = dto.RoomId,
            BookingCode = "BK" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            CheckInDate = dto.CheckInDate,
            CheckOutDate = dto.CheckOutDate,
            OriginalCheckOutDate = dto.CheckOutDate,
            Status = BookingStatus.PENDING,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(entity);

        // Chỉ khóa phòng RESERVED nếu khách nhận phòng NGAY HÔM NAY.
        // Đặt cho tương lai thì phòng vẫn AVAILABLE hôm nay.
        var today = DateOnly.FromDateTime(DateTime.Now);
        if (room.Status == RoomStatus.AVAILABLE && dto.CheckInDate <= today)
            room.Status = RoomStatus.RESERVED;

        await LogAsync(entity.Id, BookingAction.CREATED, performedBy,
            $"Đặt phòng {room.RoomNumber} từ {dto.CheckInDate} đến {dto.CheckOutDate}");

        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(b => b.Customer).LoadAsync();
        await _context.Entry(entity).Reference(b => b.Room).LoadAsync();

        return (ToDto(entity), null);
    }

    public async Task<(bool Success, string? Error)> ConfirmAsync(Guid id, Guid performedBy)
    {
        var b = await _context.Bookings.FindAsync(id);
        if (b is null) return (false, "Không tìm thấy đặt phòng.");
        if (b.Status != BookingStatus.PENDING) return (false, "Chỉ xác nhận được đặt phòng đang chờ.");

        b.Status = BookingStatus.CONFIRMED;
        await LogAsync(id, BookingAction.CONFIRMED, performedBy);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CheckInAsync(Guid id, Guid performedBy)
    {
        var b = await _context.Bookings.Include(x => x.Room).FirstOrDefaultAsync(x => x.Id == id);
        if (b is null) return (false, "Không tìm thấy đặt phòng.");
        if (b.Status is not (BookingStatus.PENDING or BookingStatus.CONFIRMED))
            return (false, "Đặt phòng không ở trạng thái có thể nhận phòng.");

        var today = DateOnly.FromDateTime(DateTime.Now);
        if (today < b.CheckInDate)
            return (false, $"Chưa tới ngày nhận phòng ({b.CheckInDate}).");

        _context.CheckIns.Add(new CheckIn
        {
            Id = Guid.NewGuid(),
            BookingId = id,
            ActualCheckIn = DateTime.UtcNow,
            EmployeeId = performedBy
        });

        b.Status = BookingStatus.CHECKED_IN;
        b.Room!.Status = RoomStatus.OCCUPIED;

        await LogAsync(id, BookingAction.CHECKED_IN, performedBy, $"Nhận phòng {b.Room.RoomNumber}");
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error, decimal? TotalAmount, string? InvoiceDescription)> CheckOutAsync(
        Guid id, CheckOutRequestDto dto, Guid performedBy)
    {
        var b = await _context.Bookings
            .Include(x => x.Room).ThenInclude(r => r.RoomType)
            .Include(x => x.CheckIn)
            .Include(x => x.BookingServices)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (b is null) return (false, "Không tìm thấy đặt phòng.", null, null);
        if (b.Status != BookingStatus.CHECKED_IN) return (false, "Đặt phòng chưa nhận phòng, không thể trả.", null, null);

        var actualCheckOut = dto.ActualCheckOutTime ?? DateTime.UtcNow;
        var roomType = b.Room!.RoomType;
        var pricePerNight = roomType.PricePerNight;

        var actualCheckOutDateOnly = DateOnly.FromDateTime(actualCheckOut);
        int nightsStayed;
        string checkoutNote;
        decimal surcharge = 0;
        int lateHours = 0;
        BookingAction action = BookingAction.CHECKED_OUT;

        if (actualCheckOutDateOnly < b.CheckOutDate)
        {
            nightsStayed = BookingBusinessRules.NightsBetween(b.CheckInDate, actualCheckOutDateOnly);
            checkoutNote = $"Trả phòng sớm hơn dự kiến ({b.CheckOutDate}). Tính tiền {nightsStayed} đêm thực ở.";
            action = BookingAction.EARLY_CHECKOUT;
        }
        else if (actualCheckOutDateOnly == b.CheckOutDate)
        {
            nightsStayed = BookingBusinessRules.NightsBetween(b.CheckInDate, b.CheckOutDate);

            if (actualCheckOut.Hour >= BookingBusinessRules.LateCheckoutCutoffHour)
            {
                nightsStayed += 1;
                checkoutNote = "Trả phòng sau 18h - tính thêm 1 đêm.";
                action = BookingAction.LATE_CHECKOUT;
            }
            else if (actualCheckOut.Hour > BookingBusinessRules.StandardCheckOutHour)
            {
                lateHours = actualCheckOut.Hour - BookingBusinessRules.StandardCheckOutHour;
                surcharge = pricePerNight * BookingBusinessRules.LateCheckoutHourlyRate * lateHours;
                checkoutNote = $"Trả phòng trễ {lateHours} giờ so với giờ chuẩn ({BookingBusinessRules.StandardCheckOutHour}h). Phụ phí {surcharge:N0}đ.";
                action = BookingAction.LATE_CHECKOUT;
            }
            else
            {
                checkoutNote = "Trả phòng đúng giờ.";
            }
        }
        else
        {
            nightsStayed = BookingBusinessRules.NightsBetween(b.CheckInDate, actualCheckOutDateOnly);
            checkoutNote = "Trả phòng trễ ngày so với dự kiến (chưa gia hạn) - đã tính thêm đêm phát sinh.";
            action = BookingAction.LATE_CHECKOUT;
        }

        // ---------- Tách "thuê gốc" vs "gia hạn" để in rõ trên hóa đơn ----------
        var originalNights = BookingBusinessRules.NightsBetween(b.CheckInDate, b.OriginalCheckOutDate);
        var originalNightsCharged = Math.Min(nightsStayed, originalNights);
        var extendedNightsCharged = nightsStayed - originalNightsCharged;

        var roomAmount = nightsStayed * pricePerNight;
        var serviceAmount = b.BookingServices.Sum(bs => bs.Quantity * bs.UnitPrice);
        var totalAmount = roomAmount + serviceAmount + surcharge;

        // ---------- Xây chuỗi mô tả chi tiết cho hóa đơn ----------
        var lines = new List<string>();
        lines.Add($"Thuê gốc: {originalNightsCharged} đêm x {pricePerNight:N0}đ = {originalNightsCharged * pricePerNight:N0}đ");
        if (extendedNightsCharged > 0)
            lines.Add($"Gia hạn thêm: {extendedNightsCharged} đêm x {pricePerNight:N0}đ = {extendedNightsCharged * pricePerNight:N0}đ");
        if (lateHours > 0)
        {
            var hourlyRate = pricePerNight * BookingBusinessRules.LateCheckoutHourlyRate;
            lines.Add($"Thêm giờ (trễ so với giờ chuẩn): {lateHours} giờ x {hourlyRate:N0}đ/giờ = {surcharge:N0}đ");
        }
        if (serviceAmount > 0)
            lines.Add($"Dịch vụ đi kèm: {serviceAmount:N0}đ");
        lines.Add($"Tổng cộng: {totalAmount:N0}đ");
        var description = string.Join("\n", lines);

        _context.CheckOuts.Add(new CheckOut
        {
            Id = Guid.NewGuid(),
            BookingId = id,
            ActualCheckOut = actualCheckOut,
            EmployeeId = performedBy
        });

        _context.Invoices.Add(new Invoice
        {
            Id = Guid.NewGuid(),
            BookingId = id,
            InvoiceCode = "INV" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            RoomAmount = roomAmount,
            ServiceAmount = serviceAmount,
            Surcharge = surcharge,
            TotalAmount = totalAmount,
            Description = description,
            PaymentStatus = PaymentStatus.UNPAID,
            IssuedAt = DateTime.UtcNow
        });

        b.Status = BookingStatus.CHECKED_OUT;
        b.Room.Status = RoomStatus.CLEANING;

        await LogAsync(id, action, performedBy, checkoutNote);
        await _context.SaveChangesAsync();

        return (true, null, totalAmount, description);
    }

    public async Task<(bool Success, string? Error)> ExtendAsync(Guid id, ExtendBookingDto dto, Guid performedBy)
    {
        var b = await _context.Bookings.Include(x => x.Room).FirstOrDefaultAsync(x => x.Id == id);
        if (b is null) return (false, "Không tìm thấy đặt phòng.");
        if (b.Status != BookingStatus.CHECKED_IN)
            return (false, "Chỉ gia hạn được khi khách đang ở trong phòng (đã check-in).");

        var extensionType = dto.ExtensionType?.ToUpperInvariant();

        // ---------------- Gia hạn thêm ĐÊM ----------------
        if (extensionType == "DAYS")
        {
            if (dto.NewCheckOutDate is null || dto.NewCheckOutDate <= b.CheckOutDate)
                return (false, "Ngày trả phòng mới phải muộn hơn ngày hiện tại.");

            var noBookingConflict = await IsRoomAvailableAsync(b.RoomId, b.CheckOutDate, dto.NewCheckOutDate.Value, id);
            if (!noBookingConflict)
                return (false, "Phòng đã có khách khác đặt ngay sau ngày trả dự kiến, không thể gia hạn.");

            var hasWaitlist = await HasWaitlistConflictAsync(b.RoomId, b.CheckOutDate, dto.NewCheckOutDate.Value);
            if (hasWaitlist)
                return (false, "Có khách đang xếp hàng chờ phòng này trong khoảng thời gian gia hạn. Hãy liên hệ khách trong hàng chờ trước khi gia hạn.");

            var oldCheckOut = b.CheckOutDate;
            b.CheckOutDate = dto.NewCheckOutDate.Value;

            await LogAsync(id, BookingAction.EXTENDED, performedBy,
                $"Gia hạn thêm đêm: từ {oldCheckOut} sang {dto.NewCheckOutDate.Value} " +
                $"(đã kiểm tra không trùng đặt phòng khác và không có khách trong hàng chờ).");

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ---------------- Gia hạn thêm GIỜ (trong ngày trả dự kiến) ----------------
        if (extensionType == "HOURS")
        {
            if (dto.AdditionalHours is null || dto.AdditionalHours <= 0)
                return (false, "Số giờ gia hạn không hợp lệ.");
            if (dto.AdditionalHours > BookingBusinessRules.MaxApprovedExtraHours)
                return (false, $"Chỉ được gia hạn tối đa {BookingBusinessRules.MaxApprovedExtraHours} giờ. Nếu cần lâu hơn, hãy gia hạn thêm đêm.");

            var extensionDay = b.CheckOutDate;

            // Kiểm tra KHÔNG có khách khác nhận phòng đúng ngày này
            var hasArrivalSameDay = await _context.Bookings.AnyAsync(x =>
                x.RoomId == b.RoomId && x.Id != b.Id &&
                x.Status != BookingStatus.CANCELLED && x.Status != BookingStatus.CHECKED_OUT &&
                x.CheckInDate == extensionDay);

            // Kiểm tra KHÔNG có ai trong hàng chờ mong muốn nhận phòng đúng ngày này
            var hasWaitlistSameDay = await _context.RoomWaitlist.AnyAsync(w =>
                w.RoomId == b.RoomId &&
                (w.Status == WaitlistStatus.WAITING || w.Status == WaitlistStatus.NOTIFIED) &&
                w.DesiredCheckIn == extensionDay);

            if (hasArrivalSameDay || hasWaitlistSameDay)
                return (false, "Phòng đã có khách khác nhận phòng hoặc đang có người chờ đúng ngày này, " +
                    "không thể gia hạn thêm giờ (cần chừa thời gian cho lao công dọn phòng). " +
                    "Có thể đề nghị khách gia hạn thêm cả đêm nếu phòng còn trống, hoặc từ chối.");

            b.ApprovedExtraHours += dto.AdditionalHours.Value;

            await LogAsync(id, BookingAction.EXTENDED, performedBy,
                $"Duyệt gia hạn thêm {dto.AdditionalHours} giờ trong ngày {extensionDay:dd/MM/yyyy} " +
                $"(đã kiểm tra không có khách nhận phòng/hàng chờ cùng ngày, đảm bảo còn thời gian " +
                $"{BookingBusinessRules.HousekeepingBufferMinutes} phút cho lao công dọn phòng trước ca tiếp theo).");

            await _context.SaveChangesAsync();
            return (true, null);
        }

        return (false, "ExtensionType không hợp lệ (chỉ chấp nhận DAYS hoặc HOURS).");
    }

    public async Task<(bool Success, string? Error)> CancelAsync(Guid id, Guid performedBy)
    {
        var b = await _context.Bookings.Include(x => x.Room).FirstOrDefaultAsync(x => x.Id == id);
        if (b is null) return (false, "Không tìm thấy đặt phòng.");
        if (b.Status is BookingStatus.CHECKED_IN or BookingStatus.CHECKED_OUT)
            return (false, "Không thể hủy đặt phòng đã nhận/trả phòng.");

        b.Status = BookingStatus.CANCELLED;

        var stillHeld = await _context.Bookings.AnyAsync(x =>
            x.RoomId == b.RoomId && x.Id != b.Id &&
            (x.Status == BookingStatus.PENDING || x.Status == BookingStatus.CONFIRMED));

        if (!stillHeld && b.Room!.Status == RoomStatus.RESERVED)
            b.Room.Status = RoomStatus.AVAILABLE;

        await LogAsync(id, BookingAction.CANCELLED, performedBy);
        await _context.SaveChangesAsync();
        return (true, null);
    }
}