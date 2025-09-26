using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace WhatsAppIntegration.Models;

/// <summary>
/// MongoDB document for storing categorized orders by customer
/// </summary>
public class CategorizedOrdersDocument
{
    /// <summary>
    /// MongoDB document ID
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    /// <summary>
    /// Shopify customer ID
    /// </summary>
    [BsonElement("customerId")]
    public long CustomerId { get; set; }
    
    /// <summary>
    /// Customer information
    /// </summary>
    [BsonElement("customer")]
    public ShopifyCustomer Customer { get; set; } = new();
    
    /// <summary>
    /// Automation products orders for this customer
    /// </summary>
    [BsonElement("automationProductsOrders")]
    public List<ShopifyOrder> AutomationProductsOrders { get; set; } = new();
    
    /// <summary>
    /// Dog extra products orders for this customer
    /// </summary>
    [BsonElement("dogExtraProductsOrders")]
    public List<ShopifyOrder> DogExtraProductsOrders { get; set; } = new();
    
    /// <summary>
    /// Timestamp when this document was created
    /// </summary>
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when this document was last updated
    /// </summary>
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Filters used when generating this data
    /// </summary>
    [BsonElement("filters")]
    public OrderFilters Filters { get; set; } = new();
    
    /// <summary>
    /// Total number of automation orders for this customer
    /// </summary>
    [BsonElement("totalAutomationOrders")]
    public int TotalAutomationOrders => AutomationProductsOrders.Count;
    
    /// <summary>
    /// Total number of dog extra orders for this customer
    /// </summary>
    [BsonElement("totalDogExtraOrders")]
    public int TotalDogExtraOrders => DogExtraProductsOrders.Count;
    
    /// <summary>
    /// Total number of orders for this customer
    /// </summary>
    [BsonElement("totalOrders")]
    public int TotalOrders => TotalAutomationOrders + TotalDogExtraOrders;
    
    /// <summary>
    /// Next purchase prediction for automation products
    /// </summary>
    [BsonElement("automationNextPurchase")]
    public NextPurchasePrediction? AutomationNextPurchase { get; set; }
    
    /// <summary>
    /// Next purchase prediction for dog extra products
    /// </summary>
    [BsonElement("dogExtraNextPurchase")]
    public NextPurchasePrediction? DogExtraNextPurchase { get; set; }
}

/// <summary>
/// Filters applied when generating the categorized orders
/// </summary>
public class OrderFilters
{
    /// <summary>
    /// Order status filter
    /// </summary>
    [BsonElement("status")]
    public string Status { get; set; } = "any";
    
    /// <summary>
    /// Maximum number of orders limit
    /// </summary>
    [BsonElement("limit")]
    public int? Limit { get; set; }
    
    /// <summary>
    /// Minimum orders per customer filter
    /// </summary>
    [BsonElement("minOrdersPerCustomer")]
    public int? MinOrdersPerCustomer { get; set; }
    
    /// <summary>
    /// Created at minimum date filter
    /// </summary>
    [BsonElement("createdAtMin")]
    public DateTime? CreatedAtMin { get; set; }
    
    /// <summary>
    /// Created at maximum date filter
    /// </summary>
    [BsonElement("createdAtMax")]
    public DateTime? CreatedAtMax { get; set; }
}

/// <summary>
/// Next purchase prediction data for a specific product category
/// </summary>
public class NextPurchasePrediction
{
    /// <summary>
    /// Predicted next purchase date
    /// </summary>
    [BsonElement("nextPurchaseDate")]
    public DateTime? NextPurchaseDate { get; set; }
    
    /// <summary>
    /// Average days between purchases
    /// </summary>
    [BsonElement("averageDaysBetweenPurchases")]
    public double? AverageDaysBetweenPurchases { get; set; }
    
    /// <summary>
    /// Purchase dates used for calculation
    /// </summary>
    [BsonElement("purchaseDates")]
    public List<DateTime> PurchaseDates { get; set; } = new();
    
    /// <summary>
    /// Product details involved in the category
    /// </summary>
    [BsonElement("productsInCategory")]
    public List<ProductSummary> ProductsInCategory { get; set; } = new();
    
    /// <summary>
    /// Confidence level of the prediction (0-1)
    /// </summary>
    [BsonElement("confidenceLevel")]
    public double ConfidenceLevel { get; set; }
    
    /// <summary>
    /// Whether there's enough data to make a prediction
    /// </summary>
    [BsonElement("hasSufficientData")]
    public bool HasSufficientData { get; set; }
    
    /// <summary>
    /// Reason for prediction or lack thereof
    /// </summary>
    [BsonElement("predictionReason")]
    public string PredictionReason { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when prediction was calculated
    /// </summary>
    [BsonElement("calculatedAt")]
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Summary of product information for next purchase prediction
/// </summary>
public class ProductSummary
{
    /// <summary>
    /// Product ID
    /// </summary>
    [BsonElement("productId")]
    public long ProductId { get; set; }
    
    /// <summary>
    /// Product title
    /// </summary>
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Product tags
    /// </summary>
    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Number of times purchased
    /// </summary>
    [BsonElement("purchaseCount")]
    public int PurchaseCount { get; set; }
    
    /// <summary>
    /// Total quantity purchased
    /// </summary>
    [BsonElement("totalQuantityPurchased")]
    public int TotalQuantityPurchased { get; set; }
    
    /// <summary>
    /// Last purchase date for this product
    /// </summary>
    [BsonElement("lastPurchaseDate")]
    public DateTime? LastPurchaseDate { get; set; }
}