using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WhatsAppIntegration.Models;
using WhatsAppIntegration.Services;

namespace WhatsAppIntegration.Controllers;

/// <summary>
/// Shopify Integration Controller
/// Provides endpoints for fetching customers, orders, products, and analytics from Shopify store
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Shopify")]
public class ShopifyController : ControllerBase
{
    private readonly IShopifyService _shopifyService;
    private readonly ILogger<ShopifyController> _logger;

    /// <summary>
    /// Initializes a new instance of the ShopifyController
    /// </summary>
    /// <param name="shopifyService">Shopify service instance</param>
    /// <param name="logger">Logger instance</param>
    public ShopifyController(IShopifyService shopifyService, ILogger<ShopifyController> logger)
    {
        _shopifyService = shopifyService;
        _logger = logger;
    }

    /// <summary>
    /// Get customers with specific tags
    /// </summary>
    /// <param name="tags">Comma-separated list of tags to filter by</param>
    /// <param name="limit">Maximum number of customers to retrieve (default: 50, max: 250)</param>
    /// <param name="excludeTags">Comma-separated list of tags to exclude customers that have these tags</param>
    /// <returns>List of customers matching the specified tags and not having excluded tags</returns>
    /// <response code="200">Customers retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("customers/by-tags")]
    [ProducesResponseType(typeof(ShopifyCustomersResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCustomersByTags(
        [FromQuery] string? tags,
        [FromQuery] int? limit,
        [FromQuery] string? excludeTags = null)
    {

        try
        {
            var customers = await _shopifyService.GetCustomersWithTagsAsync(tags, limit, excludeTags);
            
            _logger.LogInformation("Retrieved {Count} customers with tags: {Tags}, excluding tags: {ExcludeTags}", 
                customers.Count, tags, excludeTags ?? "none");
            
            return Ok(new ShopifyCustomersResponse { Customers = customers });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers with tags: {Tags}", tags);
            return StatusCode(500, "Internal server error while retrieving customers");
        }
    }

    /// <summary>
    /// Get all customers from the store with automatic pagination
    /// </summary>
    /// <param name="limit">Maximum number of customers to retrieve (null for unlimited, fetches all customers in batches of 250)</param>
    /// <returns>List of all customers</returns>
    /// <response code="200">Customers retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("customers")]
    [ProducesResponseType(typeof(ShopifyCustomersResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllCustomers(
        [FromQuery] int? limit = null)
    {
        if (limit.HasValue && limit.Value <= 0)
        {
            return BadRequest("Limit must be greater than 0");
        }

        try
        {
            var customers = await _shopifyService.GetAllCustomersAsync(limit);
            
            _logger.LogInformation("Retrieved {Count} total customers", customers.Count);
            
            return Ok(new ShopifyCustomersResponse { Customers = customers });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all customers");
            return StatusCode(500, "Internal server error while retrieving customers");
        }
    }

    /// <summary>
    /// Get a specific customer by ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Customer details</returns>
    /// <response code="200">Customer retrieved successfully</response>
    /// <response code="404">Customer not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("customers/{customerId}")]
    [ProducesResponseType(typeof(ShopifyCustomer), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCustomer(long customerId)
    {
        try
        {
            var customer = await _shopifyService.GetCustomerAsync(customerId);
            
            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found", customerId);
                return NotFound($"Customer with ID {customerId} not found");
            }

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {CustomerId}", customerId);
            return StatusCode(500, "Internal server error while retrieving customer");
        }
    }

    /// <summary>
    /// Get the total count of customers in the store
    /// </summary>
    /// <returns>Total number of customers</returns>
    /// <response code="200">Customer count retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("customers/count")]
    [ProducesResponseType(typeof(int), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCustomersCount()
    {
        try
        {
            var count = await _shopifyService.GetCustomersCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers count");
            return StatusCode(500, "Internal server error while retrieving customers count");
        }
    }

    /// <summary>
    /// Get all orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="status">Order status filter (any, open, closed, cancelled)</param>
    /// <param name="limit">Maximum number of orders to retrieve (default: 50, max: 250)</param>
    /// <param name="sinceId">Retrieve orders created after this ID</param>
    /// <returns>List of customer orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("customers/{customerId}/orders")]
    [ProducesResponseType(typeof(ShopifyOrdersResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCustomerOrders(
        long customerId,
        [FromQuery] string status = "any",
        [FromQuery] int limit = 50,
        [FromQuery] long? sinceId = null)
    {
        if (limit <= 0 || limit > 250)
        {
            return BadRequest("Limit must be between 1 and 250");
        }

        var validStatuses = new[] { "any", "open", "closed", "cancelled" };
        if (!validStatuses.Contains(status.ToLower()))
        {
            return BadRequest($"Status must be one of: {string.Join(", ", validStatuses)}");
        }

        try
        {
            var orders = await _shopifyService.GetCustomerOrdersAsync(customerId, status, limit, sinceId);
            
            _logger.LogInformation("Retrieved {Count} orders for customer {CustomerId}", orders.Count, customerId);
            
            return Ok(new ShopifyOrdersResponse { Orders = orders });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer {CustomerId}", customerId);
            return StatusCode(500, "Internal server error while retrieving customer orders");
        }
    }

    /// <summary>
    /// Get all orders from the store
    /// </summary>
    /// <param name="status">Order status filter (any, open, closed, cancelled)</param>
    /// <param name="limit">Maximum number of orders to retrieve (null for unlimited)</param>
    /// <param name="sinceId">Retrieve orders created after this ID</param>
    /// <param name="createdAtMin">Show orders created at or after this date (ISO 8601)</param>
    /// <param name="createdAtMax">Show orders created at or before this date (ISO 8601)</param>
    /// <returns>List of orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(ShopifyOrdersResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] string status = "any",
        [FromQuery] int? limit = null,
        [FromQuery] long? sinceId = null,
        [FromQuery] DateTime? createdAtMin = null,
        [FromQuery] DateTime? createdAtMax = null)
    {
        var validStatuses = new[] { "any", "open", "closed", "cancelled" };
        if (!validStatuses.Contains(status.ToLower()))
        {
            return BadRequest($"Status must be one of: {string.Join(", ", validStatuses)}");
        }

        try
        {
            var orders = await _shopifyService.GetAllOrdersAsync(status, limit, sinceId, createdAtMin, createdAtMax);
            
            _logger.LogInformation("Retrieved {Count} total orders", orders.Count);
            
            return Ok(new ShopifyOrdersResponse { Orders = orders });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders");
            return StatusCode(500, "Internal server error while retrieving orders");
        }
    }

    /// <summary>
    /// Get orders from the last N days
    /// </summary>
    /// <param name="days">Number of days to look back (default: 365)</param>
    /// <param name="status">Order status filter (any, open, closed, cancelled)</param>
    /// <param name="limit">Maximum number of orders to retrieve (null for unlimited)</param>
    /// <returns>List of orders from the last N days</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("orders/last-days")]
    [ProducesResponseType(typeof(ShopifyOrdersResponse), 200)]
    public async Task<IActionResult> GetOrdersFromLastDays(
        [FromQuery] int days = 365,
        [FromQuery] string status = "any",
        [FromQuery] int? limit = null)
    {
        try
        {
            if (days <= 0 || days > 3650) // Max 10 years
            {
                return BadRequest("Days must be between 1 and 3650");
            }

            var validStatuses = new[] { "any", "open", "closed", "cancelled" };
            if (!validStatuses.Contains(status.ToLower()))
            {
                return BadRequest($"Status must be one of: {string.Join(", ", validStatuses)}");
            }

            _logger.LogInformation("Getting orders from last {Days} days with status: {Status}, limit: {Limit}", 
                days, status, limit);

            var orders = await _shopifyService.GetOrdersFromLastDaysAsync(days, status, limit);
            
            return Ok(new ShopifyOrdersResponse { Orders = orders });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders from last {Days} days", days);
            return StatusCode(500, $"Internal server error while retrieving orders from last {days} days");
        }
    }

    /// <summary>
    /// Get orders grouped by customer ID with filtering options
    /// </summary>
    /// <param name="status">Order status filter (any, open, closed, cancelled)</param>
    /// <param name="limit">Maximum number of orders to retrieve (null for unlimited)</param>
    /// <param name="minOrdersPerCustomer">Minimum number of orders required per customer to be included</param>
    /// <param name="productIds">Comma-separated list of product IDs - only include orders containing these products</param>
    /// <param name="createdAtMin">Show orders created at or after date</param>
    /// <param name="createdAtMax">Show orders created at or before date</param>
    /// <returns>Orders grouped by customer ID</returns>
    /// <response code="200">Orders grouped by customer retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("orders/by-customer")]
    [ProducesResponseType(typeof(ShopifyOrdersByCustomerResponse), 200)]
    public async Task<IActionResult> GetOrdersByCustomer(
        [FromQuery] string status = "any",
        [FromQuery] int? limit = null,
        [FromQuery] int? minOrdersPerCustomer = null,
        [FromQuery] string? productIds = null,
        [FromQuery] DateTime? createdAtMin = null,
        [FromQuery] DateTime? createdAtMax = null)
    {
        try
        {
            _logger.LogInformation("Getting orders grouped by customer with status: {Status}, limit: {Limit}, minOrdersPerCustomer: {MinOrders}, productIds: {ProductIds}", 
                status, limit, minOrdersPerCustomer, productIds);

            // Convert comma-separated productIds string to List<long>
            List<long>? productIdsList = null;
            if (!string.IsNullOrEmpty(productIds))
            {
                productIdsList = productIds.Split(',')
                    .Where(id => long.TryParse(id.Trim(), out _))
                    .Select(id => long.Parse(id.Trim()))
                    .ToList();
            }

            var ordersByCustomer = await _shopifyService.GetOrdersByCustomerAsync(status, limit, minOrdersPerCustomer, productIdsList, createdAtMin, createdAtMax);
            
            var response = new ShopifyOrdersByCustomerResponse
            {
                OrdersByCustomer = ordersByCustomer
            };

            _logger.LogInformation("Successfully retrieved orders for {CustomerCount} customers with {TotalOrders} total orders", 
                response.TotalCustomers, response.TotalOrders);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders grouped by customer");
            return StatusCode(500, "Internal server error while retrieving orders by customer");
        }
    }

    /// <summary>
    /// Get orders grouped by customer ID and categorized by product type (AutomationProducts and DogExtraProducts)
    /// </summary>
    /// <param name="status">Order status filter (any, open, closed, cancelled)</param>
    /// <param name="limit">Maximum number of orders to retrieve (null for unlimited)</param>
    /// <param name="minOrdersPerCustomer">Minimum number of orders required per customer to be included</param>
    /// <param name="createdAtMin">Show orders created at or after date</param>
    /// <param name="createdAtMax">Show orders created at or before date</param>
    /// <returns>Orders grouped by customer ID and categorized by product type</returns>
    /// <response code="200">Categorized orders grouped by customer retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("orders/by-customer/categorized")]
    [ProducesResponseType(typeof(ShopifyCategorizedOrdersByCustomerResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCategorizedOrdersByCustomer(
        [FromQuery] string status = "any",
        [FromQuery] int? limit = null,
        [FromQuery] int? minOrdersPerCustomer = null,
        [FromQuery] DateTime? createdAtMin = null,
        [FromQuery] DateTime? createdAtMax = null)
    {
        try
        {
            _logger.LogInformation("Getting categorized orders grouped by customer with status: {Status}, limit: {Limit}, minOrdersPerCustomer: {MinOrders}", 
                status, limit, minOrdersPerCustomer);

            // Validate status parameter
            var validStatuses = new[] { "any", "open", "closed", "cancelled" };
            if (!validStatuses.Contains(status.ToLowerInvariant()))
            {
                return BadRequest($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");
            }

            var response = await _shopifyService.GetCategorizedOrdersByCustomerAsync(status, limit, minOrdersPerCustomer, createdAtMin, createdAtMax);

            _logger.LogInformation("Successfully retrieved categorized orders for {CustomerCount} customers: {AutomationOrders} automation orders, {DogExtraOrders} dog extra orders", 
                response.TotalCustomers, response.TotalAutomationOrders, response.TotalDogExtraOrders);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categorized orders grouped by customer");
            return StatusCode(500, "Internal server error while retrieving categorized orders by customer");
        }
    }

    /// <summary>
    /// Get all products from the store
    /// </summary>
    /// <param name="limit">Maximum number of products to retrieve (default: 50, max: 250)</param>
    /// <param name="sinceId">Retrieve products created after this ID</param>
    /// <param name="vendor">Filter products by vendor</param>
    /// <param name="productType">Filter products by product type</param>
    /// <returns>List of products</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("products")]
    [ProducesResponseType(typeof(ShopifyProductsResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] int limit = 50,
        [FromQuery] long? sinceId = null,
        [FromQuery] string? vendor = null,
        [FromQuery] string? productType = null)
    {
        if (limit <= 0 || limit > 250)
        {
            return BadRequest("Limit must be between 1 and 250");
        }

        try
        {
            var products = await _shopifyService.GetAllProductsAsync(limit, sinceId, vendor, productType);
            
            _logger.LogInformation("Retrieved {Count} total products", products.Count);
            
            return Ok(new ShopifyProductsResponse { Products = products });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products");
            return StatusCode(500, "Internal server error while retrieving products");
        }
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Product details</returns>
    /// <response code="200">Product retrieved successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("products/{productId}")]
    [ProducesResponseType(typeof(ShopifyProduct), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetProduct(long productId)
    {
        try
        {
            var product = await _shopifyService.GetProductAsync(productId);
            
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", productId);
                return NotFound($"Product with ID {productId} not found");
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", productId);
            return StatusCode(500, "Internal server error while retrieving product");
        }
    }

    /// <summary>
    /// Get all products categorized by tags (AutomationProducts, Products, DogExtraProducts)
    /// Returns products with only id, handle, and tags fields for optimization
    /// </summary>
    /// <returns>Categorized products with counts</returns>
    /// <response code="200">Products categorized successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("products/categorized")]
    [ProducesResponseType(typeof(ShopifyCategorizedProductsResponse), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCategorizedProducts()
    {
        try
        {
            _logger.LogInformation("Getting categorized products");

            var categorizedProducts = await _shopifyService.GetCategorizedProductsAsync();
            
            return Ok(categorizedProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categorized products");
            return StatusCode(500, "Internal server error while retrieving categorized products");
        }
    }

    /// <summary>
    /// Get customer analytics including purchase predictions
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Customer analytics data with next purchase predictions</returns>
    /// <response code="200">Analytics retrieved successfully</response>
    /// <response code="404">Customer not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("customers/{customerId}/analytics")]
    [ProducesResponseType(typeof(CustomerAnalytics), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCustomerAnalytics(long customerId)
    {
        try
        {
            var analytics = await _shopifyService.GetCustomerAnalyticsAsync(customerId);
            
            if (analytics == null)
            {
                _logger.LogWarning("Customer analytics for {CustomerId} not found", customerId);
                return NotFound($"Customer with ID {customerId} not found");
            }

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer analytics for {CustomerId}", customerId);
            return StatusCode(500, "Internal server error while retrieving customer analytics");
        }
    }

    /// <summary>
    /// Get analytics for customers with specific tags
    /// </summary>
    /// <param name="tags">Comma-separated list of tags to filter by</param>
    /// <param name="limit">Maximum number of customers to analyze (default: 50, max: 100)</param>
    /// <returns>List of customer analytics data</returns>
    /// <response code="200">Analytics retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("customers/analytics/by-tags")]
    [ProducesResponseType(typeof(List<CustomerAnalytics>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCustomerAnalyticsByTags(
        [FromQuery] [Required] string tags,
        [FromQuery] int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return BadRequest("Tags parameter is required");
        }

        if (limit <= 0 || limit > 100)
        {
            return BadRequest("Limit must be between 1 and 100");
        }

        try
        {
            var analytics = await _shopifyService.GetCustomerAnalyticsByTagsAsync(tags, limit);
            
            _logger.LogInformation("Retrieved analytics for {Count} customers with tags: {Tags}", analytics.Count, tags);
            
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer analytics with tags: {Tags}", tags);
            return StatusCode(500, "Internal server error while retrieving customer analytics");
        }
    }

    /// <summary>
    /// Calculate next purchase date prediction for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Next purchase date prediction based on order history</returns>
    /// <response code="200">Prediction calculated successfully</response>
    /// <response code="404">Customer not found or insufficient data</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("customers/{customerId}/next-purchase-prediction")]
    [ProducesResponseType(typeof(NextPurchasePredictionResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CalculateNextPurchaseDate(long customerId)
    {
        try
        {
            var orders = await _shopifyService.GetCustomerOrdersAsync(customerId);
            
            if (orders.Count < 2)
            {
                return NotFound("Customer not found or insufficient order history for prediction (minimum 2 orders required)");
            }

            var nextPurchaseDate = _shopifyService.CalculateNextPurchaseDate(orders);
            
            if (!nextPurchaseDate.HasValue)
            {
                return NotFound("Unable to calculate next purchase date with available data");
            }

            var response = new NextPurchasePredictionResponse
            {
                CustomerId = customerId,
                PredictedNextPurchaseDate = nextPurchaseDate.Value,
                TotalOrders = orders.Count,
                LastOrderDate = orders.Max(o => o.CreatedAt),
                DaysSinceLastOrder = (DateTime.Now - orders.Max(o => o.CreatedAt)).Days,
                Confidence = CalculatePredictionConfidence(orders)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next purchase date for customer {CustomerId}", customerId);
            return StatusCode(500, "Internal server error while calculating purchase prediction");
        }
    }

    /// <summary>
    /// Get customers who are likely to purchase soon
    /// </summary>
    /// <param name="tags">Optional tags to filter customers</param>
    /// <param name="daysThreshold">Number of days from now to consider "soon" (default: 7)</param>
    /// <param name="limit">Maximum number of customers to analyze (default: 25, max: 50)</param>
    /// <returns>List of customers likely to purchase soon</returns>
    /// <response code="200">Customers retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("customers/likely-to-purchase-soon")]
    [ProducesResponseType(typeof(List<CustomerAnalytics>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCustomersLikelyToPurchaseSoon(
        [FromQuery] string? tags = null,
        [FromQuery] int daysThreshold = 7,
        [FromQuery] int limit = 25)
    {
        if (daysThreshold <= 0 || daysThreshold > 365)
        {
            return BadRequest("Days threshold must be between 1 and 365");
        }

        if (limit <= 0 || limit > 50)
        {
            return BadRequest("Limit must be between 1 and 50");
        }

        try
        {
            var customers = await _shopifyService.GetCustomersLikelyToPurchaseSoonAsync(tags, daysThreshold, limit);
            
            _logger.LogInformation("Found {Count} customers likely to purchase within {Days} days", 
                customers.Count, daysThreshold);
            
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers likely to purchase soon");
            return StatusCode(500, "Internal server error while retrieving purchase predictions");
        }
    }

    private static string CalculatePredictionConfidence(List<ShopifyOrder> orders)
    {
        if (orders.Count < 3) return "Low";
        if (orders.Count < 5) return "Medium";
        
        // Calculate consistency of purchase intervals
        var sortedOrders = orders.OrderBy(o => o.CreatedAt).ToList();
        var intervals = new List<int>();
        
        for (int i = 1; i < sortedOrders.Count; i++)
        {
            intervals.Add((sortedOrders[i].CreatedAt - sortedOrders[i - 1].CreatedAt).Days);
        }
        
        var average = intervals.Average();
        var variance = intervals.Sum(x => Math.Pow(x - average, 2)) / intervals.Count;
        var standardDeviation = Math.Sqrt(variance);
        
        // Lower standard deviation = more consistent = higher confidence
        var coefficientOfVariation = standardDeviation / average;
        
        if (coefficientOfVariation < 0.3) return "High";
        if (coefficientOfVariation < 0.6) return "Medium";
        return "Low";
    }
}

/// <summary>
/// Response model for next purchase prediction
/// </summary>
public class NextPurchasePredictionResponse
{
    /// <summary>
    /// Customer ID
    /// </summary>
    /// <example>123456789</example>
    public long CustomerId { get; set; }

    /// <summary>
    /// Predicted next purchase date
    /// </summary>
    /// <example>2024-03-15T10:30:00Z</example>
    public DateTime PredictedNextPurchaseDate { get; set; }

    /// <summary>
    /// Total number of orders used for prediction
    /// </summary>
    /// <example>5</example>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Date of customer's last order
    /// </summary>
    /// <example>2024-02-01T14:20:00Z</example>
    public DateTime LastOrderDate { get; set; }

    /// <summary>
    /// Number of days since last order
    /// </summary>
    /// <example>15</example>
    public int DaysSinceLastOrder { get; set; }

    /// <summary>
    /// Confidence level of the prediction (High, Medium, Low)
    /// </summary>
    /// <example>High</example>
    public string Confidence { get; set; } = string.Empty;
}