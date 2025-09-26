namespace WhatsAppIntegration.BackgroundService.Configuration;

/// <summary>
/// Configuration settings for the background sync service
/// </summary>
public class SyncServiceConfig
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "SyncService";

    /// <summary>
    /// How often to run the sync (in hours). Default: 24 hours
    /// </summary>
    public int IntervalHours { get; set; } = 24;

    /// <summary>
    /// How many hours back to look for data when syncing. Default: 48 hours
    /// </summary>
    public int LookbackHours { get; set; } = 48;

    /// <summary>
    /// Base URL of the WhatsApp Integration API
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://localhost:7021";

    /// <summary>
    /// API endpoint for the PUT request
    /// </summary>
    public string ApiEndpoint { get; set; } = "/api/shopify/orders/by-customer/categorized";

    /// <summary>
    /// Maximum number of orders to retrieve per sync
    /// </summary>
    public int? MaxOrdersLimit { get; set; } = null;

    /// <summary>
    /// Minimum orders per customer to be included
    /// </summary>
    public int? MinOrdersPerCustomer { get; set; } = 3;

    /// <summary>
    /// Order status filter (any, open, closed, cancelled)
    /// </summary>
    public string OrderStatus { get; set; } = "any";

    /// <summary>
    /// Whether to run the sync immediately on startup
    /// </summary>
    public bool RunOnStartup { get; set; } = true;

    /// <summary>
    /// Timeout for HTTP requests in seconds
    /// </summary>
    public int HttpTimeoutSeconds { get; set; } = 300; // 5 minutes
}