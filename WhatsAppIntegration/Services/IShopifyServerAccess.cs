using WhatsAppIntegration.Models;

namespace WhatsAppIntegration.Services;

/// <summary>
/// Interface for low-level Shopify API server access operations
/// </summary>
public interface IShopifyServerAccess
{
    /// <summary>
    /// Get all customers with specific tags
    /// </summary>
    /// <param name="tags">Comma-separated list of tags to filter by</param>
    /// <param name="limit">Maximum number of customers to retrieve (default: 250)</param>
    /// <param name="excludeTags">Comma-separated list of tags to exclude customers that have these tags</param>
    /// <returns>List of customers matching the specified tags and not having excluded tags</returns>
    Task<List<ShopifyCustomer>> GetCustomersWithTagsAsync(string tags, int? limit = 250, string? excludeTags = null);

    /// <summary>
    /// Get all customers from the store with automatic pagination
    /// </summary>
    /// <param name="limit">Maximum number of customers to retrieve (null for unlimited)</param>
    /// <returns>List of all customers (fetched in batches of 250 until completion)</returns>
    Task<List<ShopifyCustomer>> GetAllCustomersAsync(int? limit = null);

    /// <summary>
    /// Get a specific customer by ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Customer details if found, null otherwise</returns>
    Task<ShopifyCustomer?> GetCustomerAsync(long customerId);

    /// <summary>
    /// Get the total count of customers in the store
    /// </summary>
    /// <returns>Total number of customers</returns>
    Task<int> GetCustomersCountAsync();

    /// <summary>
    /// Get all orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="status">Order status filter (any, open, closed, cancelled)</param>
    /// <param name="limit">Maximum number of orders to retrieve (default: 250)</param>
    /// <param name="sinceId">Retrieve orders created after this ID</param>
    /// <returns>List of orders for the customer</returns>
    Task<List<ShopifyOrder>> GetCustomerOrdersAsync(long customerId, string status = "any", int limit = 250, long? sinceId = null);

    /// <summary>
    /// Get all orders from the store (defaults to last 365 days if no date filters provided)
    /// </summary>
    /// <param name="status">Order status filter (any, open, closed, cancelled)</param>
    /// <param name="limit">Maximum number of orders per request (null for unlimited)</param>
    /// <param name="sinceId">Retrieve orders created after this ID</param>
    /// <param name="createdAtMin">Show orders created at or after date</param>
    /// <param name="createdAtMax">Show orders created at or before date</param>
    /// <param name="customerIds">Optional list of customer IDs to filter by (null for all customers)</param>
    /// <returns>List of orders</returns>
    Task<List<ShopifyOrder>> GetAllOrdersAsync(string status = "any", int? limit = null, long? sinceId = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null, List<long>? customerIds = null);

    /// <summary>
    /// Get all orders from the last N days
    /// </summary>
    /// <param name="days">Number of days to look back (default: 365)</param>
    /// <param name="status">Order status filter (any, open, closed, cancelled)</param>
    /// <param name="limit">Maximum number of orders per request (null for unlimited)</param>
    /// <returns>List of orders from the last N days</returns>
    Task<List<ShopifyOrder>> GetOrdersFromLastDaysAsync(int days = 365, string status = "any", int? limit = null);

    /// <summary>
    /// Get all orders grouped by customer ID with filtering options
    /// </summary>
    /// <param name="status">Order status filter (any, open, closed, cancelled)</param>
    /// <param name="limit">Maximum number of orders to retrieve (null for unlimited)</param>
    /// <param name="minOrdersPerCustomer">Minimum number of orders required per customer to be included</param>
    /// <param name="targetProductIds">List of product IDs to filter orders (only orders containing these products)</param>
    /// <param name="createdAtMin">Show orders created at or after date</param>
    /// <param name="createdAtMax">Show orders created at or before date</param>
    /// <returns>Dictionary where key is customer ID and value is list of orders</returns>
    Task<Dictionary<long, List<ShopifyOrder>>> GetOrdersByCustomerAsync(string status = "any", int? limit = null, int? minOrdersPerCustomer = null, List<long>? targetProductIds = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null);

    /// <summary>
    /// Get all products from the store
    /// </summary>
    /// <param name="limit">Maximum number of products per request (default: 250)</param>
    /// <param name="sinceId">Retrieve products created after this ID</param>
    /// <param name="vendor">Filter products by vendor</param>
    /// <param name="productType">Filter products by product type</param>
    /// <returns>List of products</returns>
    Task<List<ShopifyProduct>> GetAllProductsAsync(int limit = 250, long? sinceId = null, string? vendor = null, string? productType = null);

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Product details if found, null otherwise</returns>
    Task<ShopifyProduct?> GetProductAsync(long productId);

    /// <summary>
    /// Get the total count of products in the store
    /// </summary>
    /// <returns>Total number of products</returns>
    Task<int> GetProductsCountAsync();

    /// <summary>
    /// Get all products with specific tags
    /// </summary>
    /// <param name="tags">Comma-separated list of tags to filter by</param>
    /// <param name="limit">Maximum number of products to retrieve (default: 250)</param>
    /// <returns>List of products matching the specified tags</returns>
    Task<List<ShopifyProduct>> GetProductsWithTagsAsync(string tags, int limit = 250);

    /// <summary>
    /// Get all products with specific fields
    /// </summary>
    /// <param name="fields">Comma-separated list of fields to retrieve</param>
    /// <param name="limit">Maximum number of products per request (default: 250)</param>
    /// <param name="sinceId">Retrieve products created after this ID</param>
    /// <returns>List of products with specified fields</returns>
    Task<List<ShopifyProduct>> GetAllProductsWithFieldsAsync(string fields, int limit = 250, long? sinceId = null);

    /// <summary>
    /// Fetch customers with pagination support
    /// </summary>
    /// <param name="limit">Maximum number of customers per page</param>
    /// <param name="pageInfo">Page info token for pagination</param>
    /// <returns>Tuple of customers list and next page URL</returns>
    Task<(List<ShopifyCustomer> customers, string? nextPageUrl)> FetchCustomersWithPaginationAsync(int limit, string? pageInfo = null);

    /// <summary>
    /// Get all orders for a specific customer with additional filtering options
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="status">Order status filter</param>
    /// <param name="limit">Maximum number of orders</param>
    /// <param name="sinceId">Retrieve orders created after this ID</param>
    /// <param name="createdAtMin">Show orders created at or after date</param>
    /// <param name="createdAtMax">Show orders created at or before date</param>
    /// <returns>List of orders for the customer</returns>
    Task<List<ShopifyOrder>> GetCustomerOrdersWithFiltersAsync(long customerId, string status, int? limit, long? sinceId, DateTime? createdAtMin, DateTime? createdAtMax);
}