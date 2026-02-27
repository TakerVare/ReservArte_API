namespace ReservArte_API.Models;

/// <summary>
/// Estados posibles de una cita
/// </summary>
public static class Status
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
    public const string CancelledByCustomer = "cancelled_by_customer";
    public const string CancelledByBusiness = "cancelled_by_business";
    public const string NoShow = "no_show";
    
    // Mantener "cancelled" para compatibilidad hacia atrás
    public const string Cancelled = "cancelled";
    
    public static readonly string[] All = { 
        Pending, Confirmed, InProgress, Completed, 
        CancelledByCustomer, CancelledByBusiness, NoShow, Cancelled 
    };
    
    public static readonly string[] Active = { Pending, Confirmed, InProgress };
    public static readonly string[] Cancellable = { Pending, Confirmed };
    public static readonly string[] Final = { Completed, CancelledByCustomer, CancelledByBusiness, NoShow, Cancelled };
    
    public static bool IsValid(string? status)
    {
        return !string.IsNullOrEmpty(status) && All.Contains(status);
    }
    
    public static bool IsCancellable(string? status)
    {
        return !string.IsNullOrEmpty(status) && Cancellable.Contains(status);
    }
    
    public static bool IsFinal(string? status)
    {
        return !string.IsNullOrEmpty(status) && Final.Contains(status);
    }

    /// <summary>
    /// Devuelve el valor canónico del estado (snake_case) si el input coincide con alguno (case-insensitive).
    /// Acepta por ejemplo "CancelledByCustomer" o "cancelled_by_customer".
    /// </summary>
    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input.Trim();
        return All.FirstOrDefault(s => string.Equals(s, trimmed, StringComparison.OrdinalIgnoreCase));
    }
}

