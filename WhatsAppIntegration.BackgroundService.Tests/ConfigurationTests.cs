using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WhatsAppIntegration.BackgroundService.Configuration;

namespace WhatsAppIntegration.BackgroundService.Tests;

public class ConfigurationTests
{
    [Fact]
    public void SyncServiceConfig_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var config = new SyncServiceConfig();

        // Assert
        Assert.Equal(24, config.IntervalHours);
        Assert.Equal(48, config.LookbackHours);
        Assert.Equal("https://localhost:7021", config.ApiBaseUrl);
        Assert.Equal("/api/shopify/orders/by-customer/categorized", config.ApiEndpoint);
        Assert.Equal(1000, config.MaxOrdersLimit);
        Assert.Equal(1, config.MinOrdersPerCustomer);
        Assert.Equal("any", config.OrderStatus);
        Assert.True(config.RunOnStartup);
        Assert.Equal(300, config.HttpTimeoutSeconds);
    }

    [Fact]
    public void SyncServiceConfig_ShouldBindFromConfiguration()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            {"SyncService:IntervalHours", "12"},
            {"SyncService:LookbackHours", "24"},
            {"SyncService:ApiBaseUrl", "http://test.com"},
            {"SyncService:MaxOrdersLimit", "500"},
            {"SyncService:RunOnStartup", "false"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var config = new SyncServiceConfig();
        configuration.GetSection(SyncServiceConfig.SectionName).Bind(config);

        // Assert
        Assert.Equal(12, config.IntervalHours);
        Assert.Equal(24, config.LookbackHours);
        Assert.Equal("http://test.com", config.ApiBaseUrl);
        Assert.Equal(500, config.MaxOrdersLimit);
        Assert.False(config.RunOnStartup);
    }

    [Fact]
    public void SyncServiceConfig_SectionName_ShouldBeCorrect()
    {
        // Assert
        Assert.Equal("SyncService", SyncServiceConfig.SectionName);
    }
}