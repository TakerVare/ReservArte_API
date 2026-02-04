using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

/// <summary>
/// Servicio en segundo plano que elimina fotos expiradas según RGPD.
/// Se ejecuta una vez al día para limpiar fotos que han superado su tiempo de retención.
/// </summary>
public class PhotoCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PhotoCleanupBackgroundService> _logger;
    
    // Ejecutar cada 24 horas
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public PhotoCleanupBackgroundService(
        IServiceProvider serviceProvider, 
        ILogger<PhotoCleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PhotoCleanupBackgroundService iniciado. Intervalo: {Interval} horas", _interval.TotalHours);
        
        // Esperar un poco al inicio para no interferir con el arranque
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Iniciando limpieza de fotos expiradas (RGPD)");
                
                using var scope = _serviceProvider.CreateScope();
                var photoService = scope.ServiceProvider.GetRequiredService<IServicePhotoService>();
                
                var deletedCount = await photoService.CleanupExpiredPhotosAsync();
                
                _logger.LogInformation("Limpieza RGPD completada. Fotos eliminadas: {Count}", deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la limpieza de fotos expiradas");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
