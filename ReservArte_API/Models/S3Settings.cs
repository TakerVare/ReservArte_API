namespace ReservArte_API.Models;

/// <summary>
/// Configuración para Amazon S3 - almacenamiento de fotografías.
/// </summary>
public class S3Settings
{
    public const string SectionName = "S3";
    
    /// <summary>
    /// Nombre del bucket de S3 para almacenar las fotografías.
    /// </summary>
    public string BucketName { get; set; } = string.Empty;
    
    /// <summary>
    /// Access Key de AWS.
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Secret Key de AWS.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Región de AWS donde está el bucket.
    /// </summary>
    public string Region { get; set; } = "eu-west-1";
    
    /// <summary>
    /// Años de retención de fotos antes de expiración automática (RGPD).
    /// Por defecto: 2 años.
    /// </summary>
    public int ExpirationYears { get; set; } = 2;
    
    /// <summary>
    /// Duración en minutos de las URLs pre-firmadas para acceso a fotos.
    /// Por defecto: 60 minutos.
    /// </summary>
    public int PresignedUrlExpirationMinutes { get; set; } = 60;
    
    /// <summary>
    /// Ruta al archivo de imagen del logo para marca de agua.
    /// Si está vacío, no se aplicará marca de agua.
    /// </summary>
    public string WatermarkLogoPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Opacidad de la marca de agua (0.0 a 1.0).
    /// </summary>
    public float WatermarkOpacity { get; set; } = 0.3f;
}
