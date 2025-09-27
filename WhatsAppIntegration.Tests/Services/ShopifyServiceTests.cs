using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WhatsAppIntegration.Models;
using WhatsAppIntegration.Repositories;
using WhatsAppIntegration.Services;

namespace WhatsAppIntegration.Tests.Services;

public class ShopifyServiceTests
{
    private readonly Mock<ILogger<ShopifyService>> _loggerMock;
    private readonly Mock<IOptions<ShopifyConfig>> _configMock;
    private readonly Mock<ICategorizedOrdersRepository> _repositoryMock;
    private readonly Mock<IShopifyServerAccess> _shopifyServerAccessMock;
    private readonly ShopifyConfig _config;
    private readonly ShopifyService _service;

    public ShopifyServiceTests()
    {
        _loggerMock = new Mock<ILogger<ShopifyService>>();
        _configMock = new Mock<IOptions<ShopifyConfig>>();
        _repositoryMock = new Mock<ICategorizedOrdersRepository>();
        _shopifyServerAccessMock = new Mock<IShopifyServerAccess>();
        
        _config = new ShopifyConfig
        {
            ShopDomain = "test-shop",
            AccessToken = "test_access_token",
            ApiVersion = "2023-10"
        };
        
        _configMock.Setup(x => x.Value).Returns(_config);
        _service = new ShopifyService(_loggerMock.Object, _repositoryMock.Object, _shopifyServerAccessMock.Object);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldDelegateToShopifyServerAccess()
    {
        // Arrange
        var customerId = 123L;
        var expectedCustomer = new ShopifyCustomer { Id = customerId, Email = "test@example.com" };
        _shopifyServerAccessMock
            .Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _service.GetCustomerAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedCustomer);
        _shopifyServerAccessMock.Verify(x => x.GetCustomerAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task GetCustomersCountAsync_ShouldDelegateToShopifyServerAccess()
    {
        // Arrange
        var expectedCount = 5000;
        _shopifyServerAccessMock
            .Setup(x => x.GetCustomersCountAsync())
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCustomersCountAsync();

        // Assert
        result.Should().Be(expectedCount);
        _shopifyServerAccessMock.Verify(x => x.GetCustomersCountAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_ShouldDelegateToShopifyServerAccess()
    {
        // Arrange
        var customerId = 123L;
        var status = "fulfilled";
        var limit = 100;
        var sinceId = 456L;
        var expectedOrders = new List<ShopifyOrder>
        {
            new() { Id = 789, TotalPrice = "25.99" }
        };

        _shopifyServerAccessMock
            .Setup(x => x.GetCustomerOrdersAsync(customerId, status, limit, sinceId))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _service.GetCustomerOrdersAsync(customerId, status, limit, sinceId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedOrders);
        _shopifyServerAccessMock.Verify(x => x.GetCustomerOrdersAsync(customerId, status, limit, sinceId), Times.Once);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldDelegateToShopifyServerAccess()
    {
        // Arrange
        var status = "any";
        var limit = 250;
        var sinceId = 123L;
        var createdAtMin = DateTime.UtcNow.AddDays(-30);
        var createdAtMax = DateTime.UtcNow;
        var customerIds = new List<long> { 456, 789 };
        var expectedOrders = new List<ShopifyOrder>
        {
            new() { Id = 100, TotalPrice = "35.99" },
            new() { Id = 101, TotalPrice = "45.99" }
        };

        _shopifyServerAccessMock
            .Setup(x => x.GetAllOrdersAsync(status, limit, sinceId, createdAtMin, createdAtMax, customerIds))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _service.GetAllOrdersAsync(status, limit, sinceId, createdAtMin, createdAtMax, customerIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedOrders);
        _shopifyServerAccessMock.Verify(x => x.GetAllOrdersAsync(status, limit, sinceId, createdAtMin, createdAtMax, customerIds), Times.Once);
    }

    [Fact]
    public async Task GetOrdersFromLastDaysAsync_ShouldDelegateToShopifyServerAccess()
    {
        // Arrange
        var days = 7;
        var status = "paid";
        var limit = 100;
        var expectedOrders = new List<ShopifyOrder>
        {
            new() { Id = 200, TotalPrice = "55.99" }
        };

        _shopifyServerAccessMock
            .Setup(x => x.GetOrdersFromLastDaysAsync(days, status, limit))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _service.GetOrdersFromLastDaysAsync(days, status, limit);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedOrders);
        _shopifyServerAccessMock.Verify(x => x.GetOrdersFromLastDaysAsync(days, status, limit), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldDelegateToShopifyServerAccess()
    {
        // Arrange
        var limit = 250;
        var sinceId = 123L;
        var vendor = "Test Vendor";
        var productType = "Test Type";
        var expectedProducts = new List<ShopifyProduct>
        {
            new() { Id = 300, Title = "Test Product", Vendor = vendor }
        };

        _shopifyServerAccessMock
            .Setup(x => x.GetAllProductsAsync(limit, sinceId, vendor, productType))
            .ReturnsAsync(expectedProducts);

        // Act
        var result = await _service.GetAllProductsAsync(limit, sinceId, vendor, productType);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedProducts);
        _shopifyServerAccessMock.Verify(x => x.GetAllProductsAsync(limit, sinceId, vendor, productType), Times.Once);
    }

    [Fact]
    public async Task GetProductAsync_ShouldDelegateToShopifyServerAccess()
    {
        // Arrange
        var productId = 456L;
        var expectedProduct = new ShopifyProduct { Id = productId, Title = "Test Product" };
        
        _shopifyServerAccessMock
            .Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await _service.GetProductAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedProduct);
        _shopifyServerAccessMock.Verify(x => x.GetProductAsync(productId), Times.Once);
    }

}