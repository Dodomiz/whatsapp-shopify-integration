namespace WhatsAppIntegration.Configuration;

/// <summary>
/// MongoDB configuration settings loaded from environment variables
/// Environment variables: MongoDB__ConnectionString, MongoDB__DatabaseName, MongoDB__CategorizedOrdersCollection
/// </summary>
public class MongoDbConfig
{
    /// <summary>
    /// MongoDB connection string (from MongoDB__ConnectionString environment variable)
    /// </summary>
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    
    /// <summary>
    /// Database name (from MongoDB__DatabaseName environment variable)
    /// </summary>
    public string DatabaseName { get; set; } = "WhatsAppIntegration";
    
    /// <summary>
    /// Collection name for categorized orders (from MongoDB__CategorizedOrdersCollection environment variable)
    /// </summary>
    public string CategorizedOrdersCollection { get; set; } = "CategorizedOrders";
}