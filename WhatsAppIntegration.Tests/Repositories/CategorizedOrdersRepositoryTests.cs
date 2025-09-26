using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WhatsAppIntegration.Configuration;
using WhatsAppIntegration.Models;
using WhatsAppIntegration.Repositories;

namespace WhatsAppIntegration.Tests.Repositories;

public class CategorizedOrdersRepositoryTests : IDisposable
{
    private readonly CategorizedOrdersRepository _repository;
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _database;
    private readonly string _testDatabaseName;

    public CategorizedOrdersRepositoryTests()
    {
        // Use a unique test database name to avoid conflicts
        _testDatabaseName = $"WhatsAppIntegrationTest_{Guid.NewGuid():N}";
        
        var config = Options.Create(new MongoDbConfig
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = _testDatabaseName,
            CategorizedOrdersCollection = "CategorizedOrders"
        });

        var logger = new LoggerFactory().CreateLogger<CategorizedOrdersRepository>();
        
        _mongoClient = new MongoClient(config.Value.ConnectionString);
        _database = _mongoClient.GetDatabase(_testDatabaseName);
        
        _repository = new CategorizedOrdersRepository(config, logger);
    }

    [Fact]
    public async Task SaveCategorizedOrdersAsync_WithNewDocument_ShouldCreateDocument()
    {
        // Arrange
        var document = new CategorizedOrdersDocument
        {
            CustomerId = 123,
            Customer = new ShopifyCustomer { Id = 123, FirstName = "John", LastName = "Doe" },
            AutomationProductsOrders = new List<ShopifyOrder>
            {
                new() { Id = 1, TotalPrice = "100.00" }
            },
            DogExtraProductsOrders = new List<ShopifyOrder>(),
            Filters = new OrderFilters { Status = "any", Limit = null, MinOrdersPerCustomer = 1 }
        };

        // Act
        var result = await _repository.SaveCategorizedOrdersAsync(document);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.CustomerId.Should().Be(123);
        result.Customer.FirstName.Should().Be("John");
        result.AutomationProductsOrders.Should().HaveCount(1);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetCategorizedOrdersByCustomerIdAsync_WithExistingCustomer_ShouldReturnDocument()
    {
        // Arrange
        var document = new CategorizedOrdersDocument
        {
            CustomerId = 456,
            Customer = new ShopifyCustomer { Id = 456, FirstName = "Jane", LastName = "Smith" },
            AutomationProductsOrders = new List<ShopifyOrder>(),
            DogExtraProductsOrders = new List<ShopifyOrder>
            {
                new() { Id = 2, TotalPrice = "200.00" }
            }
        };
        
        await _repository.SaveCategorizedOrdersAsync(document);

        // Act
        var result = await _repository.GetCategorizedOrdersByCustomerIdAsync(456);

        // Assert
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be(456);
        result.Customer.FirstName.Should().Be("Jane");
        result.DogExtraProductsOrders.Should().HaveCount(1);
        result.DogExtraProductsOrders[0].TotalPrice.Should().Be("200.00");
    }

    [Fact]
    public async Task GetCategorizedOrdersByCustomerIdAsync_WithNonExistentCustomer_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetCategorizedOrdersByCustomerIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCategorizedOrdersAsync_WithExistingDocument_ShouldUpdateSuccessfully()
    {
        // Arrange
        var document = new CategorizedOrdersDocument
        {
            CustomerId = 789,
            Customer = new ShopifyCustomer { Id = 789, FirstName = "Bob", LastName = "Johnson" },
            AutomationProductsOrders = new List<ShopifyOrder>
            {
                new() { Id = 3, TotalPrice = "300.00" }
            }
        };
        
        var savedDocument = await _repository.SaveCategorizedOrdersAsync(document);
        
        // Modify the document
        savedDocument.AutomationProductsOrders.Add(new ShopifyOrder { Id = 4, TotalPrice = "400.00" });

        // Act
        var updateResult = await _repository.UpdateCategorizedOrdersAsync(savedDocument);

        // Assert
        updateResult.Should().BeTrue();
        
        var retrievedDocument = await _repository.GetCategorizedOrdersByCustomerIdAsync(789);
        retrievedDocument!.AutomationProductsOrders.Should().HaveCount(2);
        retrievedDocument.AutomationProductsOrders.Should().Contain(o => o.Id == 4);
    }

    [Fact]
    public async Task DeleteCategorizedOrdersByCustomerIdAsync_WithExistingCustomer_ShouldDeleteSuccessfully()
    {
        // Arrange
        var document = new CategorizedOrdersDocument
        {
            CustomerId = 101,
            Customer = new ShopifyCustomer { Id = 101, FirstName = "Alice", LastName = "Wilson" }
        };
        
        await _repository.SaveCategorizedOrdersAsync(document);

        // Act
        var deleteResult = await _repository.DeleteCategorizedOrdersByCustomerIdAsync(101);

        // Assert
        deleteResult.Should().BeTrue();
        
        var retrievedDocument = await _repository.GetCategorizedOrdersByCustomerIdAsync(101);
        retrievedDocument.Should().BeNull();
    }

    [Fact]
    public async Task GetAllCategorizedOrdersAsync_WithMultipleDocuments_ShouldReturnAllDocuments()
    {
        // Arrange
        var documents = new[]
        {
            new CategorizedOrdersDocument { CustomerId = 201, Customer = new ShopifyCustomer { Id = 201 } },
            new CategorizedOrdersDocument { CustomerId = 202, Customer = new ShopifyCustomer { Id = 202 } },
            new CategorizedOrdersDocument { CustomerId = 203, Customer = new ShopifyCustomer { Id = 203 } }
        };

        foreach (var doc in documents)
        {
            await _repository.SaveCategorizedOrdersAsync(doc);
        }

        // Act
        var result = await _repository.GetAllCategorizedOrdersAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(d => d.CustomerId == 201);
        result.Should().Contain(d => d.CustomerId == 202);
        result.Should().Contain(d => d.CustomerId == 203);
    }

    [Fact]
    public async Task GetCategorizedOrdersByDateRangeAsync_WithValidRange_ShouldReturnDocumentsInRange()
    {
        // Arrange
        var oldDocument = new CategorizedOrdersDocument
        {
            CustomerId = 301,
            Customer = new ShopifyCustomer { Id = 301 },
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        
        var recentDocument = new CategorizedOrdersDocument
        {
            CustomerId = 302,
            Customer = new ShopifyCustomer { Id = 302 },
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        await _repository.SaveCategorizedOrdersAsync(oldDocument);
        await _repository.SaveCategorizedOrdersAsync(recentDocument);

        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddHours(1);

        // Act
        var result = await _repository.GetCategorizedOrdersByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result[0].CustomerId.Should().Be(302);
    }

    public void Dispose()
    {
        // Clean up test database
        _mongoClient.DropDatabase(_testDatabaseName);
    }
}