/*
 * RedsysWebhookDto.cs
 * ===================
 * 
 * PROPÓSITO:
 * DTOs para recibir y procesar notificaciones webhook de Redsys.
 * 
 * DÓNDE SE USA:
 * - PaymentController.cs: POST /api/payment/webhook (sin autenticación)
 * - PaymentService.cs: ProcessWebhookAsync()
 * - RedsysService.cs: ValidateWebhookSignature(), ParseWebhookData()
 * 
 * PARA QUÉ SE USA:
 * - Recibir notificaciones asíncronas de Redsys sobre el estado de transacciones
 * - Actualizar estado de pagos en base de datos
 * - Validar firma HMAC SHA-256 para seguridad
 * - Redsys envía notificación a DS_MERCHANT_MERCHANTURL
 */

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para recibir webhook de Redsys
/// Los datos vienen en formato form-urlencoded
/// </summary>
public class RedsysWebhookDto
{
    /// <summary>
    /// Versión de la firma (HMAC_SHA256_V1)
    /// </summary>
    public string Ds_SignatureVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// Datos de la operación codificados en Base64
    /// Contiene JSON con los parámetros de respuesta
    /// </summary>
    public string Ds_MerchantParameters { get; set; } = string.Empty;
    
    /// <summary>
    /// Firma HMAC SHA-256 para validar autenticidad
    /// </summary>
    public string Ds_Signature { get; set; } = string.Empty;
}

/// <summary>
/// Datos decodificados del webhook (contenido de Ds_MerchantParameters)
/// </summary>
public class RedsysWebhookDataDto
{
    /// <summary>
    /// Fecha de la operación (formato: ddMMYYYY)
    /// </summary>
    public string? Ds_Date { get; set; }
    
    /// <summary>
    /// Hora de la operación (formato: HH:mm)
    /// </summary>
    public string? Ds_Hour { get; set; }
    
    /// <summary>
    /// Importe en céntimos
    /// </summary>
    public string? Ds_Amount { get; set; }
    
    /// <summary>
    /// Código de moneda
    /// </summary>
    public string? Ds_Currency { get; set; }
    
    /// <summary>
    /// Número de pedido (12 caracteres)
    /// </summary>
    public string? Ds_Order { get; set; }
    
    /// <summary>
    /// Código del comercio (FUC)
    /// </summary>
    public string? Ds_MerchantCode { get; set; }
    
    /// <summary>
    /// Número de terminal
    /// </summary>
    public string? Ds_Terminal { get; set; }
    
    /// <summary>
    /// Código de respuesta (0000-0099 = éxito)
    /// </summary>
    public string? Ds_Response { get; set; }
    
    /// <summary>
    /// Datos del comercio enviados en la petición original
    /// </summary>
    public string? Ds_MerchantData { get; set; }
    
    /// <summary>
    /// Tipo de transacción (0, 1, 2, 3, 9)
    /// </summary>
    public string? Ds_TransactionType { get; set; }
    
    /// <summary>
    /// Código de autorización del banco
    /// </summary>
    public string? Ds_AuthorisationCode { get; set; }
    
    /// <summary>
    /// Tipo de tarjeta (C=crédito, D=débito)
    /// </summary>
    public string? Ds_Card_Type { get; set; }
    
    /// <summary>
    /// País de emisión de la tarjeta (ISO 3166-1)
    /// </summary>
    public string? Ds_Card_Country { get; set; }
    
    /// <summary>
    /// Marca de la tarjeta (1=Visa, 2=Mastercard, etc.)
    /// </summary>
    public string? Ds_Card_Brand { get; set; }
    
    /// <summary>
    /// PAN enmascarado de la tarjeta
    /// </summary>
    public string? Ds_Card_Number { get; set; }
    
    /// <summary>
    /// Fecha de caducidad (formato: YYMM)
    /// </summary>
    public string? Ds_ExpiryDate { get; set; }
    
    /// <summary>
    /// Identificador de tarjeta para tokenización (COF)
    /// </summary>
    public string? Ds_Merchant_Identifier { get; set; }
    
    /// <summary>
    /// ID de transacción COF
    /// </summary>
    public string? Ds_Merchant_Cof_Txnid { get; set; }
    
    /// <summary>
    /// Código de error EMV3DS
    /// </summary>
    public string? Ds_EMV3DS { get; set; }
    
    /// <summary>
    /// Indica si la respuesta fue exitosa (código 0000-0099)
    /// </summary>
    public bool IsSuccessful
    {
        get
        {
            if (string.IsNullOrEmpty(Ds_Response))
                return false;
            
            if (int.TryParse(Ds_Response, out int code))
                return code >= 0 && code <= 99;
            
            return false;
        }
    }
    
    /// <summary>
    /// Obtiene el importe en euros (convierte de céntimos)
    /// </summary>
    public decimal GetAmountInEuros()
    {
        if (string.IsNullOrEmpty(Ds_Amount))
            return 0;
        
        if (decimal.TryParse(Ds_Amount, out decimal cents))
            return cents / 100;
        
        return 0;
    }
    
    /// <summary>
    /// Obtiene la marca de tarjeta como texto
    /// </summary>
    public string GetCardBrandName()
    {
        return Ds_Card_Brand switch
        {
            "1" => "Visa",
            "2" => "Mastercard",
            "6" => "Diners Club",
            "7" => "Private Card",
            "8" => "American Express",
            "9" => "JCB",
            "22" => "Bizum",
            _ => "Desconocida"
        };
    }
    
    /// <summary>
    /// Obtiene mensaje descriptivo del código de respuesta
    /// </summary>
    public string GetResponseMessage()
    {
        if (string.IsNullOrEmpty(Ds_Response))
            return "Sin respuesta";
        
        if (!int.TryParse(Ds_Response, out int code))
            return $"Código no válido: {Ds_Response}";
        
        return code switch
        {
            >= 0 and <= 99 => "Transacción autorizada",
            101 => "Tarjeta caducada",
            102 => "Tarjeta bloqueada temporalmente",
            104 => "Operación no permitida",
            106 => "Intentos de PIN excedidos",
            116 => "Saldo insuficiente",
            118 => "Tarjeta no registrada",
            125 => "Tarjeta no efectiva",
            129 => "CVV2/CVC2 incorrecto",
            180 => "Tarjeta no válida para este servicio",
            184 => "Error en autenticación del titular",
            190 => "Denegación sin especificar motivo",
            191 => "Fecha de caducidad errónea",
            202 => "Tarjeta bloqueada por robo",
            904 => "Comercio no registrado",
            909 => "Error de sistema",
            912 => "Emisor no disponible",
            913 => "Pedido repetido",
            944 => "Sesión incorrecta",
            950 => "Operación de devolución no permitida",
            9064 => "Número de posiciones de tarjeta incorrecto",
            9078 => "Tipo de operación no permitida",
            9093 => "Tarjeta no existe",
            9094 => "Rechazo internacional",
            9104 => "Comercio con titular seguro",
            9218 => "Operaciones seguras no permitidas",
            9253 => "Tarjeta no cumple check-digit",
            9256 => "Preautorización no permitida",
            9257 => "Preautorización no permitida",
            9261 => "Operación detenida por exceder límite",
            9912 => "Emisor no disponible",
            9913 => "Error en confirmación del comercio",
            9914 => "Confirmación KO del comercio",
            9915 => "Pago cancelado por usuario",
            9928 => "Anulación autorización en diferido",
            9997 => "Operación en proceso",
            9998 => "Operación de solicitud de datos de tarjeta",
            9999 => "Operación hacia SIS redirigida",
            _ => $"Error no catalogado: {code}"
        };
    }
}
