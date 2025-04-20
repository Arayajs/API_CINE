using API_CINE.Models.DTOs;
using API_CINE.Services.Interfaces;

namespace API_CINE.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILoggingService _loggingService;

        public PaymentService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILoggingService loggingService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _loggingService = loggingService;

            // Configurar el cliente HTTP para el servicio de pago
            string apiUrl = _configuration["PaymentSettings:ApiUrl"];
            string apiKey = _configuration["PaymentSettings:ApiKey"];
            bool useSandbox = bool.Parse(_configuration["PaymentSettings:UseSandbox"] ?? "true");

            _httpClient.BaseAddress = new Uri(apiUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("X-Sandbox", useSandbox.ToString().ToLower());
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                // En un entorno real, aquí se enviaría una solicitud al proveedor de pago
                // Para este ejemplo, simulamos un proceso de pago exitoso

                // Validaciones básicas
                if (request.Amount <= 0)
                    throw new ApplicationException("El monto debe ser mayor a 0");

                if (string.IsNullOrEmpty(request.PaymentMethod))
                    throw new ApplicationException("El método de pago es obligatorio");

                // Si es pago con tarjeta, validar datos de tarjeta
                if (request.PaymentMethod.Equals("CreditCard", StringComparison.OrdinalIgnoreCase) ||
                    request.PaymentMethod.Equals("DebitCard", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(request.CardNumber) || request.CardNumber.Length != 16 ||
                        !long.TryParse(request.CardNumber, out _))
                        throw new ApplicationException("Número de tarjeta inválido");

                    if (string.IsNullOrEmpty(request.ExpiryDate) || request.ExpiryDate.Length != 5)
                        throw new ApplicationException("Fecha de expiración inválida");

                    if (string.IsNullOrEmpty(request.Cvv) || request.Cvv.Length != 3 || !int.TryParse(request.Cvv, out _))
                        throw new ApplicationException("CVV inválido");
                }

                // Generar un ID de transacción único
                string transactionId = Guid.NewGuid().ToString("N");

                // Simulación de un proceso de pago exitoso
                // En un entorno real, aquí se comunicaría con la API del proveedor de pago

                // Intentamos usar el modo sandbox si está configurado
                if (bool.Parse(_configuration["PaymentSettings:UseSandbox"] ?? "true"))
                {
                    // Simular un pago exitoso sin contactar al proveedor externo
                    _loggingService.LogInformation($"Procesando pago en modo sandbox: {request.Amount:C} con método {request.PaymentMethod}");

                    // Simular un retraso en el proceso de pago
                    await Task.Delay(500); // Retraso de 500ms para simular el procesamiento

                    return new PaymentResponse
                    {
                        Success = true,
                        TransactionId = transactionId,
                        Message = "Pago procesado con éxito (modo sandbox)"
                    };
                }
                else
                {
                    // En un entorno de producción, enviaríamos la solicitud al proveedor de pago
                    // Para este ejemplo, simulamos una llamada exitosa

                    _loggingService.LogInformation($"Procesando pago real: {request.Amount:C} con método {request.PaymentMethod}");

                    var paymentData = new
                    {
                        amount = request.Amount,
                        method = request.PaymentMethod,
                        card_number = MaskCardNumber(request.CardNumber),
                        expiry_date = request.ExpiryDate,
                        cvv = "***", // No enviar el CVV real en el registro
                        description = "Compra de entradas de cine"
                    };

                    try
                    {
                        // Simulación de una llamada API (sin enviar realmente datos de tarjeta)
                        await Task.Delay(1000); // Simular tiempo de respuesta del API

                        return new PaymentResponse
                        {
                            Success = true,
                            TransactionId = transactionId,
                            Message = "Pago procesado con éxito"
                        };
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError($"Error al comunicarse con el proveedor de pago: {ex.Message}", ex);
                        throw new ApplicationException("Error al procesar el pago: No se pudo contactar al proveedor de pago");
                    }
                }
            }
            catch (ApplicationException ex)
            {
                _loggingService.LogWarning($"Error en validación de pago: {ex.Message}");
                return new PaymentResponse
                {
                    Success = false,
                    TransactionId = null,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al procesar pago: {ex.Message}", ex);
                return new PaymentResponse
                {
                    Success = false,
                    TransactionId = null,
                    Message = "Error al procesar el pago. Por favor, inténtelo de nuevo más tarde."
                };
            }
        }

        public async Task<bool> VerifyPaymentAsync(string transactionId)
        {
            try
            {
                // En un entorno real, aquí se enviaría una solicitud al proveedor de pago
                // Para verificar el estado de la transacción

                if (string.IsNullOrEmpty(transactionId))
                    throw new ApplicationException("ID de transacción inválido");

                // Simulamos una verificación exitosa
                _loggingService.LogInformation($"Verificando pago con ID de transacción: {transactionId}");

                // Simular un retraso en la verificación
                await Task.Delay(300); // Retraso para simular la verificación

                return true; // Asumimos que el pago es válido
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al verificar pago con ID {transactionId}: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<bool> RefundPaymentAsync(string transactionId, decimal amount)
        {
            try
            {
                // En un entorno real, aquí se enviaría una solicitud al proveedor de pago
                // Para procesar un reembolso

                if (string.IsNullOrEmpty(transactionId))
                    throw new ApplicationException("ID de transacción inválido");

                if (amount <= 0)
                    throw new ApplicationException("El monto a reembolsar debe ser mayor a 0");

                // Simulamos un reembolso exitoso
                _loggingService.LogInformation($"Procesando reembolso de {amount:C} para transacción: {transactionId}");

                // Simular un retraso en el reembolso
                await Task.Delay(500); // Retraso para simular el procesamiento

                return true; // Asumimos que el reembolso fue exitoso
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al reembolsar pago con ID {transactionId}: {ex.Message}", ex);
                throw;
            }
        }

        // Método para ocultar el número de tarjeta excepto los últimos 4 dígitos
        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return "****";

            return "************" + cardNumber.Substring(cardNumber.Length - 4);
        }
    }
}