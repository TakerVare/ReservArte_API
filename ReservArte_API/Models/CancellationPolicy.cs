namespace ReservArte_API.Models;

/// <summary>
/// Políticas de cancelación configurables por organización
/// </summary>
public class CancellationPolicy
{
    public int Id { get; set; }
    
    /// <summary>
    /// Horas mínimas de anticipación para cancelar sin penalización
    /// </summary>
    public int MinHoursBeforeCancel { get; set; } = 24;
    
    /// <summary>
    /// Porcentaje de penalización si cancela tarde (0-100)
    /// </summary>
    public int PenaltyPercentage { get; set; } = 50;
    
    /// <summary>
    /// Número máximo de no-shows antes de bloquear al cliente
    /// </summary>
    public int MaxNoShowsBeforeBlock { get; set; } = 3;
    
    /// <summary>
    /// Horas mínimas para VIP (nullable usa el valor general)
    /// </summary>
    public int? VipMinHoursBeforeCancel { get; set; }
    
    /// <summary>
    /// Porcentaje de penalización para VIP (nullable usa el valor general)
    /// </summary>
    public int? VipPenaltyPercentage { get; set; }
    
    /// <summary>
    /// Si la política está activa
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
