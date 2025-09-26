using System.Text.Json;
using Microsoft.Extensions.Options;
using WhatsAppIntegration.BackgroundService.Configuration;

namespace WhatsAppIntegration.BackgroundService.Services;

/// <summary>
/// Service for syncing data with the WhatsApp Integration API
/// </summary>
public class ApiSyncService : IApiSyncService
{
    private readonly HttpClient _httpClient;
    private readonly SyncServiceConfig _config;
    private readonly ILogger<ApiSyncService> _logger;

    public ApiSyncService(HttpClient httpClient, IOptions<SyncServiceConfig> config, ILogger<ApiSyncService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
        
        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_config.ApiBaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.HttpTimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "WhatsAppIntegration.BackgroundService/1.0");
    }

    public async Task<int> SyncCategorizedOrdersAsync(int hoursBack, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting sync for categorized orders looking back {HoursBack} hours", hoursBack);

            // Calculate date range
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddHours(-hoursBack);

            // Build query parameters
            var queryParams = new List<string>();
            
            if (_config.MaxOrdersLimit.HasValue)
                queryParams.Add($"limit={_config.MaxOrdersLimit.Value}");
            
            if (_config.MinOrdersPerCustomer.HasValue)
                queryParams.Add($"minOrdersPerCustomer={_config.MinOrdersPerCustomer.Value}");
            
            queryParams.Add($"status={_config.OrderStatus}");
            queryParams.Add($"createdAtMin={startDate:yyyy-MM-ddTHH:mm:ss.fffZ}");
            queryParams.Add($"createdAtMax={endDate:yyyy-MM-ddTHH:mm:ss.fffZ}");

            var queryString = string.Join("&", queryParams);
            var requestUri = $"{_config.ApiEndpoint}?{queryString}";

            _logger.LogDebug("Making PUT request to: {RequestUri}", requestUri);

            // Make PUT request
            var response = await _httpClient.PutAsync(requestUri, null, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("API request failed with status {StatusCode}: {ErrorContent}", 
                    response.StatusCode, errorContent);
                return -1;
            }

            // Parse response
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<SyncResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (responseData == null)
            {
                _logger.LogError("Failed to parse API response: {ResponseContent}", responseContent);
                return -1;
            }

            _logger.LogInformation("Successfully synchronized {CustomerCount} customers (IDs: {CustomerIds})", 
                responseData.ProcessedCustomersCount, 
                string.Join(", ", responseData.ProcessedCustomerIds));

            return responseData.ProcessedCustomersCount;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "HTTP request timed out after {TimeoutSeconds} seconds", _config.HttpTimeoutSeconds);
            return -1;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed: {Message}", ex.Message);
            return -1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sync: {Message}", ex.Message);
            return -1;
        }
    }

    /// <summary>
    /// Response model matching the API's CategorizedOrdersProcessedResponse
    /// </summary>
    private class SyncResponse
    {
        public List<long> ProcessedCustomerIds { get; set; } = new();
        public int ProcessedCustomersCount { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}