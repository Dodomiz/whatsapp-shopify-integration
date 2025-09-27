using System.Text.Json;
using Microsoft.Extensions.Options;
using WhatsAppIntegration.Configuration;
using WhatsAppIntegration.Models;

namespace WhatsAppIntegration.Services;

/// <summary>
/// Low-level Shopify API server access implementation
/// Handles HTTP communication with Shopify REST Admin API
/// </summary>
public class ShopifyServerAccess : IShopifyServerAccess
{
    private readonly HttpClient _httpClient;
    private readonly ShopifyConfig _config;
    private readonly ILogger<ShopifyServerAccess> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    // Only fetch the specific customer fields we need for better performance
    private const string CustomerFields = "id,first_name,last_name,phone,orders_count,last_order_id,state,last_order_name,total_spent,tags,created_at,updated_at";

    public ShopifyServerAccess(HttpClient httpClient, IOptions<ShopifyConfig> config, ILogger<ShopifyServerAccess> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
        
        // Configure HttpClient with Shopify credentials
        _httpClient.BaseAddress = new Uri($"{_config.BaseUrl}");
        _httpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", _config.AccessToken);

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
    }

    // HTTP helper methods
    public async Task<(List<ShopifyCustomer> customers, string? nextPageUrl)> FetchCustomersWithPaginationAsync(int batchSize, string? nextPageInfo)
    {
        try
        {
            var url = nextPageInfo != null 
                ? $"/admin/api/{_config.ApiVersion}/customers.json?limit={batchSize}&page_info={nextPageInfo}&fields={CustomerFields}"
                : $"/admin/api/{_config.ApiVersion}/customers.json?limit={batchSize}&fields={CustomerFields}";

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Shopify API request failed. Status: {StatusCode}, Response: {Response}", response.StatusCode, errorContent);
                return (new List<ShopifyCustomer>(), null);
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var customersResponse = JsonSerializer.Deserialize<ShopifyCustomersResponse>(jsonContent, _jsonOptions);
            
            var nextPageUrl = ParseLinkHeader(response.Headers);
            return (customersResponse?.Customers ?? new List<ShopifyCustomer>(), nextPageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching customers batch");
            return (new List<ShopifyCustomer>(), null);
        }
    }

    private string? ParseLinkHeader(System.Net.Http.Headers.HttpResponseHeaders headers)
    {
        if (headers.TryGetValues("Link", out var linkValues))
        {
            var linkHeader = linkValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(linkHeader))
            {
                var nextMatch = System.Text.RegularExpressions.Regex.Match(linkHeader, @"<([^>]+)>;\s*rel=""next""");
                if (nextMatch.Success)
                {
                    var nextUrl = nextMatch.Groups[1].Value;
                    var uri = new Uri(nextUrl);
                    return uri.PathAndQuery;
                }
            }
        }
        return null;
    }

    public async Task<List<ShopifyCustomer>> GetCustomersWithTagsAsync(string tags, int? limit = 250, string? excludeTags = null)
    {
        try
        {
            var allCustomers = new List<ShopifyCustomer>();
            string? nextPageInfo = null;
            const int batchSize = 250;
            var searchTags = tags?.Split(',').Select(t => t.Trim()).ToList() ?? new List<string>();
            var excludeTagsList = string.IsNullOrEmpty(excludeTags) 
                ? new List<string>() 
                : excludeTags.Split(',').Select(t => t.Trim()).ToList();
            
            do
            {
                var (customers, nextPageUrl) = await FetchCustomersWithPaginationAsync(batchSize, nextPageInfo);
                
                if (customers.Count == 0) break;

                // Filter customers by tags
                var filteredCustomers = customers.Where(customer =>
                {
                    var customerTags = customer.TagsList ?? new List<string>();
                    var hasRequiredTags = !searchTags.Any() || searchTags.Any(tag => customerTags.Contains(tag, StringComparer.OrdinalIgnoreCase));
                    var hasExcludedTags = excludeTagsList.Any() && excludeTagsList.Any(tag => customerTags.Contains(tag, StringComparer.OrdinalIgnoreCase));
                    return hasRequiredTags && !hasExcludedTags;
                }).ToList();

                allCustomers.AddRange(filteredCustomers);

                if (string.IsNullOrEmpty(nextPageUrl) || (limit.HasValue && allCustomers.Count >= limit.Value))
                    break;

                nextPageInfo = ExtractPageInfoFromUrl(nextPageUrl);
            } while (true);

            if (limit.HasValue && allCustomers.Count > limit.Value)
                allCustomers = allCustomers.Take(limit.Value).ToList();

            return allCustomers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching customers with tags");
            return new List<ShopifyCustomer>();
        }
    }

    private string? ExtractPageInfoFromUrl(string url)
    {
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        return query["page_info"];
    }

    public async Task<List<ShopifyCustomer>> GetAllCustomersAsync(int? limit = null)
    {
        try
        {
            var allCustomers = new List<ShopifyCustomer>();
            string? nextPageInfo = null;
            const int batchSize = 250;
            
            do
            {
                var (customers, nextPageUrl) = await FetchCustomersWithPaginationAsync(batchSize, nextPageInfo);
                
                if (customers.Count == 0) break;

                allCustomers.AddRange(customers);

                if (string.IsNullOrEmpty(nextPageUrl) || (limit.HasValue && allCustomers.Count >= limit.Value))
                    break;

                nextPageInfo = ExtractPageInfoFromUrl(nextPageUrl);
            } while (true);

            if (limit.HasValue && allCustomers.Count > limit.Value)
                allCustomers = allCustomers.Take(limit.Value).ToList();

            return allCustomers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all customers");
            return new List<ShopifyCustomer>();
        }
    }

    public async Task<ShopifyCustomer?> GetCustomerAsync(long customerId)
    {
        try
        {
            var url = $"/admin/api/{_config.ApiVersion}/customers/{customerId}.json?fields={CustomerFields}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);
                
                if (document.RootElement.TryGetProperty("customer", out var customerElement))
                {
                    return JsonSerializer.Deserialize<ShopifyCustomer>(customerElement.GetRawText(), _jsonOptions);
                }
            }

            _logger.LogWarning("Failed to get customer {CustomerId}. Status: {StatusCode}", customerId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {CustomerId}", customerId);
            return null;
        }
    }

    public async Task<int> GetCustomersCountAsync()
    {
        try
        {
            var url = $"/admin/api/{_config.ApiVersion}/customers/count.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);
                if (document.RootElement.TryGetProperty("count", out var countElement))
                {
                    return countElement.GetInt32();
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers count");
            return 0;
        }
    }

    // Simplified implementations for other methods - can be expanded later
    public async Task<List<ShopifyOrder>> GetCustomerOrdersAsync(long customerId, string status = "any", int limit = 250, long? sinceId = null)
    {
        try
        {
            var url = $"/admin/api/{_config.ApiVersion}/customers/{customerId}/orders.json?status={status}&limit={limit}";
            if (sinceId.HasValue)
                url += $"&since_id={sinceId}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var ordersResponse = JsonSerializer.Deserialize<ShopifyOrdersResponse>(content, _jsonOptions);
                return ordersResponse?.Orders ?? new List<ShopifyOrder>();
            }

            return new List<ShopifyOrder>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer orders for {CustomerId}", customerId);
            return new List<ShopifyOrder>();
        }
    }

    public async Task<List<ShopifyOrder>> GetAllOrdersAsync(string status = "any", int? limit = null, long? sinceId = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null, List<long>? customerIds = null)
    {
        try
        {
            // If specific customer IDs are provided and the list is small, use multiple API calls for efficiency
            if (customerIds != null && customerIds.Any())
            {
                // For small lists (≤ 10 customers), make individual API calls per customer
                if (customerIds.Count <= 10)
                {
                    return await GetOrdersForSpecificCustomersAsync(customerIds, status, limit, sinceId, createdAtMin, createdAtMax);
                }
                // For larger lists, fetch all orders and filter client-side
                _logger.LogInformation("Large customer ID list ({Count} customers). Fetching all orders and filtering client-side.", customerIds.Count);
            }

            var allOrders = new List<ShopifyOrder>();
            const int batchSize = 250; // Maximum allowed by Shopify API
            string? nextPageInfo = null;
            var totalBatches = 0;
            
            _logger.LogInformation("Starting to fetch all orders using page_info pagination with batch size {BatchSize}. " +
                "Filters: status={Status}, sinceId={SinceId}, createdAtMin={CreatedAtMin}, createdAtMax={CreatedAtMax}, customerIds={CustomerIds}", 
                batchSize, status, sinceId, createdAtMin, createdAtMax, customerIds?.Count);
            
            do
            {
                totalBatches++;

                // Use page_info pagination for all requests
                // For the first request, include sinceId if provided; subsequent requests use page_info only
                var currentSinceId = (totalBatches == 1) ? sinceId : null;
                var url = BuildOrdersUrlWithPageInfo(status, batchSize, nextPageInfo, createdAtMin, createdAtMax, currentSinceId);
                
                _logger.LogDebug("Making Shopify API request to: {Url}", url);
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Shopify API request failed. Status: {StatusCode}, URL: {Url}, Response: {Response}", 
                        response.StatusCode, url, errorContent);
                    throw new HttpRequestException($"Shopify API request failed with status {response.StatusCode}: {errorContent}");
                }
                
                var jsonContent = await response.Content.ReadAsStringAsync();
                var ordersResponse = JsonSerializer.Deserialize<ShopifyOrdersResponse>(jsonContent, _jsonOptions);
                var orders = ordersResponse?.Orders ?? new List<ShopifyOrder>();
                
                if (orders.Count == 0)
                {
                    _logger.LogInformation("No more orders to fetch. Total retrieved: {Count} orders in {Batches} batches", 
                        allOrders.Count, totalBatches);
                    break;
                }

                allOrders.AddRange(orders);
                
                _logger.LogDebug("Batch {BatchNumber}: Fetched {BatchCount} orders. Total so far: {Total}", 
                    totalBatches, orders.Count, allOrders.Count);
                
                // Get next page URL from Link header
                var nextPageUrl = ParseLinkHeader(response.Headers);
                if (string.IsNullOrEmpty(nextPageUrl))
                {
                    _logger.LogInformation("No more pages available. Retrieved {Count} total orders in {Batches} batches", 
                        allOrders.Count, totalBatches);
                    break;
                }
                
                // Extract page_info from the next page URL for the next iteration
                nextPageInfo = ExtractPageInfoFromUrl(nextPageUrl);
                
                // Log progress every 10 batches for large datasets
                if (totalBatches % 10 == 0)
                {
                    _logger.LogInformation("Progress: {Count} orders fetched across {Batches} batches", 
                        allOrders.Count, totalBatches);
                }
                
            } while (true); // Continue until no more orders or limit reached

            // Apply client-side filtering if customer IDs were specified (for large lists > 10)
            if (customerIds != null && customerIds.Any() && customerIds.Count > 10)
            {
                var originalCount = allOrders.Count;
                var customerIdSet = customerIds.ToHashSet();
                allOrders = allOrders.Where(order => order.Customer != null && customerIdSet.Contains(order.Customer.Id)).ToList();
                _logger.LogInformation("Client-side filtering: {OriginalCount} orders filtered to {FilteredCount} orders for {CustomerCount} customers", 
                    originalCount, allOrders.Count, customerIds.Count);
            }

            _logger.LogInformation("Successfully retrieved {Count} total orders in {Batches} batches", 
                allOrders.Count, totalBatches);
            return allOrders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all orders");
            throw;
        }
    }

    public Task<List<ShopifyOrder>> GetOrdersFromLastDaysAsync(int days = 365, string status = "any", int? limit = null)
    {
        var createdAtMin = DateTime.UtcNow.AddDays(-days);
        return GetAllOrdersAsync(status, limit, null, createdAtMin, null);
    }

    public Task<Dictionary<long, List<ShopifyOrder>>> GetOrdersByCustomerAsync(string status = "any", int? limit = null, int? minOrdersPerCustomer = null, List<long>? targetProductIds = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null)
    {
        throw new NotImplementedException("Complex grouping logic remains in ShopifyService for now");
    }

    public async Task<List<ShopifyProduct>> GetAllProductsAsync(int limit = 250, long? sinceId = null, string? vendor = null, string? productType = null)
    {
        try
        {
            var url = $"/admin/api/{_config.ApiVersion}/products.json?limit={limit}";
            if (sinceId.HasValue) url += $"&since_id={sinceId}";
            if (!string.IsNullOrEmpty(vendor)) url += $"&vendor={Uri.EscapeDataString(vendor)}";
            if (!string.IsNullOrEmpty(productType)) url += $"&product_type={Uri.EscapeDataString(productType)}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var productsResponse = JsonSerializer.Deserialize<ShopifyProductsResponse>(content, _jsonOptions);
                return productsResponse?.Products ?? new List<ShopifyProduct>();
            }

            return new List<ShopifyProduct>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return new List<ShopifyProduct>();
        }
    }

    public async Task<ShopifyProduct?> GetProductAsync(long productId)
    {
        try
        {
            var url = $"/admin/api/{_config.ApiVersion}/products/{productId}.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);
                
                if (document.RootElement.TryGetProperty("product", out var productElement))
                {
                    return JsonSerializer.Deserialize<ShopifyProduct>(productElement.GetRawText(), _jsonOptions);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", productId);
            return null;
        }
    }

    public async Task<int> GetProductsCountAsync()
    {
        try
        {
            var url = $"/admin/api/{_config.ApiVersion}/products/count.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);
                if (document.RootElement.TryGetProperty("count", out var countElement))
                {
                    return countElement.GetInt32();
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products count");
            return 0;
        }
    }

    public async Task<List<ShopifyProduct>> GetProductsWithTagsAsync(string tags, int limit = 250)
    {
        try
        {
            var allProducts = await GetAllProductsAsync(limit);
            var searchTags = tags.Split(',').Select(t => t.Trim()).ToList();
            
            return allProducts.Where(product =>
            {
                var productTags = product.TagsList ?? new List<string>();
                return searchTags.Any(tag => productTags.Contains(tag, StringComparer.OrdinalIgnoreCase));
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products with tags");
            return new List<ShopifyProduct>();
        }
    }

    // Helper methods for GetAllOrdersAsync
    private string BuildOrdersUrlWithPageInfo(string status, int limit, string? pageInfo, DateTime? createdAtMin, DateTime? createdAtMax, long? sinceId = null)
    {
        // According to Shopify docs, when using page_info, we can only include limit parameter
        // All other filters must be applied in the initial request only
        if (!string.IsNullOrEmpty(pageInfo))
        {
            return $"/admin/api/{_config.ApiVersion}/orders.json?limit={limit}&page_info={pageInfo}";
        }
        
        // For the first request without page_info, include all parameters
        var url = $"/admin/api/{_config.ApiVersion}/orders.json?status={status}&limit={limit}";
        if (sinceId.HasValue)
            url += $"&since_id={sinceId.Value}";
        if (createdAtMin.HasValue)
            url += $"&created_at_min={createdAtMin.Value:yyyy-MM-ddTHH:mm:ssZ}";
        if (createdAtMax.HasValue)
            url += $"&created_at_max={createdAtMax.Value:yyyy-MM-ddTHH:mm:ssZ}";
        return url;
    }

    private async Task<List<ShopifyOrder>> GetOrdersForSpecificCustomersAsync(List<long> customerIds, string status, int? limit, long? sinceId, DateTime? createdAtMin, DateTime? createdAtMax)
    {
        var allOrders = new List<ShopifyOrder>();
        var tasks = new List<Task<List<ShopifyOrder>>>();

        _logger.LogInformation("Fetching orders for {CustomerCount} specific customers using individual API calls", customerIds.Count);

        // Create tasks for each customer to fetch orders in parallel
        foreach (var customerId in customerIds)
        {
            tasks.Add(GetCustomerOrdersWithFiltersAsync(customerId, status, limit, sinceId, createdAtMin, createdAtMax));
        }

        // Wait for all customer order fetches to complete
        var customerOrderResults = await Task.WhenAll(tasks);
        
        // Combine all orders
        foreach (var customerOrders in customerOrderResults)
        {
            allOrders.AddRange(customerOrders);
        }

        _logger.LogInformation("Successfully fetched {TotalOrders} orders from {CustomerCount} customers", allOrders.Count, customerIds.Count);
        return allOrders;
    }

    public async Task<List<ShopifyOrder>> GetCustomerOrdersWithFiltersAsync(long customerId, string status, int? limit, long? sinceId, DateTime? createdAtMin, DateTime? createdAtMax)
    {
        try
        {
            var url = BuildCustomerOrdersUrl(customerId, status, limit, sinceId, createdAtMin, createdAtMax);
            
            _logger.LogDebug("Fetching orders for customer {CustomerId} from: {Url}", customerId, url);
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to fetch orders for customer {CustomerId}. Status: {StatusCode}, Response: {Response}", 
                    customerId, response.StatusCode, errorContent);
                return new List<ShopifyOrder>();
            }
            
            var jsonContent = await response.Content.ReadAsStringAsync();
            var ordersResponse = JsonSerializer.Deserialize<ShopifyOrdersResponse>(jsonContent, _jsonOptions);
            var orders = ordersResponse?.Orders ?? new List<ShopifyOrder>();
            
            _logger.LogDebug("Retrieved {OrderCount} orders for customer {CustomerId}", orders.Count, customerId);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders for customer {CustomerId}", customerId);
            return new List<ShopifyOrder>();
        }
    }

    /// <summary>
    /// Builds URL for fetching customer orders with optional filters
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="status">Order status filter</param>
    /// <param name="limit">Maximum number of orders</param>
    /// <param name="sinceId">Retrieve orders created after this ID</param>
    /// <param name="createdAtMin">Show orders created at or after date</param>
    /// <param name="createdAtMax">Show orders created at or before date</param>
    /// <returns>Formatted URL for the API request</returns>
    private string BuildCustomerOrdersUrl(long customerId, string status, int? limit, long? sinceId, DateTime? createdAtMin, DateTime? createdAtMax)
    {
        var queryParams = new List<string>
        {
            $"customer_id={customerId}",
            $"status={status}"
        };

        if (limit.HasValue)
            queryParams.Add($"limit={limit.Value}");

        if (sinceId.HasValue)
            queryParams.Add($"since_id={sinceId.Value}");

        if (createdAtMin.HasValue)
            queryParams.Add($"created_at_min={createdAtMin.Value:yyyy-MM-ddTHH:mm:ssZ}");

        if (createdAtMax.HasValue)
            queryParams.Add($"created_at_max={createdAtMax.Value:yyyy-MM-ddTHH:mm:ssZ}");

        var queryString = string.Join("&", queryParams);
        return $"/admin/api/{_config.ApiVersion}/orders.json?{queryString}";
    }

    public async Task<List<ShopifyProduct>> GetAllProductsWithFieldsAsync(string fields, int limit = 250, long? sinceId = null)
    {
        try
        {
            var allProducts = new List<ShopifyProduct>();
            long? currentSinceId = sinceId;
            
            do
            {
                var url = BuildProductsUrlWithFields(limit, currentSinceId, fields);
                
                _logger.LogDebug("Fetching products with fields from: {Url}", url);
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch products with fields. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                    break;
                }
                
                var jsonContent = await response.Content.ReadAsStringAsync();
                var productsResponse = JsonSerializer.Deserialize<ShopifyProductsResponse>(jsonContent, _jsonOptions);
                var products = productsResponse?.Products ?? new List<ShopifyProduct>();
                
                if (products.Count == 0)
                    break;

                allProducts.AddRange(products);
                
                if (products.Count < limit)
                    break;
                    
                currentSinceId = products.Last().Id;
                
            } while (allProducts.Count < 10000); // Safety limit

            _logger.LogInformation("Retrieved {Count} total products with fields: {Fields}", allProducts.Count, fields);
            return allProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products with fields: {Fields}", fields);
            return new List<ShopifyProduct>();
        }
    }

    private string BuildProductsUrlWithFields(int limit, long? sinceId, string fields)
    {
        var url = $"/admin/api/{_config.ApiVersion}/products.json?limit={limit}&fields={fields}";
        if (sinceId.HasValue)
            url += $"&since_id={sinceId.Value}";
        return url;
    }



    private string BuildCustomersUrlWithPageInfo(int limit, string? pageInfo)
    {
        // Only fetch the specific fields we need for better performance
        const string CustomerFields = "id,first_name,last_name,phone,orders_count,last_order_id,state,last_order_name,total_spent,tags,created_at,updated_at";
        var url = $"/admin/api/{_config.ApiVersion}/customers.json?limit={limit}&fields={CustomerFields}";
        if (!string.IsNullOrEmpty(pageInfo))
            url += $"&page_info={pageInfo}";
        return url;
    }

    private string BuildCustomerOrdersUrlWithFilters(long customerId, string status, int? limit, long? sinceId, DateTime? createdAtMin, DateTime? createdAtMax)
    {
        var queryParams = new List<string>
        {
            $"customer_id={customerId}",
            $"status={status}"
        };

        if (limit.HasValue)
            queryParams.Add($"limit={limit.Value}");

        if (sinceId.HasValue)
            queryParams.Add($"since_id={sinceId.Value}");

        if (createdAtMin.HasValue)
            queryParams.Add($"created_at_min={createdAtMin.Value:yyyy-MM-ddTHH:mm:ssZ}");

        if (createdAtMax.HasValue)
            queryParams.Add($"created_at_max={createdAtMax.Value:yyyy-MM-ddTHH:mm:ssZ}");

        var queryString = string.Join("&", queryParams);
        return $"/admin/api/{_config.ApiVersion}/orders.json?{queryString}";
    }

}