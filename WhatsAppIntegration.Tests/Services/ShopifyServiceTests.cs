using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using WhatsAppIntegration.Models;
using WhatsAppIntegration.Services;

namespace WhatsAppIntegration.Tests.Services;

public class ShopifyServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<ILogger<ShopifyService>> _loggerMock;
    private readonly Mock<IOptions<ShopifyConfig>> _configMock;
    private readonly ShopifyConfig _config;
    private readonly HttpClient _httpClient;
    private readonly ShopifyService _service;

    public ShopifyServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _loggerMock = new Mock<ILogger<ShopifyService>>();
        _configMock = new Mock<IOptions<ShopifyConfig>>();
        
        _config = new ShopifyConfig
        {
            ShopDomain = "test-shop",
            AccessToken = "test_access_token",
            ApiVersion = "2023-10"
        };
        
        _configMock.Setup(x => x.Value).Returns(_config);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _service = new ShopifyService(_httpClient, _configMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCustomersWithTagsAsync_WithValidTags_ShouldReturnFilteredCustomers()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "vip,premium" },
            new() { Id = 2, Email = "customer2@example.com", Tags = "regular" },
            new() { Id = 3, Email = "customer3@example.com", Tags = "vip" }
        };

        var responseContent = JsonSerializer.Serialize(new ShopifyCustomersResponse { Customers = customers });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetCustomersWithTagsAsync("vip", 250);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Id == 1);
        result.Should().Contain(c => c.Id == 3);
        result.Should().NotContain(c => c.Id == 2);
    }

    [Fact]
    public async Task GetAllCustomersAsync_WithValidLimit_ShouldReturnCustomers()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com" },
            new() { Id = 2, Email = "customer2@example.com" }
        };

        var responseContent = JsonSerializer.Serialize(new ShopifyCustomersResponse { Customers = customers });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetAllCustomersAsync(50);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Id == 1);
        result.Should().Contain(c => c.Id == 2);
    }

    [Fact]
    public async Task GetCustomerAsync_WithValidId_ShouldReturnCustomer()
    {
        // Arrange
        var customer = new ShopifyCustomer { Id = 123, Email = "test@example.com", FirstName = "John" };
        var responseContent = JsonSerializer.Serialize(new { customer });

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/customers/123")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetCustomerAsync(123);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(123);
        result.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetCustomerAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetCustomerAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_WithValidCustomerId_ShouldReturnOrders()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, TotalPrice = "100.00", CreatedAt = DateTime.Now.AddDays(-10) },
            new() { Id = 2, TotalPrice = "150.00", CreatedAt = DateTime.Now.AddDays(-5) }
        };

        var responseContent = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = orders });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetCustomerOrdersAsync(123);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == 1);
        result.Should().Contain(o => o.Id == 2);
    }

    [Fact]
    public async Task GetAllOrdersAsync_WithValidParameters_ShouldReturnOrders()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, FinancialStatus = "paid" },
            new() { Id = 2, FinancialStatus = "pending" }
        };

        var responseContent = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = orders });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetAllOrdersAsync("any", 50);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == 1);
        result.Should().Contain(o => o.Id == 2);
    }

    [Fact]
    public async Task GetAllProductsAsync_WithValidParameters_ShouldReturnProducts()
    {
        // Arrange
        var products = new List<ShopifyProduct>
        {
            new() { Id = 1, Title = "Product 1", Vendor = "Vendor A" },
            new() { Id = 2, Title = "Product 2", Vendor = "Vendor B" }
        };

        var responseContent = JsonSerializer.Serialize(new ShopifyProductsResponse { Products = products });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetAllProductsAsync(50);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == 1);
        result.Should().Contain(p => p.Id == 2);
    }

    [Fact]
    public async Task GetProductAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var product = new ShopifyProduct { Id = 456, Title = "Test Product", Vendor = "Test Vendor" };
        var responseContent = JsonSerializer.Serialize(new { product });

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/products/456")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetProductAsync(456);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(456);
        result.Title.Should().Be("Test Product");
        result.Vendor.Should().Be("Test Vendor");
    }

    [Fact]
    public async Task GetCustomerAnalyticsAsync_WithValidCustomer_ShouldReturnAnalytics()
    {
        // Arrange
        var customer = new ShopifyCustomer 
        { 
            Id = 123, 
            Email = "test@example.com", 
            FirstName = "John",
            Tags = "vip,premium"
        };
        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, TotalPrice = "100.00", CreatedAt = DateTime.Now.AddDays(-30) },
            new() { Id = 2, TotalPrice = "150.00", CreatedAt = DateTime.Now.AddDays(-15) },
            new() { Id = 3, TotalPrice = "200.00", CreatedAt = DateTime.Now.AddDays(-5) }
        };

        // Setup customer response
        var customerResponseContent = JsonSerializer.Serialize(new { customer });
        var customerHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(customerResponseContent, Encoding.UTF8, "application/json")
        };

        // Setup orders response
        var ordersResponseContent = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = orders });
        var ordersHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ordersResponseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(customerHttpResponse)
            .ReturnsAsync(ordersHttpResponse);

        // Act
        var result = await _service.GetCustomerAnalyticsAsync(123);

        // Assert
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be(123);
        result.Email.Should().Be("test@example.com");
        result.TotalOrders.Should().Be(3);
        result.TotalSpent.Should().Be(450m);
        result.Tags.Should().Contain("vip");
        result.Tags.Should().Contain("premium");
    }

    [Theory]
    [InlineData(2, true)] // Minimum orders for prediction
    [InlineData(1, false)] // Insufficient orders
    [InlineData(0, false)] // No orders
    public void CalculateNextPurchaseDate_WithVariousOrderCounts_ShouldReturnExpectedResult(int orderCount, bool shouldHavePrediction)
    {
        // Arrange
        var orders = new List<ShopifyOrder>();
        var baseDate = DateTime.Now.AddDays(-60);
        
        for (int i = 0; i < orderCount; i++)
        {
            orders.Add(new ShopifyOrder
            {
                Id = i + 1,
                CreatedAt = baseDate.AddDays(i * 30), // 30 days apart
                TotalPrice = "100.00"
            });
        }

        // Act
        var result = _service.CalculateNextPurchaseDate(orders);

        // Assert
        if (shouldHavePrediction)
        {
            result.Should().NotBeNull();
            result.Should().BeAfter(orders.Last().CreatedAt);
        }
        else
        {
            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetCustomersLikelyToPurchaseSoonAsync_WithValidParameters_ShouldReturnFilteredCustomers()
    {
        // Arrange - Setup customers response
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "regular" },
            new() { Id = 2, Email = "customer2@example.com", Tags = "vip" }
        };

        var customersResponseContent = JsonSerializer.Serialize(new ShopifyCustomersResponse { Customers = customers });
        var customersHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(customersResponseContent, Encoding.UTF8, "application/json")
        };

        // Setup orders responses for each customer
        var customer1Orders = new List<ShopifyOrder>
        {
            new() { Id = 1, CreatedAt = DateTime.Now.AddDays(-30), TotalPrice = "100.00" },
            new() { Id = 2, CreatedAt = DateTime.Now.AddDays(-10), TotalPrice = "150.00" } // Last order 10 days ago, should predict soon
        };

        var customer2Orders = new List<ShopifyOrder>
        {
            new() { Id = 3, CreatedAt = DateTime.Now.AddDays(-90), TotalPrice = "200.00" },
            new() { Id = 4, CreatedAt = DateTime.Now.AddDays(-60), TotalPrice = "250.00" } // Longer intervals, less likely soon
        };

        var customer1OrdersResponse = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = customer1Orders });
        var customer2OrdersResponse = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = customer2Orders });

        var customer1OrdersHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(customer1OrdersResponse, Encoding.UTF8, "application/json")
        };

        var customer2OrdersHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(customer2OrdersResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(customersHttpResponse) // Get customers with tags
            .ReturnsAsync(customer1OrdersHttpResponse) // Get customer 1 orders
            .ReturnsAsync(customer2OrdersHttpResponse); // Get customer 2 orders

        // Act
        var result = await _service.GetCustomersLikelyToPurchaseSoonAsync("regular", 30, 10);

        // Assert
        result.Should().NotBeEmpty();
        // Should contain customers with predicted purchase dates within threshold
        result.Should().OnlyContain(c => c.PredictedNextPurchaseDate.HasValue);
        result.Should().OnlyContain(c => c.TotalOrders > 1);
    }

    [Fact]
    public async Task GetCustomerAnalyticsByTagsAsync_WithValidTags_ShouldReturnAnalytics()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "vip" }
        };

        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, TotalPrice = "100.00", CreatedAt = DateTime.Now.AddDays(-20) },
            new() { Id = 2, TotalPrice = "150.00", CreatedAt = DateTime.Now.AddDays(-10) }
        };

        var customersResponseContent = JsonSerializer.Serialize(new ShopifyCustomersResponse { Customers = customers });
        var ordersResponseContent = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = orders });

        var customersHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(customersResponseContent, Encoding.UTF8, "application/json")
        };

        var ordersHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ordersResponseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(customersHttpResponse)
            .ReturnsAsync(ordersHttpResponse);

        // Act
        var result = await _service.GetCustomerAnalyticsByTagsAsync("vip", 10);

        // Assert
        result.Should().HaveCount(1);
        result[0].CustomerId.Should().Be(1);
        result[0].TotalOrders.Should().Be(2);
        result[0].TotalSpent.Should().Be(250m);
        result[0].Tags.Should().Contain("vip");
    }

    [Fact]
    public async Task Service_WithHttpError_ShouldHandleGracefully()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var customers = await _service.GetAllCustomersAsync();
        var orders = await _service.GetAllOrdersAsync();
        var products = await _service.GetAllProductsAsync();
        var customer = await _service.GetCustomerAsync(123);
        var product = await _service.GetProductAsync(456);

        // Assert
        customers.Should().BeEmpty();
        orders.Should().BeEmpty();
        products.Should().BeEmpty();
        customer.Should().BeNull();
        product.Should().BeNull();
    }

    [Fact]
    public async Task Service_WithNetworkException_ShouldHandleGracefully()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.GetCustomerAnalyticsAsync(123);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldSetAuthorizationHeader()
    {
        // Arrange & Act
        var httpClient = new HttpClient();
        var service = new ShopifyService(httpClient, _configMock.Object, _loggerMock.Object);

        // Assert
        httpClient.DefaultRequestHeaders.Should().Contain(h => h.Key == "X-Shopify-Access-Token");
        httpClient.DefaultRequestHeaders.GetValues("X-Shopify-Access-Token").First().Should().Be(_config.AccessToken);
    }

    [Fact]
    public async Task GetCustomersCountAsync_WithValidResponse_ShouldReturnCount()
    {
        // Arrange
        var countResponse = new { count = 5000 };
        var jsonResponse = JsonSerializer.Serialize(countResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var count = await _service.GetCustomersCountAsync();

        // Assert
        count.Should().Be(5000);
    }

    [Fact]
    public async Task GetCustomersCountAsync_WithServerError_ShouldReturnZero()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var count = await _service.GetCustomersCountAsync();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetCustomersWithTagsAsync_WithExcludeTags_ShouldFilterOutExcludedCustomers()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "vip,premium" },
            new() { Id = 2, Email = "customer2@example.com", Tags = "vip,regular" },
            new() { Id = 3, Email = "customer3@example.com", Tags = "vip" },
            new() { Id = 4, Email = "customer4@example.com", Tags = "regular" }
        };

        var customersResponse = new ShopifyCustomersResponse { Customers = customers };
        var jsonResponse = JsonSerializer.Serialize(customersResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetCustomersWithTagsAsync("vip", 250, "premium");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Id == 2); // Has vip but no premium
        result.Should().Contain(c => c.Id == 3); // Has vip but no premium
        result.Should().NotContain(c => c.Id == 1); // Has both vip and premium (excluded)
        result.Should().NotContain(c => c.Id == 4); // Doesn't have vip
    }

    [Fact]
    public async Task GetCustomersWithTagsAsync_WithMultipleExcludeTags_ShouldFilterCorrectly()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "vip,premium,gold" },
            new() { Id = 2, Email = "customer2@example.com", Tags = "vip,regular" },
            new() { Id = 3, Email = "customer3@example.com", Tags = "vip,silver" },
            new() { Id = 4, Email = "customer4@example.com", Tags = "vip" }
        };

        var customersResponse = new ShopifyCustomersResponse { Customers = customers };
        var jsonResponse = JsonSerializer.Serialize(customersResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetCustomersWithTagsAsync("vip", 250, "premium,silver");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Id == 2); // Has vip, regular (not excluded)
        result.Should().Contain(c => c.Id == 4); // Has only vip
        result.Should().NotContain(c => c.Id == 1); // Has premium (excluded)
        result.Should().NotContain(c => c.Id == 3); // Has silver (excluded)
    }

    [Fact]
    public async Task GetCustomersWithTagsAsync_WithEmptyExcludeTags_ShouldNotFilter()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "vip,premium" },
            new() { Id = 2, Email = "customer2@example.com", Tags = "vip" }
        };

        var customersResponse = new ShopifyCustomersResponse { Customers = customers };
        var jsonResponse = JsonSerializer.Serialize(customersResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetCustomersWithTagsAsync("vip", 250, "");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Id == 1);
        result.Should().Contain(c => c.Id == 2);
    }

    [Fact]
    public async Task GetAllOrdersAsync_WithNullLimit_ShouldReturnAllOrders()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, FinancialStatus = "paid", Customer = new ShopifyCustomer { Id = 123 } },
            new() { Id = 2, FinancialStatus = "pending", Customer = new ShopifyCustomer { Id = 456 } }
        };

        var responseContent = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = orders });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetAllOrdersAsync("any", null);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == 1);
        result.Should().Contain(o => o.Id == 2);
    }

    [Fact]
    public async Task GetOrdersByCustomerAsync_WithBasicParameters_ShouldReturnGroupedOrders()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, Customer = new ShopifyCustomer { Id = 123 }, FinancialStatus = "paid" },
            new() { Id = 2, Customer = new ShopifyCustomer { Id = 123 }, FinancialStatus = "pending" },
            new() { Id = 3, Customer = new ShopifyCustomer { Id = 456 }, FinancialStatus = "paid" }
        };

        var responseContent = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = orders });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetOrdersByCustomerAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainKey(123);
        result.Should().ContainKey(456);
        result[123].Should().HaveCount(2);
        result[456].Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOrdersByCustomerAsync_WithMinOrdersFilter_ShouldFilterCustomers()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, Customer = new ShopifyCustomer { Id = 123 }, FinancialStatus = "paid" },
            new() { Id = 2, Customer = new ShopifyCustomer { Id = 123 }, FinancialStatus = "pending" },
            new() { Id = 3, Customer = new ShopifyCustomer { Id = 456 }, FinancialStatus = "paid" }
        };

        var responseContent = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = orders });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetOrdersByCustomerAsync(minOrdersPerCustomer: 2);

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(123);
        result.Should().NotContainKey(456);
        result[123].Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrdersByCustomerAsync_WithProductIdsFilter_ShouldFilterByProducts()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() 
            { 
                Id = 1, 
                Customer = new ShopifyCustomer { Id = 123 }, 
                LineItems = new List<ShopifyLineItem> 
                { 
                    new() { ProductId = 100, Quantity = 1 } 
                } 
            },
            new() 
            { 
                Id = 2, 
                Customer = new ShopifyCustomer { Id = 123 }, 
                LineItems = new List<ShopifyLineItem> 
                { 
                    new() { ProductId = 200, Quantity = 1 } 
                } 
            },
            new() 
            { 
                Id = 3, 
                Customer = new ShopifyCustomer { Id = 456 }, 
                LineItems = new List<ShopifyLineItem> 
                { 
                    new() { ProductId = 300, Quantity = 1 } 
                } 
            }
        };

        var responseContent = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = orders });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetOrdersByCustomerAsync(productIds: new List<long> { 100, 200 });

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(123);
        result.Should().NotContainKey(456);
        result[123].Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrdersByCustomerAsync_WithMultipleProductIds_ShouldReturnOrdersMatchingAnyProductId()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() 
            { 
                Id = 1, 
                Customer = new ShopifyCustomer { Id = 123 }, 
                LineItems = new List<ShopifyLineItem> 
                { 
                    new() { ProductId = 100, Quantity = 1 } 
                } 
            }
        };

        var responseContent = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = orders });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetOrdersByCustomerAsync(productIds: new List<long> { 100, 200 });

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(123);
        result[123].Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOrdersByCustomerAsync_WithNoMatchingProductIds_ShouldReturnEmpty()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() 
            { 
                Id = 1, 
                Customer = new ShopifyCustomer { Id = 123 }, 
                LineItems = new List<ShopifyLineItem> 
                { 
                    new() { ProductId = 100, Quantity = 1 } 
                } 
            }
        };

        var responseContent = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = orders });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetOrdersByCustomerAsync(productIds: new List<long> { 999 });

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrdersByCustomerAsync_WithCombinedFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() 
            { 
                Id = 1, 
                Customer = new ShopifyCustomer { Id = 123 }, 
                LineItems = new List<ShopifyLineItem> 
                { 
                    new() { ProductId = 100, Quantity = 1 } 
                } 
            },
            new() 
            { 
                Id = 2, 
                Customer = new ShopifyCustomer { Id = 123 }, 
                LineItems = new List<ShopifyLineItem> 
                { 
                    new() { ProductId = 100, Quantity = 1 } 
                } 
            },
            new() 
            { 
                Id = 3, 
                Customer = new ShopifyCustomer { Id = 456 }, 
                LineItems = new List<ShopifyLineItem> 
                { 
                    new() { ProductId = 100, Quantity = 1 } 
                } 
            }
        };

        var responseContent = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = orders });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetOrdersByCustomerAsync(
            minOrdersPerCustomer: 2, 
            productIds: new List<long> { 100 });

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(123);
        result.Should().NotContainKey(456);
        result[123].Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCategorizedOrdersByCustomerAsync_WithBasicParameters_ShouldReturnCategorizedOrders()
    {
        // Arrange
        var products = new List<ShopifyProduct>
        {
            new() { Id = 100, Tags = "includeAutomation" },
            new() { Id = 200, Tags = "dogExtra1" }
        };

        var orders = new List<ShopifyOrder>
        {
            new() 
            { 
                Id = 1, 
                Customer = new ShopifyCustomer { Id = 123 }, 
                LineItems = new List<ShopifyLineItem> 
                { 
                    new() { ProductId = 100, Quantity = 1 } 
                } 
            },
            new() 
            { 
                Id = 2, 
                Customer = new ShopifyCustomer { Id = 123 }, 
                LineItems = new List<ShopifyLineItem> 
                { 
                    new() { ProductId = 200, Quantity = 1 } 
                } 
            }
        };

        var productResponseContent = JsonSerializer.Serialize(new ShopifyProductsResponse { Products = products });
        var orderResponseContent = JsonSerializer.Serialize(new ShopifyOrdersResponse { Orders = orders });

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(productResponseContent, Encoding.UTF8, "application/json")
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(orderResponseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _service.GetCategorizedOrdersByCustomerAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalCustomers.Should().Be(1);
        result.OrdersByCustomer.Should().ContainKey(123);
        result.OrdersByCustomer[123].AutomationProductsOrders.Should().HaveCount(1);
        result.OrdersByCustomer[123].DogExtraProductsOrders.Should().HaveCount(1);
        result.TotalAutomationOrders.Should().Be(1);
        result.TotalDogExtraOrders.Should().Be(1);
        
        // Verify customer information is present
        result.OrdersByCustomer[123].Customer.Should().NotBeNull();
        result.OrdersByCustomer[123].Customer.Id.Should().Be(123);
        
        // Verify orders don't have customer information (to avoid duplication)
        result.OrdersByCustomer[123].AutomationProductsOrders[0].Customer.Should().BeNull();
        result.OrdersByCustomer[123].DogExtraProductsOrders[0].Customer.Should().BeNull();
    }
}