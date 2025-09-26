# WhatsApp Integration Background Service

A .NET 9 background service that automatically synchronizes categorized orders from Shopify using the WhatsApp Integration API.

## Features

- **Configurable Schedule**: Runs every X hours based on configuration
- **Smart Lookback**: Syncs data for the last X hours to ensure no data is missed
- **Startup Sync**: Optionally run sync immediately on startup
- **HTTP Client Integration**: Calls the PUT API endpoint with proper error handling
- **Comprehensive Logging**: Detailed logging for monitoring and debugging
- **Graceful Error Handling**: Continues running even if individual sync operations fail

## Configuration

Configure the service using `appsettings.json`:

```json
{
  "SyncService": {
    "IntervalHours": 24,           // How often to run the sync
    "LookbackHours": 48,           // How many hours back to look for data
    "ApiBaseUrl": "https://localhost:7021",
    "ApiEndpoint": "/api/shopify/orders/by-customer/categorized",
    "MaxOrdersLimit": 1000,        // Maximum orders per sync
    "MinOrdersPerCustomer": 1,     // Minimum orders per customer
    "OrderStatus": "any",          // Order status filter
    "RunOnStartup": true,          // Run sync on service startup
    "HttpTimeoutSeconds": 300      // HTTP request timeout
  }
}
```

### Development Settings

For development/testing, use `appsettings.Development.json` with shorter intervals:

```json
{
  "SyncService": {
    "IntervalHours": 1,            // Test every hour
    "LookbackHours": 24,           
    "ApiBaseUrl": "http://localhost:5015",  // HTTP for local dev
    "HttpTimeoutSeconds": 120      
  }
}
```

## Running the Service

### Development
```bash
cd WhatsAppIntegration.BackgroundService
dotnet run
```

### Production
```bash
cd WhatsAppIntegration.BackgroundService
dotnet run --environment Production
```

### As Windows Service
```bash
# Install as Windows service
sc create "WhatsAppIntegrationSync" binpath="C:\path\to\WhatsAppIntegration.BackgroundService.exe"

# Start the service
sc start "WhatsAppIntegrationSync"
```

### As Linux Service (systemd)
Create `/etc/systemd/system/whatsapp-integration-sync.service`:

```ini
[Unit]
Description=WhatsApp Integration Background Sync Service
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /opt/whatsapp-integration/WhatsAppIntegration.BackgroundService.dll
Restart=always
RestartSec=5
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
WorkingDirectory=/opt/whatsapp-integration

[Install]
WantedBy=multi-user.target
```

Then:
```bash
sudo systemctl enable whatsapp-integration-sync
sudo systemctl start whatsapp-integration-sync
```

## How It Works

1. **Startup**: Service starts and optionally runs initial sync
2. **Scheduled Sync**: Every `IntervalHours`, the service:
   - Calculates date range (last `LookbackHours` hours)
   - Makes PUT request to `/api/shopify/orders/by-customer/categorized`
   - Logs results (success/failure, customer count)
3. **Error Recovery**: If sync fails, service continues and retries on next schedule
4. **Graceful Shutdown**: Service responds to shutdown signals properly

## API Integration

The service calls the PUT endpoint with these query parameters:
- `createdAtMin`: Start of lookback period
- `createdAtMax`: Current time
- `limit`: Max orders limit from config
- `minOrdersPerCustomer`: Min orders filter from config
- `status`: Order status from config

Example API call:
```
PUT /api/shopify/orders/by-customer/categorized?
    createdAtMin=2025-01-25T10:00:00.000Z&
    createdAtMax=2025-01-26T10:00:00.000Z&
    limit=1000&
    minOrdersPerCustomer=1&
    status=any
```

## Logging

The service provides detailed logging:

```
[12:00:00 INF] WhatsApp Integration Background Service started. Interval: 24h, Lookback: 48h
[12:00:01 INF] Running initial sync on startup
[12:00:01 INF] Starting scheduled sync at 01/26/2025 12:00:01
[12:00:05 INF] Successfully synchronized 25 customers (IDs: 123, 456, 789, ...)
[12:00:05 INF] Sync completed successfully in 00:04. Processed 25 customers
```

## Monitoring

Monitor the service health by:
1. **Log Files**: Check for sync completion messages
2. **API Logs**: Monitor the main API for PUT request activity
3. **Database**: Verify `CategorizedOrders` collection is being updated
4. **Service Status**: Check if the background service is running

## Troubleshooting

### Common Issues

**Service won't start:**
- Check configuration is valid JSON
- Verify API base URL is reachable
- Check .NET 9 runtime is installed

**Sync failing:**
- Verify API is running and accessible
- Check network connectivity
- Review API logs for errors
- Increase `HttpTimeoutSeconds` if needed

**High memory usage:**
- Reduce `MaxOrdersLimit`
- Increase `IntervalHours` to run less frequently
- Monitor for memory leaks in API

### Log Levels

Set log levels in `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "WhatsAppIntegration.BackgroundService": "Debug"  // More verbose
    }
  }
}
```