namespace WhatsAppIntegration.Configuration;

/// <summary>
/// Configuration for order lookup period
/// Environment variable: OrderLookupHours
/// </summary>
public class OrderLookupConfig
{
    /// <summary>
    /// Number of hours to look back for orders (from OrderLookupHours environment variable)
    /// Default: 48 hours
    /// </summary>
    public int LookupHours { get; set; } = 48;
}