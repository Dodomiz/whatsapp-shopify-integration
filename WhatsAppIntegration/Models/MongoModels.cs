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