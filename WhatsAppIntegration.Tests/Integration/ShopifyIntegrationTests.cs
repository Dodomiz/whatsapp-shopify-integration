using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WhatsAppIntegration.Controllers;
using WhatsAppIntegration.Models;
using WhatsAppIntegration.Services;

namespace WhatsAppIntegration.Tests.Integration;

public class ShopifyIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ShopifyIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace Shopify service with mock for integration tests
                var descriptors = services.Where(d => d.ServiceType == typeof(IShopifyService)).ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                var mockShopifyService = new Mock<IShopifyService>();
                
                // Setup mock responses for various endpoints
                SetupMockResponses(mockShopifyService);
                
                services.AddScoped<IShopifyService>(_ => mockShopifyService.Object);

                // Configure test Shopify settings
                services.Configure<ShopifyConfig>(config =>
                {
                    config.ShopDomain = "test-shop";
                    config.AccessToken = "test_access_token";
                    config.ApiVersion = "2023-10";
                });
            });
        });

        _client = _factory.CreateClient();
    }

    private static void SetupMockResponses(Mock<IShopifyService> mockShopifyService)
    {
        // Setup customers with tags
        var customersWithTags = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "vip1@example.com", FirstName = "John", LastName = "Doe", Tags = "vip,premium" },
            new() { Id = 2, Email = "vip2@example.com", FirstName = "Jane", LastName = "Smith", Tags = "vip" }
        };

        mockShopifyService.Setup(x => x.GetCustomersWithTagsAsync("vip", It.IsAny<int?>(), It.IsAny<string?>()))
            .ReturnsAsync(customersWithTags);

        // Setup customers with exclusion  
        var excludeFilteredCustomers = new List<ShopifyCustomer>
        {
            new() { Id = 2, Email = "vip2@example.com", FirstName = "Jane", LastName = "Smith", Tags = "vip" }
        };

        mockShopifyService.Setup(x => x.GetCustomersWithTagsAsync("vip", It.IsAny<int?>(), "premium"))
            .ReturnsAsync(excludeFilteredCustomers);

        // Setup all customers
        var allCustomers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com" },
            new() { Id = 2, Email = "customer2@example.com" },
            new() { Id = 3, Email = "customer3@example.com" }
        };

        mockShopifyService.Setup(x => x.GetAllCustomersAsync(It.IsAny<int?>()))
            .ReturnsAsync(allCustomers);

        // Setup for null/empty tags requests (return all customers)
        mockShopifyService.Setup(x => x.GetCustomersWithTagsAsync(null, It.IsAny<int?>(), It.IsAny<string?>()))
            .ReturnsAsync(allCustomers);
        
        mockShopifyService.Setup(x => x.GetCustomersWithTagsAsync("", It.IsAny<int?>(), It.IsAny<string?>()))
            .ReturnsAsync(allCustomers);

        // Setup specific customer
        var specificCustomer = new ShopifyCustomer 
        { 
            Id = 123, 
            Email = "specific@example.com", 
            FirstName = "Specific", 
            LastName = "Customer" 
        };

        mockShopifyService.Setup(x => x.GetCustomerAsync(123))
            .ReturnsAsync(specificCustomer);

        mockShopifyService.Setup(x => x.GetCustomerAsync(999))
            .ReturnsAsync((ShopifyCustomer?)null);

        // Setup customer count
        mockShopifyService.Setup(x => x.GetCustomersCountAsync())
            .ReturnsAsync(5000); // Mock a store with 5000 customers

        // Setup customer orders
        var customerOrders = new List<ShopifyOrder>
        {
            new() { Id = 1, TotalPrice = "100.00", CreatedAt = DateTime.Now.AddDays(-30) },
            new() { Id = 2, TotalPrice = "150.00", CreatedAt = DateTime.Now.AddDays(-15) },
            new() { Id = 3, TotalPrice = "200.00", CreatedAt = DateTime.Now.AddDays(-5) }
        };

        mockShopifyService.Setup(x => x.GetCustomerOrdersAsync(123, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long?>()))
            .ReturnsAsync(customerOrders);

        // Setup all orders
        var allOrders = new List<ShopifyOrder>
        {
            new() { Id = 1, FinancialStatus = "paid", TotalPrice = "100.00" },
            new() { Id = 2, FinancialStatus = "pending", TotalPrice = "200.00" }
        };

        mockShopifyService.Setup(x => x.GetAllOrdersAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(allOrders);

        // Setup products
        var allProducts = new List<ShopifyProduct>
        {
            new() { Id = 1, Title = "Product 1", Vendor = "Vendor A", ProductType = "Type A" },
            new() { Id = 2, Title = "Product 2", Vendor = "Vendor B", ProductType = "Type B" }
        };

        mockShopifyService.Setup(x => x.GetAllProductsAsync(It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(allProducts);

        var specificProduct = new ShopifyProduct 
        { 
            Id = 456, 
            Title = "Specific Product", 
            Vendor = "Specific Vendor" 
        };

        mockShopifyService.Setup(x => x.GetProductAsync(456))
            .ReturnsAsync(specificProduct);

        mockShopifyService.Setup(x => x.GetProductAsync(999))
            .ReturnsAsync((ShopifyProduct?)null);

        // Setup customer analytics
        var customerAnalytics = new CustomerAnalytics
        {
            CustomerId = 123,
            Email = "specific@example.com",
            FirstName = "Specific",
            LastName = "Customer",
            TotalOrders = 3,
            TotalSpent = 450m,
            AverageOrderValue = 150m,
            DaysSinceLastOrder = 5,
            PredictedNextPurchaseDate = DateTime.Now.AddDays(20),
            PurchaseFrequency = "Regular",
            Tags = new List<string> { "vip", "premium" }
        };

        mockShopifyService.Setup(x => x.GetCustomerAnalyticsAsync(123))
            .ReturnsAsync(customerAnalytics);

        mockShopifyService.Setup(x => x.GetCustomerAnalyticsAsync(999))
            .ReturnsAsync((CustomerAnalytics?)null);

        // Setup analytics by tags
        var analyticsByTags = new List<CustomerAnalytics>
        {
            customerAnalytics,
            new CustomerAnalytics
            {
                CustomerId = 2,
                Email = "vip2@example.com",
                TotalOrders = 2,
                TotalSpent = 300m,
                Tags = new List<string> { "vip" }
            }
        };

        mockShopifyService.Setup(x => x.GetCustomerAnalyticsByTagsAsync("vip", It.IsAny<int>()))
            .ReturnsAsync(analyticsByTags);

        // Setup next purchase prediction
        var nextPurchaseDate = DateTime.Now.AddDays(25);
        mockShopifyService.Setup(x => x.CalculateNextPurchaseDate(It.IsAny<List<ShopifyOrder>>()))
            .Returns(nextPurchaseDate);

        // Setup customers likely to purchase soon
        var customersLikelyToPurchase = new List<CustomerAnalytics>
        {
            new CustomerAnalytics
            {
                CustomerId = 1,
                Email = "soon1@example.com",
                PredictedNextPurchaseDate = DateTime.Now.AddDays(3),
                TotalOrders = 5
            },
            new CustomerAnalytics
            {
                CustomerId = 2,
                Email = "soon2@example.com",
                PredictedNextPurchaseDate = DateTime.Now.AddDays(6),
                TotalOrders = 3
            }
        };

        mockShopifyService.Setup(x => x.GetCustomersLikelyToPurchaseSoonAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(customersLikelyToPurchase);
    }

    #region Customer Endpoint Tests

    [Fact]
    public async Task GET_CustomersByTags_WithValidTags_ShouldReturnCustomers()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/by-tags?tags=vip&limit=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var customersResponse = JsonSerializer.Deserialize<ShopifyCustomersResponse>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        customersResponse.Should().NotBeNull();
        customersResponse!.Customers.Should().HaveCount(2);
        customersResponse.Customers.Should().Contain(c => c.Email == "vip1@example.com");
        customersResponse.Customers.Should().Contain(c => c.Email == "vip2@example.com");
    }

    [Fact]
    public async Task GET_CustomersByTags_WithoutTags_ShouldReturnAllCustomers()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/by-tags?limit=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ShopifyCustomersResponse>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        result.Should().NotBeNull();
        result!.Customers.Should().NotBeNull();
    }

    [Fact]
    public async Task GET_AllCustomers_ShouldReturnAllCustomers()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers?limit=100");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var customersResponse = JsonSerializer.Deserialize<ShopifyCustomersResponse>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        customersResponse.Should().NotBeNull();
        customersResponse!.Customers.Should().HaveCount(3);
    }

    [Fact]
    public async Task GET_AllCustomers_WithoutLimit_ShouldReturnAllCustomers()
    {
        // Act - Test unlimited pagination by not specifying a limit
        var response = await _client.GetAsync("/api/shopify/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var customersResponse = JsonSerializer.Deserialize<ShopifyCustomersResponse>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        customersResponse.Should().NotBeNull();
        customersResponse!.Customers.Should().HaveCount(3);
    }

    [Fact]  
    public async Task GET_AllCustomers_WithHighLimit_ShouldReturnAllCustomers()
    {
        // Act - Test that high limits are now allowed (no upper bound validation)
        var response = await _client.GetAsync("/api/shopify/customers?limit=1000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var customersResponse = JsonSerializer.Deserialize<ShopifyCustomersResponse>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        customersResponse.Should().NotBeNull();
        customersResponse!.Customers.Should().HaveCount(3);
    }

    [Fact]
    public async Task GET_SpecificCustomer_WithValidId_ShouldReturnCustomer()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var customer = JsonSerializer.Deserialize<ShopifyCustomer>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        customer.Should().NotBeNull();
        customer!.Id.Should().Be(123);
        customer.Email.Should().Be("specific@example.com");
    }

    [Fact]
    public async Task GET_SpecificCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_CustomersCount_ShouldReturnTotalCount()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var count = int.Parse(content);
        
        count.Should().Be(5000);
    }

    [Fact]
    public async Task GET_CustomersByTags_WithExcludeTags_ShouldReturnFilteredCustomers()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/by-tags?tags=vip&excludeTags=premium&limit=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ShopifyCustomersResponse>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        result.Should().NotBeNull();
        result!.Customers.Should().HaveCount(1);
        result.Customers[0].Id.Should().Be(2);
        result.Customers[0].Email.Should().Be("vip2@example.com");
        result.Customers[0].Tags.Should().NotContain("premium");
    }

    #endregion

    #region Order Endpoint Tests

    [Fact]
    public async Task GET_CustomerOrders_WithValidCustomerId_ShouldReturnOrders()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/123/orders?status=any&limit=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var ordersResponse = JsonSerializer.Deserialize<ShopifyOrdersResponse>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        ordersResponse.Should().NotBeNull();
        ordersResponse!.Orders.Should().HaveCount(3);
        ordersResponse.Orders.Should().Contain(o => o.TotalPrice == "100.00");
        ordersResponse.Orders.Should().Contain(o => o.TotalPrice == "150.00");
        ordersResponse.Orders.Should().Contain(o => o.TotalPrice == "200.00");
    }

    [Fact]
    public async Task GET_CustomerOrders_WithInvalidStatus_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/123/orders?status=invalid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_AllOrders_ShouldReturnOrders()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/orders?status=any&limit=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var ordersResponse = JsonSerializer.Deserialize<ShopifyOrdersResponse>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        ordersResponse.Should().NotBeNull();
        ordersResponse!.Orders.Should().HaveCount(2);
    }

    #endregion

    #region Product Endpoint Tests

    [Fact]
    public async Task GET_AllProducts_ShouldReturnProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/products?limit=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var productsResponse = JsonSerializer.Deserialize<ShopifyProductsResponse>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        productsResponse.Should().NotBeNull();
        productsResponse!.Products.Should().HaveCount(2);
        productsResponse.Products.Should().Contain(p => p.Title == "Product 1");
        productsResponse.Products.Should().Contain(p => p.Title == "Product 2");
    }

    [Fact]
    public async Task GET_SpecificProduct_WithValidId_ShouldReturnProduct()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/products/456");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ShopifyProduct>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        product.Should().NotBeNull();
        product!.Id.Should().Be(456);
        product.Title.Should().Be("Specific Product");
    }

    [Fact]
    public async Task GET_SpecificProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/products/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Analytics Endpoint Tests

    [Fact]
    public async Task GET_CustomerAnalytics_WithValidId_ShouldReturnAnalytics()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/123/analytics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var analytics = JsonSerializer.Deserialize<CustomerAnalytics>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        analytics.Should().NotBeNull();
        analytics!.CustomerId.Should().Be(123);
        analytics.TotalOrders.Should().Be(3);
        analytics.TotalSpent.Should().Be(450m);
        analytics.PurchaseFrequency.Should().Be("Regular");
    }

    [Fact]
    public async Task GET_CustomerAnalytics_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/999/analytics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_CustomerAnalyticsByTags_WithValidTags_ShouldReturnAnalytics()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/analytics/by-tags?tags=vip&limit=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var analyticsList = JsonSerializer.Deserialize<List<CustomerAnalytics>>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        analyticsList.Should().NotBeNull();
        analyticsList!.Should().HaveCount(2);
        analyticsList.Should().Contain(a => a.CustomerId == 123);
        analyticsList.Should().Contain(a => a.CustomerId == 2);
    }

    [Fact]
    public async Task GET_CustomerAnalyticsByTags_WithoutTags_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/analytics/by-tags?limit=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Purchase Prediction Endpoint Tests

    [Fact]
    public async Task GET_NextPurchasePrediction_WithValidCustomerId_ShouldReturnPrediction()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/123/next-purchase-prediction");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var prediction = JsonSerializer.Deserialize<NextPurchasePredictionResponse>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        prediction.Should().NotBeNull();
        prediction!.CustomerId.Should().Be(123);
        prediction.TotalOrders.Should().Be(3);
        prediction.PredictedNextPurchaseDate.Should().BeAfter(DateTime.Now);
        prediction.Confidence.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GET_CustomersLikelyToPurchaseSoon_ShouldReturnCustomers()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/likely-to-purchase-soon?daysThreshold=7&limit=25");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<List<CustomerAnalytics>>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        customers.Should().NotBeNull();
        customers!.Should().HaveCount(2);
        customers.Should().OnlyContain(c => c.PredictedNextPurchaseDate.HasValue);
        customers.Should().OnlyContain(c => c.TotalOrders > 0);
    }

    [Fact]
    public async Task GET_CustomersLikelyToPurchaseSoon_WithInvalidDaysThreshold_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/likely-to-purchase-soon?daysThreshold=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_CustomersLikelyToPurchaseSoon_WithInvalidLimit_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/shopify/customers/likely-to-purchase-soon?limit=51");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Parameter Validation Tests

    [Theory]
    [InlineData("/api/shopify/customers/by-tags?tags=vip&limit=0")]
    [InlineData("/api/shopify/customers/by-tags?tags=vip&limit=251")]
    [InlineData("/api/shopify/customers?limit=50")]
    [InlineData("/api/shopify/products?limit=50")]
    public async Task GET_WithAnyLimit_ShouldReturnOk(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}