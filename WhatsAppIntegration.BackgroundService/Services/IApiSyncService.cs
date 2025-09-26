namespace WhatsAppIntegration.BackgroundService.Services;

/// <summary>
/// Interface for syncing data with the WhatsApp Integration API
/// </summary>
public interface IApiSyncService
{
    /// <summary>
    /// Synchronize categorized orders for the specified time range
    /// </summary>
    /// <param name="hoursBack">How many hours back to look for data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of customers processed, or -1 if failed</returns>
    Task<int> SyncCategorizedOrdersAsync(int hoursBack, CancellationToken cancellationToken = default);
}