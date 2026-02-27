namespace ReservArte_API.Models;

/// <summary>
/// Métodos de pago guardados del cliente (tokenización Redsys)
/// </summary>
public class CustomerPaymentMethod
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string RedsysToken { get; set; } = string.Empty;
    public string? RedsysCofTxnid { get; set; }
    public string CardLast4 { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty;
    public string CardExpiry { get; set; } = string.Empty; // Formato AAMM
    public bool IsDefault { get; set; } = false;
    /// <summary>Soft delete: false cuando el registro está "eliminado".</summary>
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Verifica si la tarjeta está caducada
    /// </summary>
    public bool IsExpired
    {
        get
        {
            if (string.IsNullOrEmpty(CardExpiry) || CardExpiry.Length != 4)
                return true;
                
            if (!int.TryParse(CardExpiry.Substring(0, 2), out int year) ||
                !int.TryParse(CardExpiry.Substring(2, 2), out int month))
                return true;
                
            var expiryDate = new DateTime(2000 + year, month, 1).AddMonths(1).AddDays(-1);
            return expiryDate < DateTime.UtcNow.Date;
        }
    }
    
    /// <summary>
    /// Obtiene la fecha de expiración formateada (MM/AA)
    /// </summary>
    public string FormattedExpiry
    {
        get
        {
            if (string.IsNullOrEmpty(CardExpiry) || CardExpiry.Length != 4)
                return "??/??";
            return $"{CardExpiry.Substring(2, 2)}/{CardExpiry.Substring(0, 2)}";
        }
    }
}
