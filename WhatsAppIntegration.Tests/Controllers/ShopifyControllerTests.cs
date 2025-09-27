using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WhatsAppIntegration.Controllers;
using WhatsAppIntegration.Models;
using WhatsAppIntegration.Repositories;
using WhatsAppIntegration.Services;

namespace WhatsAppIntegration.Tests.Controllers;

public class ShopifyControllerTests
{
    private readonly Mock<IShopifyService> _shopifyServiceMock;
    private readonly Mock<ICategorizedOrdersRepository> _repositoryMock;
    private readonly Mock<ILogger<ShopifyController>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ShopifyController _controller;

    public ShopifyControllerTests()
    {
        _shopifyServiceMock = new Mock<IShopifyService>();
        _repositoryMock = new Mock<ICategorizedOrdersRepository>();
        _loggerMock = new Mock<ILogger<ShopifyController>>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["OrderLookupHours"]).Returns("48");
        _controller = new ShopifyController(_shopifyServiceMock.Object, _repositoryMock.Object, _loggerMock.Object, _configurationMock.Object);
    }

    #region Customer Tests

    [Fact]
    public async Task GetCustomersByTags_WithValidTags_ShouldReturnOkWithCustomers()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "vip" },
            new() { Id = 2, Email = "customer2@example.com", Tags = "premium" }
        };

        _shopifyServiceMock.Setup(s => s.GetCustomersWithTagsAsync("vip", 50, null))
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.GetCustomersByTags("vip", 50);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyCustomersResponse>().Subject;
        response.Customers.Should().HaveCount(2);
        response.Customers.Should().Contain(c => c.Id == 1);
        response.Customers.Should().Contain(c => c.Id == 2);
    }

    [Fact]
    public async Task GetCustomersByTags_WithNullTags_ShouldReturnAllCustomers()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "vip" },
            new() { Id = 2, Email = "customer2@example.com", Tags = "regular" }
        };

        _shopifyServiceMock.Setup(s => s.GetCustomersWithTagsAsync(null, 50, null))
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.GetCustomersByTags(null, 50);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyCustomersResponse>().Subject;
        response.Customers.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCustomersByTags_WithEmptyTags_ShouldReturnAllCustomers()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "vip" }
        };

        _shopifyServiceMock.Setup(s => s.GetCustomersWithTagsAsync("", 50, null))
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.GetCustomersByTags("", 50);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyCustomersResponse>().Subject;
        response.Customers.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCustomersByTags_WithNullLimit_ShouldReturnAllCustomers()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "vip" }
        };

        _shopifyServiceMock.Setup(s => s.GetCustomersWithTagsAsync("vip", null, null))
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.GetCustomersByTags("vip", null);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyCustomersResponse>().Subject;
        response.Customers.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllCustomers_WithValidParameters_ShouldReturnOkWithCustomers()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com" }
        };

        _shopifyServiceMock.Setup(s => s.GetAllCustomersAsync(50))
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.GetAllCustomers(50);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyCustomersResponse>().Subject;
        response.Customers.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCustomer_WithValidId_ShouldReturnOkWithCustomer()
    {
        // Arrange
        var customer = new ShopifyCustomer { Id = 123, Email = "test@example.com" };
        _shopifyServiceMock.Setup(s => s.GetCustomerAsync(123))
            .ReturnsAsync(customer);

        // Act
        var result = await _controller.GetCustomer(123);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var returnedCustomer = okResult.Value.Should().BeOfType<ShopifyCustomer>().Subject;
        returnedCustomer.Id.Should().Be(123);
        returnedCustomer.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetCustomer_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        _shopifyServiceMock.Setup(s => s.GetCustomerAsync(999))
            .ReturnsAsync((ShopifyCustomer?)null);

        // Act
        var result = await _controller.GetCustomer(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.Should().Be("Customer with ID 999 not found");
    }

    [Fact]
    public async Task GetCustomersByTags_WithServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        _shopifyServiceMock.Setup(s => s.GetCustomersWithTagsAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<string?>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetCustomersByTags("vip", 50);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("Internal server error while retrieving customers");
    }

    [Fact]
    public async Task GetCustomersCount_ShouldReturnOkWithCount()
    {
        // Arrange
        _shopifyServiceMock.Setup(s => s.GetCustomersCountAsync())
            .ReturnsAsync(5000);

        // Act
        var result = await _controller.GetCustomersCount();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(5000);
    }

    [Fact]
    public async Task GetCustomersCount_WithServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        _shopifyServiceMock.Setup(s => s.GetCustomersCountAsync())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetCustomersCount();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("Internal server error while retrieving customers count");
    }

    [Fact]
    public async Task GetCustomersByTags_WithExcludeTags_ShouldReturnFilteredCustomers()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "vip" },
            new() { Id = 2, Email = "customer2@example.com", Tags = "vip,premium" }
        };

        _shopifyServiceMock.Setup(s => s.GetCustomersWithTagsAsync("vip", 50, "premium"))
            .ReturnsAsync(customers.Where(c => c.Tags.Contains("vip") && !c.Tags.Contains("premium")).ToList());

        // Act
        var result = await _controller.GetCustomersByTags("vip", 50, "premium");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyCustomersResponse>().Subject;
        response.Customers.Should().HaveCount(1);
        response.Customers[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task GetCustomersByTags_WithMultipleExcludeTags_ShouldReturnFilteredCustomers()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "vip" },
            new() { Id = 2, Email = "customer2@example.com", Tags = "vip,premium" },
            new() { Id = 3, Email = "customer3@example.com", Tags = "vip,gold" }
        };

        _shopifyServiceMock.Setup(s => s.GetCustomersWithTagsAsync("vip", 50, "premium,gold"))
            .ReturnsAsync(customers.Where(c => c.Tags.Contains("vip") && 
                                              !c.Tags.Contains("premium") && 
                                              !c.Tags.Contains("gold")).ToList());

        // Act
        var result = await _controller.GetCustomersByTags("vip", 50, "premium,gold");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyCustomersResponse>().Subject;
        response.Customers.Should().HaveCount(1);
        response.Customers[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task GetCustomersByTags_WithNullExcludeTags_ShouldReturnAllMatchingCustomers()
    {
        // Arrange
        var customers = new List<ShopifyCustomer>
        {
            new() { Id = 1, Email = "customer1@example.com", Tags = "vip" },
            new() { Id = 2, Email = "customer2@example.com", Tags = "vip,premium" }
        };

        _shopifyServiceMock.Setup(s => s.GetCustomersWithTagsAsync("vip", 50, null))
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.GetCustomersByTags("vip", 50, null);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyCustomersResponse>().Subject;
        response.Customers.Should().HaveCount(2);
    }

    #endregion

    #region Order Tests

    [Fact]
    public async Task GetCustomerOrders_WithValidParameters_ShouldReturnOkWithOrders()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, TotalPrice = "100.00" },
            new() { Id = 2, TotalPrice = "150.00" }
        };

        _shopifyServiceMock.Setup(s => s.GetCustomerOrdersAsync(123, "any", 50, null))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.GetCustomerOrders(123, "any", 50);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyOrdersResponse>().Subject;
        response.Orders.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("unknown")]
    public async Task GetCustomerOrders_WithInvalidStatus_ShouldReturnBadRequest(string status)
    {
        // Act
        var result = await _controller.GetCustomerOrders(123, status, 50);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().Be("Status must be one of: any, open, closed, cancelled");
    }

    [Fact]
    public async Task GetAllOrders_WithValidParameters_ShouldReturnOkWithOrders()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, FinancialStatus = "paid" }
        };

        _shopifyServiceMock.Setup(s => s.GetAllOrdersAsync("open", 50, null, null, null, null))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.GetAllOrders("open", 50);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyOrdersResponse>().Subject;
        response.Orders.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllOrders_WithNullLimit_ShouldReturnOkWithOrders()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, FinancialStatus = "paid" },
            new() { Id = 2, FinancialStatus = "pending" }
        };

        _shopifyServiceMock.Setup(s => s.GetAllOrdersAsync("any", null, null, null, null, null))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.GetAllOrders("any", null);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyOrdersResponse>().Subject;
        response.Orders.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrdersByCustomer_WithBasicParameters_ShouldReturnOkWithGroupedOrders()
    {
        // Arrange
        var ordersByCustomer = new Dictionary<long, List<ShopifyOrder>>
        {
            { 123, new List<ShopifyOrder> { new() { Id = 1, Customer = new ShopifyCustomer { Id = 123 } } } },
            { 456, new List<ShopifyOrder> { new() { Id = 2, Customer = new ShopifyCustomer { Id = 456 } } } }
        };

        _shopifyServiceMock.Setup(s => s.GetOrdersByCustomerAsync("any", null, null, (List<long>?)null, null, null))
            .ReturnsAsync(ordersByCustomer);

        // Act
        var result = await _controller.GetOrdersByCustomer();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyOrdersByCustomerResponse>().Subject;
        response.TotalCustomers.Should().Be(2);
        response.TotalOrders.Should().Be(2);
        response.OrdersByCustomer.Should().ContainKey(123);
        response.OrdersByCustomer.Should().ContainKey(456);
    }

    [Fact]
    public async Task GetOrdersByCustomer_WithMinOrdersFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var ordersByCustomer = new Dictionary<long, List<ShopifyOrder>>
        {
            { 123, new List<ShopifyOrder> 
                { 
                    new() { Id = 1, Customer = new ShopifyCustomer { Id = 123 } },
                    new() { Id = 2, Customer = new ShopifyCustomer { Id = 123 } }
                } 
            }
        };

        _shopifyServiceMock.Setup(s => s.GetOrdersByCustomerAsync("any", null, 2, (List<long>?)null, null, null))
            .ReturnsAsync(ordersByCustomer);

        // Act
        var result = await _controller.GetOrdersByCustomer(minOrdersPerCustomer: 2);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyOrdersByCustomerResponse>().Subject;
        response.TotalCustomers.Should().Be(1);
        response.TotalOrders.Should().Be(2);
        response.OrdersByCustomer[123].Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrdersByCustomer_WithProductIdsFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var ordersByCustomer = new Dictionary<long, List<ShopifyOrder>>
        {
            { 123, new List<ShopifyOrder> { new() { Id = 1, Customer = new ShopifyCustomer { Id = 123 } } } }
        };

        _shopifyServiceMock.Setup(s => s.GetOrdersByCustomerAsync("any", null, null, new List<long> { 100, 200 }, null, null))
            .ReturnsAsync(ordersByCustomer);

        // Act
        var result = await _controller.GetOrdersByCustomer(productIds: "100,200");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyOrdersByCustomerResponse>().Subject;
        response.TotalCustomers.Should().Be(1);
        response.OrdersByCustomer.Should().ContainKey(123);
    }

    [Fact]
    public async Task GetOrdersByCustomer_WithDateFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var ordersByCustomer = new Dictionary<long, List<ShopifyOrder>>
        {
            { 123, new List<ShopifyOrder> { new() { Id = 1, Customer = new ShopifyCustomer { Id = 123 } } } }
        };

        var startDate = DateTime.Now.AddDays(-30);
        var endDate = DateTime.Now;

        _shopifyServiceMock.Setup(s => s.GetOrdersByCustomerAsync("any", null, null, (List<long>?)null, startDate, endDate))
            .ReturnsAsync(ordersByCustomer);

        // Act
        var result = await _controller.GetOrdersByCustomer(createdAtMin: startDate, createdAtMax: endDate);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyOrdersByCustomerResponse>().Subject;
        response.TotalCustomers.Should().Be(1);
    }

    [Fact]
    public async Task GetOrdersByCustomer_WithAllFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var ordersByCustomer = new Dictionary<long, List<ShopifyOrder>>
        {
            { 123, new List<ShopifyOrder> 
                { 
                    new() { Id = 1, Customer = new ShopifyCustomer { Id = 123 } },
                    new() { Id = 2, Customer = new ShopifyCustomer { Id = 123 } }
                } 
            }
        };

        var startDate = DateTime.Now.AddDays(-30);
        var endDate = DateTime.Now;

        _shopifyServiceMock.Setup(s => s.GetOrdersByCustomerAsync("paid", 100, 2, new List<long> { 100 }, startDate, endDate))
            .ReturnsAsync(ordersByCustomer);

        // Act
        var result = await _controller.GetOrdersByCustomer("paid", 100, 2, "100", startDate, endDate);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyOrdersByCustomerResponse>().Subject;
        response.TotalCustomers.Should().Be(1);
        response.TotalOrders.Should().Be(2);
    }

    [Fact]
    public async Task GetOrdersByCustomer_WithServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        _shopifyServiceMock.Setup(s => s.GetOrdersByCustomerAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<List<long>?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetOrdersByCustomer();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("Internal server error while retrieving orders by customer");
    }

    [Fact]
    public async Task GetCategorizedOrdersByCustomer_WithBasicParameters_ShouldReturnOkWithCategorizedOrders()
    {
        // Arrange
        var response = new ShopifyCategorizedOrdersByCustomerResponse
        {
            OrdersByCustomer = new Dictionary<long, CustomerCategorizedOrders>
            {
                { 
                    123, 
                    new CustomerCategorizedOrders 
                    { 
                        Customer = new ShopifyCustomer { Id = 123, FirstName = "John", LastName = "Doe" },
                        AutomationProductsOrders = new List<ShopifyOrder> { new() { Id = 1, Customer = null } },
                        DogExtraProductsOrders = new List<ShopifyOrder> { new() { Id = 2, Customer = null } }
                    } 
                }
            }
        };

        _shopifyServiceMock.Setup(s => s.GetCategorizedOrdersByCustomerAsync("any", null, null, null, null, null))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ProcessCategorizedOrdersByCustomer();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var returnedResponse = okResult.Value.Should().BeOfType<CategorizedOrdersProcessedResponse>().Subject;
        returnedResponse.ProcessedCustomersCount.Should().Be(1);
        returnedResponse.ProcessedCustomerIds.Should().Contain(123);
    }

    [Fact]
    public async Task GetCategorizedOrdersByCustomer_WithInvalidStatus_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.ProcessCategorizedOrdersByCustomer(status: "invalid");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().Be("Invalid status. Must be one of: any, open, closed, cancelled");
    }

    [Fact]
    public async Task GetCategorizedOrdersByCustomer_WithServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        _shopifyServiceMock.Setup(s => s.GetCategorizedOrdersByCustomerAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<List<long>?>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.ProcessCategorizedOrdersByCustomer();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("Internal server error while processing categorized orders by customer");
    }

    #endregion

    #region Product Tests

    [Fact]
    public async Task GetAllProducts_WithValidParameters_ShouldReturnOkWithProducts()
    {
        // Arrange
        var products = new List<ShopifyProduct>
        {
            new() { Id = 1, Title = "Product 1" },
            new() { Id = 2, Title = "Product 2" }
        };

        _shopifyServiceMock.Setup(s => s.GetAllProductsAsync(50, null, null, null))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetAllProducts(50);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyProductsResponse>().Subject;
        response.Products.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProduct_WithValidId_ShouldReturnOkWithProduct()
    {
        // Arrange
        var product = new ShopifyProduct { Id = 456, Title = "Test Product" };
        _shopifyServiceMock.Setup(s => s.GetProductAsync(456))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetProduct(456);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var returnedProduct = okResult.Value.Should().BeOfType<ShopifyProduct>().Subject;
        returnedProduct.Id.Should().Be(456);
        returnedProduct.Title.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetProduct_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        _shopifyServiceMock.Setup(s => s.GetProductAsync(999))
            .ReturnsAsync((ShopifyProduct?)null);

        // Act
        var result = await _controller.GetProduct(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.Should().Be("Product with ID 999 not found");
    }

    #endregion

    #region Analytics Tests

    [Fact]
    public async Task GetCustomerAnalytics_WithValidId_ShouldReturnOkWithAnalytics()
    {
        // Arrange
        var analytics = new CustomerAnalytics
        {
            CustomerId = 123,
            Email = "test@example.com",
            TotalOrders = 5,
            TotalSpent = 500m,
            PredictedNextPurchaseDate = DateTime.Now.AddDays(30)
        };

        _shopifyServiceMock.Setup(s => s.GetCustomerAnalyticsAsync(123))
            .ReturnsAsync(analytics);

        // Act
        var result = await _controller.GetCustomerAnalytics(123);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var returnedAnalytics = okResult.Value.Should().BeOfType<CustomerAnalytics>().Subject;
        returnedAnalytics.CustomerId.Should().Be(123);
        returnedAnalytics.TotalOrders.Should().Be(5);
        returnedAnalytics.TotalSpent.Should().Be(500m);
    }

    [Fact]
    public async Task GetCustomerAnalytics_WithNonExistentCustomer_ShouldReturnNotFound()
    {
        // Arrange
        _shopifyServiceMock.Setup(s => s.GetCustomerAnalyticsAsync(999))
            .ReturnsAsync((CustomerAnalytics?)null);

        // Act
        var result = await _controller.GetCustomerAnalytics(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.Should().Be("Customer with ID 999 not found");
    }

    [Fact]
    public async Task GetCustomerAnalyticsByTags_WithValidTags_ShouldReturnOkWithAnalytics()
    {
        // Arrange
        var analyticsList = new List<CustomerAnalytics>
        {
            new() { CustomerId = 1, Email = "customer1@example.com", Tags = new List<string> { "vip" } },
            new() { CustomerId = 2, Email = "customer2@example.com", Tags = new List<string> { "vip" } }
        };

        _shopifyServiceMock.Setup(s => s.GetCustomerAnalyticsByTagsAsync("vip", 50))
            .ReturnsAsync(analyticsList);

        // Act
        var result = await _controller.GetCustomerAnalyticsByTags("vip", 50);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var returnedAnalytics = okResult.Value.Should().BeOfType<List<CustomerAnalytics>>().Subject;
        returnedAnalytics.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetCustomerAnalyticsByTags_WithInvalidLimit_ShouldReturnBadRequest(int limit)
    {
        // Act
        var result = await _controller.GetCustomerAnalyticsByTags("vip", limit);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().Be("Limit must be between 1 and 100");
    }

    #endregion

    #region Purchase Prediction Tests

    [Fact]
    public async Task CalculateNextPurchaseDate_WithSufficientOrderHistory_ShouldReturnPrediction()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, CreatedAt = DateTime.Now.AddDays(-60), TotalPrice = "100.00" },
            new() { Id = 2, CreatedAt = DateTime.Now.AddDays(-30), TotalPrice = "150.00" },
            new() { Id = 3, CreatedAt = DateTime.Now.AddDays(-10), TotalPrice = "200.00" }
        };

        var predictedDate = DateTime.Now.AddDays(15);

        _shopifyServiceMock.Setup(s => s.GetCustomerOrdersAsync(123, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long?>()))
            .ReturnsAsync(orders);
        _shopifyServiceMock.Setup(s => s.CalculateNextPurchaseDate(orders))
            .Returns(predictedDate);

        // Act
        var result = await _controller.CalculateNextPurchaseDate(123);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var prediction = okResult.Value.Should().BeOfType<NextPurchasePredictionResponse>().Subject;
        prediction.CustomerId.Should().Be(123);
        prediction.PredictedNextPurchaseDate.Should().Be(predictedDate);
        prediction.TotalOrders.Should().Be(3);
        prediction.Confidence.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CalculateNextPurchaseDate_WithInsufficientOrders_ShouldReturnNotFound()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, CreatedAt = DateTime.Now.AddDays(-30), TotalPrice = "100.00" }
        };

        _shopifyServiceMock.Setup(s => s.GetCustomerOrdersAsync(123, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long?>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.CalculateNextPurchaseDate(123);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.Should().Be("Customer not found or insufficient order history for prediction (minimum 2 orders required)");
    }

    [Fact]
    public async Task CalculateNextPurchaseDate_WithUnpredictablePattern_ShouldReturnNotFound()
    {
        // Arrange
        var orders = new List<ShopifyOrder>
        {
            new() { Id = 1, CreatedAt = DateTime.Now.AddDays(-60), TotalPrice = "100.00" },
            new() { Id = 2, CreatedAt = DateTime.Now.AddDays(-30), TotalPrice = "150.00" }
        };

        _shopifyServiceMock.Setup(s => s.GetCustomerOrdersAsync(123, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long?>()))
            .ReturnsAsync(orders);
        _shopifyServiceMock.Setup(s => s.CalculateNextPurchaseDate(orders))
            .Returns((DateTime?)null);

        // Act
        var result = await _controller.CalculateNextPurchaseDate(123);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result;
        notFoundResult.Value.Should().Be("Unable to calculate next purchase date with available data");
    }

    [Fact]
    public async Task GetCustomersLikelyToPurchaseSoon_WithValidParameters_ShouldReturnCustomers()
    {
        // Arrange
        var customers = new List<CustomerAnalytics>
        {
            new() 
            { 
                CustomerId = 1, 
                Email = "customer1@example.com", 
                PredictedNextPurchaseDate = DateTime.Now.AddDays(5)
            }
        };

        _shopifyServiceMock.Setup(s => s.GetCustomersLikelyToPurchaseSoonAsync("vip", 7, 25))
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.GetCustomersLikelyToPurchaseSoon("vip", 7, 25);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var returnedCustomers = okResult.Value.Should().BeOfType<List<CustomerAnalytics>>().Subject;
        returnedCustomers.Should().HaveCount(1);
        returnedCustomers[0].PredictedNextPurchaseDate.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0, 25, "Days threshold must be between 1 and 365")]
    [InlineData(366, 25, "Days threshold must be between 1 and 365")]
    [InlineData(7, 0, "Limit must be between 1 and 50")]
    [InlineData(7, 51, "Limit must be between 1 and 50")]
    public async Task GetCustomersLikelyToPurchaseSoon_WithInvalidParameters_ShouldReturnBadRequest(
        int daysThreshold, int limit, string expectedMessage)
    {
        // Act
        var result = await _controller.GetCustomersLikelyToPurchaseSoon("vip", daysThreshold, limit);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().Be(expectedMessage);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetCustomerAnalytics_WithServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        _shopifyServiceMock.Setup(s => s.GetCustomerAnalyticsAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetCustomerAnalytics(123);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("Internal server error while retrieving customer analytics");
    }

    [Fact]
    public async Task CalculateNextPurchaseDate_WithServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        _shopifyServiceMock.Setup(s => s.GetCustomerOrdersAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long?>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.CalculateNextPurchaseDate(123);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("Internal server error while calculating purchase prediction");
    }

    #endregion
}