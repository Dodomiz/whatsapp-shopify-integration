using Microsoft.Extensions.Options;
using WhatsAppIntegration.BackgroundService.Configuration;
using WhatsAppIntegration.BackgroundService.Services;

namespace WhatsAppIntegration.BackgroundService;

public class Worker : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IApiSyncService _apiSyncService;
    private readonly SyncServiceConfig _config;

    public Worker(ILogger<Worker> logger, IApiSyncService apiSyncService, IOptions<SyncServiceConfig> config)
    {
        _logger = logger;
        _apiSyncService = apiSyncService;
        _config = config.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WhatsApp Integration Background Service started. Interval: {IntervalHours}h, Lookback: {LookbackHours}h", 
            _config.IntervalHours, _config.LookbackHours);

        // Run immediately on startup if configured
        if (_config.RunOnStartup)
        {
            _logger.LogInformation("Running initial sync on startup");
            await PerformSyncAsync(stoppingToken);
        }

        // Calculate initial delay to next scheduled run
        var intervalMs = TimeSpan.FromHours(_config.IntervalHours).TotalMilliseconds;
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait for the specified interval
                await Task.Delay(TimeSpan.FromHours(_config.IntervalHours), stoppingToken);
                
                // Perform the sync
                await PerformSyncAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in background service main loop");
                
                // Wait a shorter time before retrying to avoid tight loop
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("WhatsApp Integration Background Service stopped");
    }

    private async Task PerformSyncAsync(CancellationToken stoppingToken)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Starting scheduled sync at {StartTime}", startTime);
            
            var processedCount = await _apiSyncService.SyncCategorizedOrdersAsync(_config.LookbackHours, stoppingToken);
            
            var duration = DateTime.UtcNow - startTime;
            
            if (processedCount >= 0)
            {
                _logger.LogInformation("Sync completed successfully in {Duration}. Processed {ProcessedCount} customers", 
                    duration.ToString(@"mm\:ss"), processedCount);
            }
            else
            {
                _logger.LogWarning("Sync failed after {Duration}. Will retry on next scheduled run", 
                    duration.ToString(@"mm\:ss"));
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Sync operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Sync operation failed after {Duration}: {Message}", 
                duration.ToString(@"mm\:ss"), ex.Message);
        }
    }
}
