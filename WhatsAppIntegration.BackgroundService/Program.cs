using WhatsAppIntegration.BackgroundService;
using WhatsAppIntegration.BackgroundService.Configuration;
using WhatsAppIntegration.BackgroundService.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure settings
builder.Services.Configure<SyncServiceConfig>(
    builder.Configuration.GetSection(SyncServiceConfig.SectionName));

// Add HTTP client
builder.Services.AddHttpClient<IApiSyncService, ApiSyncService>();

// Add services
builder.Services.AddScoped<IApiSyncService, ApiSyncService>();

// Add the background service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
