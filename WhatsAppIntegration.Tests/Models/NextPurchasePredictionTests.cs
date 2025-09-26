using WhatsAppIntegration.Models;

namespace WhatsAppIntegration.Tests.Models;

public class NextPurchasePredictionTests
{
    [Fact]
    public void NextPurchasePrediction_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var prediction = new NextPurchasePrediction
        {
            NextPurchaseDate = DateTime.Now.AddDays(30),
            AverageDaysBetweenPurchases = 25.5,
            PurchaseDates = new List<DateTime> { DateTime.Now.AddDays(-60), DateTime.Now.AddDays(-30) },
            ProductsInCategory = new List<ProductSummary>
            {
                new ProductSummary
                {
                    ProductId = 123,
                    Title = "Test Product",
                    Tags = new List<string> { "automation" },
                    PurchaseCount = 2,
                    TotalQuantityPurchased = 4
                }
            },
            ConfidenceLevel = 0.85,
            HasSufficientData = true,
            PredictionReason = "High confidence prediction based on consistent purchase pattern",
            CalculatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotNull(prediction.NextPurchaseDate);
        Assert.True(prediction.AverageDaysBetweenPurchases > 0);
        Assert.NotEmpty(prediction.PurchaseDates);
        Assert.NotEmpty(prediction.ProductsInCategory);
        Assert.True(prediction.ConfidenceLevel > 0);
        Assert.True(prediction.HasSufficientData);
        Assert.NotEmpty(prediction.PredictionReason);
    }

    [Fact]
    public void ProductSummary_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var productSummary = new ProductSummary
        {
            ProductId = 123,
            Title = "Automation Product",
            Tags = new List<string> { "automation", "includeautomation" },
            PurchaseCount = 3,
            TotalQuantityPurchased = 6,
            LastPurchaseDate = DateTime.Now.AddDays(-10)
        };

        // Assert
        Assert.True(productSummary.ProductId > 0);
        Assert.NotEmpty(productSummary.Title);
        Assert.NotEmpty(productSummary.Tags);
        Assert.True(productSummary.PurchaseCount > 0);
        Assert.True(productSummary.TotalQuantityPurchased > 0);
        Assert.NotNull(productSummary.LastPurchaseDate);
    }

    [Fact]
    public void CategorizedOrdersDocument_ShouldAcceptNextPurchasePredictions()
    {
        // Arrange & Act
        var document = new CategorizedOrdersDocument
        {
            CustomerId = 123,
            Customer = new ShopifyCustomer { Id = 123, FirstName = "Test", LastName = "Customer" },
            AutomationNextPurchase = new NextPurchasePrediction
            {
                NextPurchaseDate = DateTime.Now.AddDays(30),
                HasSufficientData = true,
                ConfidenceLevel = 0.8,
                PredictionReason = "Automation prediction test"
            },
            DogExtraNextPurchase = new NextPurchasePrediction
            {
                NextPurchaseDate = DateTime.Now.AddDays(45),
                HasSufficientData = true,
                ConfidenceLevel = 0.6,
                PredictionReason = "DogExtra prediction test"
            }
        };

        // Assert
        Assert.NotNull(document.AutomationNextPurchase);
        Assert.NotNull(document.DogExtraNextPurchase);
        Assert.True(document.AutomationNextPurchase.ConfidenceLevel > 0);
        Assert.True(document.DogExtraNextPurchase.ConfidenceLevel > 0);
        Assert.Contains("Automation", document.AutomationNextPurchase.PredictionReason);
        Assert.Contains("DogExtra", document.DogExtraNextPurchase.PredictionReason);
    }
}