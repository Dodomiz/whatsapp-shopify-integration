using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WhatsAppIntegration.Controllers;
using WhatsAppIntegration.Models;
using WhatsAppIntegration.Repositories;
using WhatsAppIntegration.Services;

namespace WhatsAppIntegration.Tests.Controllers;

public class CategorizedOrdersEndpointsTests
{
    private readonly Mock<IShopifyService> _shopifyServiceMock;
    private readonly Mock<ICategorizedOrdersRepository> _repositoryMock;
    private readonly Mock<ILogger<ShopifyController>> _loggerMock;
    private readonly ShopifyController _controller;

    public CategorizedOrdersEndpointsTests()
    {
        _shopifyServiceMock = new Mock<IShopifyService>();
        _repositoryMock = new Mock<ICategorizedOrdersRepository>();
        _loggerMock = new Mock<ILogger<ShopifyController>>();
        _controller = new ShopifyController(_shopifyServiceMock.Object, _repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task PostCategorizedOrders_ShouldProcessAndSaveToDatabase()
    {
        // Arrange
        var shopifyResponse = new ShopifyCategorizedOrdersByCustomerResponse();
        shopifyResponse.OrdersByCustomer[123] = new CustomerCategorizedOrders
        {
            Customer = new ShopifyCustomer { Id = 123, FirstName = "Test", LastName = "Customer" },
            AutomationProductsOrders = new List<ShopifyOrder> { new ShopifyOrder { Id = 1 } }
        };

        _shopifyServiceMock.Setup(s => s.GetCategorizedOrdersByCustomerAsync("any", null, null, null, null))
            .ReturnsAsync(shopifyResponse);

        // Act - POST endpoint
        var result = await _controller.ProcessCategorizedOrdersByCustomer();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<CategorizedOrdersProcessedResponse>().Subject;
        response.ProcessedCustomersCount.Should().Be(1);
        response.ProcessedCustomerIds.Should().Contain(123);
    }

    [Fact]
    public async Task GetCategorizedOrders_ShouldReturnDataFromDatabase()
    {
        // Arrange
        var dbResponse = new ShopifyCategorizedOrdersByCustomerResponse();
        dbResponse.OrdersByCustomer[456] = new CustomerCategorizedOrders
        {
            Customer = new ShopifyCustomer { Id = 456, FirstName = "DB", LastName = "Customer" },
            AutomationProductsOrders = new List<ShopifyOrder> { new ShopifyOrder { Id = 2 } }
        };

        _repositoryMock.Setup(r => r.GetCategorizedOrdersResponseAsync(null))
            .ReturnsAsync(dbResponse);

        // Act - GET endpoint
        var result = await _controller.GetCategorizedOrdersFromDatabase();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<ShopifyCategorizedOrdersByCustomerResponse>().Subject;
        response.TotalCustomers.Should().Be(1);
        response.OrdersByCustomer.Should().ContainKey(456);
    }

    [Fact]
    public async Task PutCategorizedOrders_ShouldUpdateExistingAndCreateNewCustomers()
    {
        // Arrange
        var shopifyResponse = new ShopifyCategorizedOrdersByCustomerResponse();
        shopifyResponse.OrdersByCustomer[123] = new CustomerCategorizedOrders
        {
            Customer = new ShopifyCustomer { Id = 123, FirstName = "Updated", LastName = "Customer" },
            AutomationProductsOrders = new List<ShopifyOrder> { new ShopifyOrder { Id = 3 } }
        };
        shopifyResponse.OrdersByCustomer[999] = new CustomerCategorizedOrders
        {
            Customer = new ShopifyCustomer { Id = 999, FirstName = "New", LastName = "Customer" },
            AutomationProductsOrders = new List<ShopifyOrder> { new ShopifyOrder { Id = 4 } }
        };

        var existingDocuments = new List<CategorizedOrdersDocument>
        {
            new CategorizedOrdersDocument { CustomerId = 123 } // Only customer 123 exists in DB
        };

        var categorizedProducts = new ShopifyCategorizedProductsResponse
        {
            AutomationProducts = new List<ShopifyProduct> { new ShopifyProduct { Id = 1, Tags = "automation" } },
            DogExtraProducts = new List<ShopifyProduct> { new ShopifyProduct { Id = 2, Tags = "dogextra" } }
        };

        _shopifyServiceMock.Setup(s => s.GetCategorizedOrdersByCustomerAsync("any", null, null, null, null))
            .ReturnsAsync(shopifyResponse);
        _shopifyServiceMock.Setup(s => s.GetCategorizedProductsAsync())
            .ReturnsAsync(categorizedProducts);
        _repositoryMock.Setup(r => r.GetAllCategorizedOrdersAsync(It.IsAny<int?>()))
            .ReturnsAsync(existingDocuments);

        // Act - PUT endpoint
        var result = await _controller.UpdateCategorizedOrdersByCustomer();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var response = okResult.Value.Should().BeOfType<CategorizedOrdersProcessedResponse>().Subject;
        response.ProcessedCustomersCount.Should().Be(2); // Both customers should be processed
        response.ProcessedCustomerIds.Should().Contain(123); // Existing customer updated
        response.ProcessedCustomerIds.Should().Contain(999); // New customer created
    }
}