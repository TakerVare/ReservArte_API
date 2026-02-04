using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

/// <summary>
/// Servicio de integración con la pasarela de pagos Redsys
/// Implementa comunicación REST con firma HMAC SHA-256
/// </summary>
public class RedsysService : IRedsysService
{
    private readonly RedsysSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<RedsysService> _logger;

    public RedsysService(
        IOptions<RedsysSettings> settings,
        HttpClient httpClient,
        ILogger<RedsysService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    #region Pre-autorización

    public async Task<RedsysOperationResult> CreatePreAuthorizationAsync(
        string orderNumber,
        decimal amount,
        string idOper,
        string? merchantData = null)
    {
        if (!IsConfigured())
            return RedsysOperationResult.NotConfigured();

        try
        {
            var parameters = new Dictionary<string, string>
            {
                ["DS_MERCHANT_AMOUNT"] = AmountToCents(amount),
                ["DS_MERCHANT_ORDER"] = orderNumber,
                ["DS_MERCHANT_MERCHANTCODE"] = _settings.MerchantCode,
                ["DS_MERCHANT_CURRENCY"] = _settings.Currency,
                ["DS_MERCHANT_TRANSACTIONTYPE"] = RedsysTransactionType.PreAuthorization,
                ["DS_MERCHANT_TERMINAL"] = _settings.Terminal,
                ["DS_MERCHANT_IDOPER"] = idOper,
                ["DS_MERCHANT_MERCHANTURL"] = _settings.NotificationUrl
            };

            if (!string.IsNullOrEmpty(merchantData))
                parameters["DS_MERCHANT_MERCHANTDATA"] = merchantData;

            return await SendRequestAsync(parameters, orderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en pre-autorización Redsys. Order: {OrderNumber}", orderNumber);
            return RedsysOperationResult.Failed($"Error de comunicación: {ex.Message}");
        }
    }

    public async Task<RedsysOperationResult> CreatePreAuthorizationWithTokenAsync(
        string orderNumber,
        decimal amount,
        string cardToken,
        string? cofTxnId = null,
        string? merchantData = null)
    {
        if (!IsConfigured())
            return RedsysOperationResult.NotConfigured();

        try
        {
            var parameters = new Dictionary<string, string>
            {
                ["DS_MERCHANT_AMOUNT"] = AmountToCents(amount),
                ["DS_MERCHANT_ORDER"] = orderNumber,
                ["DS_MERCHANT_MERCHANTCODE"] = _settings.MerchantCode,
                ["DS_MERCHANT_CURRENCY"] = _settings.Currency,
                ["DS_MERCHANT_TRANSACTIONTYPE"] = RedsysTransactionType.PreAuthorization,
                ["DS_MERCHANT_TERMINAL"] = _settings.Terminal,
                ["DS_MERCHANT_IDENTIFIER"] = cardToken,
                ["DS_MERCHANT_COF_INI"] = "N", // No es primera transacción
                ["DS_MERCHANT_COF_TYPE"] = "R", // Recurrente
                ["DS_MERCHANT_DIRECTPAYMENT"] = "true",
                ["DS_MERCHANT_MERCHANTURL"] = _settings.NotificationUrl
            };

            if (!string.IsNullOrEmpty(cofTxnId))
                parameters["DS_MERCHANT_COF_TXNID"] = cofTxnId;

            if (!string.IsNullOrEmpty(merchantData))
                parameters["DS_MERCHANT_MERCHANTDATA"] = merchantData;

            return await SendRequestAsync(parameters, orderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en pre-autorización con token. Order: {OrderNumber}", orderNumber);
            return RedsysOperationResult.Failed($"Error de comunicación: {ex.Message}");
        }
    }

    #endregion

    #region Confirmación/Captura

    public async Task<RedsysOperationResult> ConfirmPreAuthorizationAsync(
        string orderNumber,
        decimal amount)
    {
        if (!IsConfigured())
            return RedsysOperationResult.NotConfigured();

        try
        {
            var parameters = new Dictionary<string, string>
            {
                ["DS_MERCHANT_AMOUNT"] = AmountToCents(amount),
                ["DS_MERCHANT_ORDER"] = orderNumber,
                ["DS_MERCHANT_MERCHANTCODE"] = _settings.MerchantCode,
                ["DS_MERCHANT_CURRENCY"] = _settings.Currency,
                ["DS_MERCHANT_TRANSACTIONTYPE"] = RedsysTransactionType.Confirmation,
                ["DS_MERCHANT_TERMINAL"] = _settings.Terminal
            };

            return await SendRequestAsync(parameters, orderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en confirmación Redsys. Order: {OrderNumber}", orderNumber);
            return RedsysOperationResult.Failed($"Error de comunicación: {ex.Message}");
        }
    }

    #endregion

    #region Cancelación

    public async Task<RedsysOperationResult> CancelPreAuthorizationAsync(
        string orderNumber,
        decimal amount)
    {
        if (!IsConfigured())
            return RedsysOperationResult.NotConfigured();

        try
        {
            var parameters = new Dictionary<string, string>
            {
                ["DS_MERCHANT_AMOUNT"] = AmountToCents(amount),
                ["DS_MERCHANT_ORDER"] = orderNumber,
                ["DS_MERCHANT_MERCHANTCODE"] = _settings.MerchantCode,
                ["DS_MERCHANT_CURRENCY"] = _settings.Currency,
                ["DS_MERCHANT_TRANSACTIONTYPE"] = RedsysTransactionType.Cancellation,
                ["DS_MERCHANT_TERMINAL"] = _settings.Terminal
            };

            return await SendRequestAsync(parameters, orderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en cancelación Redsys. Order: {OrderNumber}", orderNumber);
            return RedsysOperationResult.Failed($"Error de comunicación: {ex.Message}");
        }
    }

    #endregion

    #region Reembolso

    public async Task<RedsysOperationResult> ProcessRefundAsync(
        string orderNumber,
        decimal amount)
    {
        if (!IsConfigured())
            return RedsysOperationResult.NotConfigured();

        try
        {
            var parameters = new Dictionary<string, string>
            {
                ["DS_MERCHANT_AMOUNT"] = AmountToCents(amount),
                ["DS_MERCHANT_ORDER"] = orderNumber,
                ["DS_MERCHANT_MERCHANTCODE"] = _settings.MerchantCode,
                ["DS_MERCHANT_CURRENCY"] = _settings.Currency,
                ["DS_MERCHANT_TRANSACTIONTYPE"] = RedsysTransactionType.AutomaticRefund,
                ["DS_MERCHANT_TERMINAL"] = _settings.Terminal
            };

            return await SendRequestAsync(parameters, orderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en reembolso Redsys. Order: {OrderNumber}", orderNumber);
            return RedsysOperationResult.Failed($"Error de comunicación: {ex.Message}");
        }
    }

    #endregion

    #region Webhooks

    public bool ValidateWebhookSignature(RedsysWebhookDto webhook)
    {
        if (string.IsNullOrEmpty(webhook.Ds_MerchantParameters) ||
            string.IsNullOrEmpty(webhook.Ds_Signature))
            return false;

        try
        {
            // Decodificar merchantParameters para obtener el order number
            var data = ParseWebhookData(webhook.Ds_MerchantParameters);
            if (data == null || string.IsNullOrEmpty(data.Ds_Order))
                return false;

            // Generar firma esperada
            var expectedSignature = GenerateSignature(webhook.Ds_MerchantParameters, data.Ds_Order);

            // Comparar firmas (URL-safe base64)
            var receivedSignature = webhook.Ds_Signature
                .Replace('-', '+')
                .Replace('_', '/');

            return string.Equals(expectedSignature, receivedSignature, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando firma de webhook Redsys");
            return false;
        }
    }

    public RedsysWebhookDataDto? ParseWebhookData(string merchantParameters)
    {
        try
        {
            // Decodificar Base64
            var jsonBytes = Convert.FromBase64String(merchantParameters);
            var json = Encoding.UTF8.GetString(jsonBytes);

            // Deserializar JSON
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<RedsysWebhookDataDto>(json, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parseando datos de webhook Redsys");
            return null;
        }
    }

    #endregion

    #region Utilidades

    public bool IsConfigured()
    {
        return _settings.IsValid();
    }

    public string GenerateSignature(string merchantParameters, string orderNumber)
    {
        // 1. Decodificar clave secreta de Base64
        var key = Convert.FromBase64String(_settings.SecretKey);

        // 2. Diversificar clave con 3DES usando el número de pedido
        var diversifiedKey = Encrypt3DES(orderNumber, key);

        // 3. Calcular HMAC SHA-256
        using var hmac = new HMACSHA256(diversifiedKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(merchantParameters));

        // 4. Codificar en Base64
        return Convert.ToBase64String(hash);
    }

    public string AmountToCents(decimal amount)
    {
        // Redsys espera el importe en céntimos sin decimales
        return ((int)(amount * 100)).ToString();
    }

    public decimal CentsToAmount(string cents)
    {
        if (decimal.TryParse(cents, out decimal value))
            return value / 100;
        return 0;
    }

    #endregion

    #region Métodos Privados

    private async Task<RedsysOperationResult> SendRequestAsync(
        Dictionary<string, string> parameters,
        string orderNumber)
    {
        // 1. Serializar parámetros a JSON
        var json = JsonSerializer.Serialize(parameters);

        // 2. Codificar en Base64
        var merchantParameters = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        // 3. Generar firma
        var signature = GenerateSignature(merchantParameters, orderNumber);

        // 4. Crear request body
        var requestBody = new Dictionary<string, string>
        {
            ["Ds_SignatureVersion"] = "HMAC_SHA256_V1",
            ["Ds_MerchantParameters"] = merchantParameters,
            ["Ds_Signature"] = signature
        };

        var content = new FormUrlEncodedContent(requestBody);

        // 5. Enviar petición
        _logger.LogInformation("Enviando petición a Redsys. Order: {OrderNumber}", orderNumber);
        
        var response = await _httpClient.PostAsync(_settings.GetRestEndpoint(), content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger.LogDebug("Respuesta Redsys: {Response}", responseContent);

        // 6. Parsear respuesta
        return ParseResponse(responseContent, orderNumber);
    }

    private RedsysOperationResult ParseResponse(string responseContent, string orderNumber)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            // Verificar si hay error
            if (root.TryGetProperty("errorCode", out var errorCode))
            {
                var errorMsg = errorCode.GetString();
                _logger.LogWarning("Error Redsys: {ErrorCode}", errorMsg);
                return RedsysOperationResult.Failed($"Error Redsys: {errorMsg}");
            }

            // Obtener Ds_MerchantParameters
            if (!root.TryGetProperty("Ds_MerchantParameters", out var merchantParams))
            {
                return RedsysOperationResult.Failed("Respuesta sin Ds_MerchantParameters");
            }

            // Decodificar y parsear
            var data = ParseWebhookData(merchantParams.GetString() ?? "");
            if (data == null)
            {
                return RedsysOperationResult.Failed("Error parseando respuesta");
            }

            // Crear resultado
            var result = new RedsysOperationResult
            {
                Success = data.IsSuccessful,
                OrderNumber = data.Ds_Order,
                AuthCode = data.Ds_AuthorisationCode,
                ResponseCode = data.Ds_Response,
                ResponseMessage = data.GetResponseMessage(),
                CardNumber = data.Ds_Card_Number,
                CardBrand = data.GetCardBrandName(),
                CardToken = data.Ds_Merchant_Identifier,
                CofTxnId = data.Ds_Merchant_Cof_Txnid,
                ExpiryDate = data.Ds_ExpiryDate,
                RawResponse = responseContent
            };

            if (!result.Success)
            {
                result.ErrorMessage = data.GetResponseMessage();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parseando respuesta Redsys");
            return RedsysOperationResult.Failed($"Error parseando respuesta: {ex.Message}");
        }
    }

    private static byte[] Encrypt3DES(string data, byte[] key)
    {
        // Asegurar que la clave tiene 24 bytes para 3DES
        var key24 = new byte[24];
        Array.Copy(key, 0, key24, 0, Math.Min(key.Length, 24));
        if (key.Length < 24)
        {
            // Rellenar con los primeros bytes si es necesario
            Array.Copy(key, 0, key24, 16, Math.Min(key.Length, 8));
        }

        // Preparar datos (rellenar con ceros hasta múltiplo de 8)
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var paddedLength = ((dataBytes.Length + 7) / 8) * 8;
        var paddedData = new byte[paddedLength];
        Array.Copy(dataBytes, paddedData, dataBytes.Length);

        // Cifrar con 3DES en modo CBC con IV de ceros
        using var tdes = TripleDES.Create();
        tdes.Key = key24;
        tdes.Mode = CipherMode.CBC;
        tdes.Padding = PaddingMode.None;
        tdes.IV = new byte[8]; // IV de ceros

        using var encryptor = tdes.CreateEncryptor();
        return encryptor.TransformFinalBlock(paddedData, 0, paddedData.Length);
    }

    #endregion
}
