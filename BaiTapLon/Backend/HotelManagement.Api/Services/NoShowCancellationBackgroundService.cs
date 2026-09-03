using HotelManagement.Api.Data;

namespace HotelManagement.Api.Services;

public class NoShowCancellationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

    public NoShowCancellationBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                await bookingService.ProcessNoShowCancellationsAsync(DataSeeder.SystemEmployeeId);
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}