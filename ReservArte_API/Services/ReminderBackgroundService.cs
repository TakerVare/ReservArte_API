using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

/// <summary>
/// Servicio en segundo plano que ejecuta el procesamiento de recordatorios cada cierto intervalo.
/// Usa IHostedService + Timer (sin Hangfire). Intervalo configurable en appsettings "Reminder:IntervalMinutes" (default 5).
/// </summary>
public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderBackgroundService> _logger;
    private readonly int _intervalMinutes;

    public ReminderBackgroundService(IServiceProvider serviceProvider, ILogger<ReminderBackgroundService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _intervalMinutes = configuration.GetValue("Reminder:IntervalMinutes", 5);
        if (_intervalMinutes < 1) _intervalMinutes = 5;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderBackgroundService iniciado. Intervalo: {Interval} min", _intervalMinutes);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
                await reminderService.ProcessDueRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando recordatorios");
            }

            await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
        }
    }
}
