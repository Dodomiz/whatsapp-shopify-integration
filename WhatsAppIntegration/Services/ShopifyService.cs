using System.Text.Json;
using Microsoft.Extensions.Options;
using WhatsAppIntegration.Constants;
using WhatsAppIntegration.Models;
using WhatsAppIntegration.Repositories;

namespace WhatsAppIntegration.Services;

public class ShopifyService : IShopifyService
{
    private readonly HttpClient _httpClient;
    private readonly ShopifyConfig _config;
    private readonly ILogger<ShopifyService> _logger;
    private readonly ICategorizedOrdersRepository _categorizedOrdersRepository;
    private readonly JsonSerializerOptions _jsonOptions;
    
    // Only fetch the specific customer fields we need for better performance
    private const string CustomerFields = "id,first_name,last_name,phone,orders_count,last_order_id,state,last_order_name,total_spent,tags,created_at,updated_at";

    public ShopifyService(HttpClient httpClient, IOptions<ShopifyConfig> config, ILogger<ShopifyService> logger, ICategorizedOrdersRepository categorizedOrdersRepository)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
        _categorizedOrdersRepository = categorizedOrdersRepository;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        _httpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", _config.AccessToken);
    }

    public async Task<List<ShopifyCustomer>> GetCustomersWithTagsAsync(string? tags, int? limit, string? excludeTags = null)
    {
        try
        {
            var allCustomers = new List<ShopifyCustomer>();
            string? nextPageInfo = null;
            const int batchSize = 250; // Maximum allowed by Shopify API
            var searchTags = tags?.Split(',').Select(t => t.Trim()).ToList() ?? new List<string>();
            var excludeTagsList = string.IsNullOrEmpty(excludeTags) 
                ? new List<string>() 
                : excludeTags.Split(',').Select(t => t.Trim()).ToList();
            
            _logger.LogInformation("Starting to fetch customers with tags '{Tags}' excluding tags '{ExcludeTags}' in batches of {BatchSize} using Link header pagination", 
                tags ?? "none", excludeTags ?? "none", batchSize);
            
            do
            {
                var (customers, nextPageUrl) = await FetchCustomersWithPaginationAsync(batchSize, nextPageInfo);
                
                if (customers.Count == 0)
                {
                    _logger.LogInformation("No more customers to fetch. Total with matching tags: {Count}", allCustomers.Count);
                    break;
                }

                var filteredCustomers = customers.Where(c => 
                    (searchTags.Count == 0 || ContainsAnyTag(c.TagsList, searchTags)) && (excludeTagsList.Count == 0 || 
                        !ContainsAnyTag(c.TagsList, excludeTagsList))).ToList();
                
                allCustomers.AddRange(filteredCustomers);
                
                _logger.LogDebug("Fetched batch of {BatchCount} customers, {FilteredCount} matched tags. Total matching so far: {Total}", 
                    customers.Count, filteredCustomers.Count, allCustomers.Count);
                
                // If we've reached the requested limit, stop
                if (limit != null && allCustomers.Count >= limit)
                {
                    allCustomers = allCustomers.Take((int)limit).ToList();
                    _logger.LogInformation("Reached specified limit of {Limit} customers", limit);
                    break;
                }
                
                // Check if there's a next page
                if (string.IsNullOrEmpty(nextPageUrl))
                {
                    _logger.LogInformation("No more pages available. Total with matching tags: {Count}", allCustomers.Count);
                    break;
                }
                
                // Extract page_info from the next page URL for the next iteration
                nextPageInfo = ExtractPageInfoFromUrl(nextPageUrl);
                
            } while (true); // Continue until no more customers or limit reached

            _logger.LogInformation("Retrieved {Count} customers with tags: {Tags}", allCustomers.Count, tags);
            return allCustomers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching customers with tags: {Tags}", tags);
            return new List<ShopifyCustomer>();
        }
    }

    public async Task<List<ShopifyCustomer>> GetAllCustomersAsync(int? limit = null)
    {
        try
        {
            var allCustomers = new List<ShopifyCustomer>();
            string? nextPageInfo = null;
            const int batchSize = 250; // Maximum allowed by Shopify API
            
            _logger.LogInformation("Starting to fetch all customers in batches of {BatchSize} using Link header pagination", batchSize);
            
            do
            {
                var (customers, nextPageUrl) = await FetchCustomersWithPaginationAsync(batchSize, nextPageInfo);
                
                if (customers.Count == 0)
                {
                    _logger.LogInformation("No more customers to fetch. Total retrieved: {Count}", allCustomers.Count);
                    break;
                }

                allCustomers.AddRange(customers);
                
                _logger.LogDebug("Fetched batch of {BatchCount} customers. Total so far: {Total}", 
                    customers.Count, allCustomers.Count);
                
                // If a limit was specified, check if we've reached it
                if (limit.HasValue && allCustomers.Count >= limit.Value)
                {
                    allCustomers = allCustomers.Take(limit.Value).ToList();
                    _logger.LogInformation("Reached specified limit of {Limit} customers", limit.Value);
                    break;
                }
                
                // Check if there's a next page
                if (string.IsNullOrEmpty(nextPageUrl))
                {
                    _logger.LogInformation("No more pages available. Total retrieved: {Count}", allCustomers.Count);
                    break;
                }
                
                // Extract page_info from the next page URL for the next iteration
                nextPageInfo = ExtractPageInfoFromUrl(nextPageUrl);
                
            } while (true); // Continue until no more customers

            _logger.LogInformation("Successfully retrieved {Count} total customers", allCustomers.Count);
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
            var url = $"{_config.BaseUrl}/admin/api/{_config.ApiVersion}/customers/{customerId}.json?fields={CustomerFields}";
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
            var url = $"{_config.BaseUrl}/admin/api/{_config.ApiVersion}/customers/count.json";
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

            _logger.LogWarning("Failed to get customers count. Status: {StatusCode}", response.StatusCode);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers count");
            return 0;
        }
    }

    public async Task<List<ShopifyOrder>> GetCustomerOrdersAsync(long customerId, string status = "any", int limit = 250, long? sinceId = null)
    {
        try
        {
            var allOrders = new List<ShopifyOrder>();
            var currentSinceId = sinceId;
            
            do
            {
                var url = BuildCustomerOrdersUrl(customerId, status, limit, currentSinceId);
                var orders = await FetchOrdersAsync(url);
                
                if (orders.Count == 0)
                    break;

                allOrders.AddRange(orders);
                
                if (orders.Count < limit)
                    break;
                    
                currentSinceId = orders.Last().Id;
                
            } while (allOrders.Count < 1000); // Safety limit

            _logger.LogInformation("Retrieved {Count} orders for customer {CustomerId}", allOrders.Count, customerId);
            return allOrders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders for customer {CustomerId}", customerId);
            return new List<ShopifyOrder>();
        }
    }

    public async Task<List<ShopifyOrder>> GetAllOrdersAsync(string status = "any", int? limit = null, long? sinceId = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null)
    {
        try
        {
            // If no date filters are provided, default to the last year to avoid response size limits
            // if (!createdAtMin.HasValue && !createdAtMax.HasValue)
            // {
            //     createdAtMin = DateTime.UtcNow.AddDays(-90);
            //     _logger.LogInformation("No date filters provided. Defaulting to fetch orders from the last 90 days (since {Date})", 
            //         createdAtMin.Value.ToString("yyyy-MM-dd"));
            // }
            //
            var allOrders = new List<ShopifyOrder>();
            const int batchSize = 250; // Maximum allowed by Shopify API
            string? nextPageInfo = null;
            var totalBatches = 0;
            
            _logger.LogInformation("Starting to fetch all orders using page_info pagination with batch size {BatchSize}. " +
                "Filters: status={Status}, sinceId={SinceId}, createdAtMin={CreatedAtMin}, createdAtMax={CreatedAtMax}", 
                batchSize, status, sinceId, createdAtMin, createdAtMax);
            
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
                
                // If a limit was specified, check if we've reached it
                // if (limit.HasValue && allOrders.Count >= limit.Value)
                // {
                //     allOrders = allOrders.Take(limit.Value).ToList();
                //     _logger.LogInformation("Reached specified limit of {Limit} orders after {Batches} batches", 
                //         limit.Value, totalBatches);
                //     break;
                // }
                
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

            _logger.LogInformation("Successfully retrieved {Count} total orders in {Batches} batches", 
                allOrders.Count, totalBatches);
            return allOrders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all orders");
            return new List<ShopifyOrder>();
        }
    }

    public async Task<List<ShopifyOrder>> GetOrdersFromLastDaysAsync(int days = 365, string status = "any", int? limit = null)
    {
        var createdAtMin = DateTime.UtcNow.AddDays(-days);
        _logger.LogInformation("Fetching orders from the last {Days} days (since {Date})", days, createdAtMin.ToString("yyyy-MM-dd"));
        
        return await GetAllOrdersAsync(status, limit, null, createdAtMin);
    }

    public async Task<Dictionary<long, List<ShopifyOrder>>> GetOrdersByCustomerAsync(string status = "any", int? limit = null, int? minOrdersPerCustomer = null, List<long>? productIds = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null)
    {
        try
        {
            var tempProductIds = productIds ?? new List<long>();
            var productIdsString = tempProductIds.Count > 0 ? string.Join(", ", tempProductIds) : "none";
            _logger.LogInformation("Starting to fetch orders grouped by customer with filters: status={Status}, limit={Limit}, minOrders={MinOrders}, productIds={ProductIds}, createdAtMin={CreatedAtMin}, createdAtMax={CreatedAtMax}", 
                status, limit, minOrdersPerCustomer, productIdsString, createdAtMin, createdAtMax);

            // First, get all orders (will default to last 365 days if no date filters provided)
            var allOrders = await GetAllOrdersAsync(status, limit, null, createdAtMin, createdAtMax);
            
            // Convert product IDs to HashSet for efficient lookup
            var targetProductIds = new HashSet<long>();
            if (tempProductIds.Count > 0)
            {
                targetProductIds = tempProductIds.ToHashSet();
                _logger.LogDebug("Filtering for orders containing products: {ProductIds}", string.Join(", ", targetProductIds));
            }

            // Group orders by customer ID
            var ordersByCustomer = new Dictionary<long, List<ShopifyOrder>>();
            
            foreach (var order in allOrders)
            {
                // Skip orders without customer ID
                if (order.Customer?.Id == null) continue;
                
                var customerId = order.Customer.Id;
                
                // If product filtering is enabled, check if order contains any target products
                if (targetProductIds.Count > 0)
                {
                    var orderHasTargetProduct = order.LineItems.Any(item => 
                        item.ProductId.HasValue && targetProductIds.Contains(item.ProductId.Value));
                    
                    if (!orderHasTargetProduct)
                    {
                        _logger.LogDebug("Order {OrderId} does not contain target products, skipping", order.Id);
                        continue;
                    }
                }
                
                if (!ordersByCustomer.ContainsKey(customerId))
                {
                    ordersByCustomer[customerId] = new List<ShopifyOrder>();
                }
                
                ordersByCustomer[customerId].Add(order);
            }

            // Apply minimum orders per customer filter
            if (minOrdersPerCustomer is > 0)
            {
                var originalCount = ordersByCustomer.Count;
                ordersByCustomer = ordersByCustomer
                    .Where(kvp => kvp.Value.Count >= minOrdersPerCustomer.Value)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                
                _logger.LogInformation("Filtered customers with minimum {MinOrders} orders: {Before} -> {After}", 
                    minOrdersPerCustomer.Value, originalCount, ordersByCustomer.Count);
            }

            // Sort orders within each customer group by creation date (newest first)
            foreach (var customerOrders in ordersByCustomer.Values)
            {
                customerOrders.Sort((o1, o2) => o2.CreatedAt.CompareTo(o1.CreatedAt));
            }

            _logger.LogInformation("Successfully grouped {OrderCount} orders into {CustomerCount} customers", 
                ordersByCustomer.Values.Sum(orders => orders.Count), ordersByCustomer.Count);

            return ordersByCustomer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders grouped by customer");
            return new Dictionary<long, List<ShopifyOrder>>();
        }
    }

    public async Task<ShopifyCategorizedOrdersByCustomerResponse> GetCategorizedOrdersByCustomerAsync(string status = "any", int? limit = null, int? minOrdersPerCustomer = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null)
    {
        try
        {
            _logger.LogInformation("Starting to fetch orders categorized by automation and dog extra products with filters: status={Status}, limit={Limit}, minOrders={MinOrders}, createdAtMin={CreatedAtMin}, createdAtMax={CreatedAtMax}", 
                status, limit, minOrdersPerCustomer, createdAtMin, createdAtMax);

            // First, get categorized products to know which product IDs belong to which category
            var categorizedProducts = await GetCategorizedProductsAsync();
            var automationProductIds = categorizedProducts.AutomationProducts.Select(p => p.Id).ToHashSet();
            var dogExtraProductIds = categorizedProducts.DogExtraProducts.Select(p => p.Id).ToHashSet();
            var allTargetProductIds = automationProductIds.Union(dogExtraProductIds).ToList();

            _logger.LogDebug("Found {AutomationCount} automation products and {DogExtraCount} dog extra products", 
                automationProductIds.Count, dogExtraProductIds.Count);

            // Get all orders containing automation or dog extra products
            var allOrders = await GetOrdersByCustomerAsync(status, limit, minOrdersPerCustomer, allTargetProductIds, createdAtMin, createdAtMax);
            
            // Create product tags lookup for faster access, handling potential duplicates
            var productTagsLookup = new Dictionary<long, List<string>>();
            
            // Add automation products
            foreach (var product in categorizedProducts.AutomationProducts)
            {
                productTagsLookup[product.Id] = product.TagsList;
            }
            
            // Add dog extra products, merging tags if product already exists
            foreach (var product in categorizedProducts.DogExtraProducts)
            {
                if (productTagsLookup.ContainsKey(product.Id))
                {
                    // Merge tags if product exists in both categories
                    var existingTags = productTagsLookup[product.Id];
                    var mergedTags = existingTags.Union(product.TagsList).Distinct().ToList();
                    productTagsLookup[product.Id] = mergedTags;
                }
                else
                {
                    productTagsLookup[product.Id] = product.TagsList;
                }
            }
            
            var response = new ShopifyCategorizedOrdersByCustomerResponse();
            
            // Categorize orders for each customer
            foreach (var (customerId, orders) in allOrders)
            {
                // Get the first order to extract customer information
                var customerInfo = orders.FirstOrDefault()?.Customer;
                if (customerInfo == null) continue;
                
                var categorizedOrders = new CustomerCategorizedOrders
                {
                    Customer = customerInfo
                };
                
                foreach (var order in orders)
                {
                    var orderProductIds = order.LineItems
                        .Where(item => item.ProductId.HasValue)
                        .Select(item => item.ProductId!.Value)
                        .ToHashSet();
                    
                    // Create order copy without customer information and add product tags
                    var orderWithoutCustomer = CreateOrderWithoutCustomer(order, productTagsLookup);
                    
                    // Check if order contains automation products
                    if (orderProductIds.Any(id => automationProductIds.Contains(id)))
                    {
                        categorizedOrders.AutomationProductsOrders.Add(orderWithoutCustomer);
                    }
                    
                    // Check if order contains dog extra products
                    if (orderProductIds.Any(id => dogExtraProductIds.Contains(id)))
                    {
                        categorizedOrders.DogExtraProductsOrders.Add(orderWithoutCustomer);
                    }
                }
                
                // Apply minOrdersPerCustomer filter to each category
                if (minOrdersPerCustomer is > 0)
                {
                    if (categorizedOrders.AutomationProductsOrders.Count < minOrdersPerCustomer.Value)
                    {
                        categorizedOrders.AutomationProductsOrders.Clear();
                    }
                    
                    if (categorizedOrders.DogExtraProductsOrders.Count < minOrdersPerCustomer.Value)
                    {
                        categorizedOrders.DogExtraProductsOrders.Clear();
                    }
                }
                
                // Only include customers that have orders in at least one category after filtering
                if (categorizedOrders.TotalOrders > 0)
                {
                    response.OrdersByCustomer[customerId] = categorizedOrders;
                    
                    // Calculate next purchase predictions for each category
                    NextPurchasePrediction? automationPrediction = null;
                    NextPurchasePrediction? dogExtraPrediction = null;
                    
                    if (categorizedOrders.AutomationProductsOrders.Count > 0)
                    {
                        automationPrediction = CalculateNextPurchasePrediction(
                            categorizedOrders.AutomationProductsOrders, 
                            automationProductIds,
                            productTagsLookup,
                            "automation"
                        );
                    }
                    
                    if (categorizedOrders.DogExtraProductsOrders.Count > 0)
                    {
                        dogExtraPrediction = CalculateNextPurchasePrediction(
                            categorizedOrders.DogExtraProductsOrders,
                            dogExtraProductIds,
                            productTagsLookup,
                            "dogExtra"
                        );
                    }
                    
                    // Save each customer's categorized orders to MongoDB
                    try
                    {
                        var document = new CategorizedOrdersDocument
                        {
                            CustomerId = customerId,
                            Customer = customerInfo,
                            AutomationProductsOrders = categorizedOrders.AutomationProductsOrders,
                            DogExtraProductsOrders = categorizedOrders.DogExtraProductsOrders,
                            AutomationNextPurchase = automationPrediction,
                            DogExtraNextPurchase = dogExtraPrediction,
                            Filters = new OrderFilters
                            {
                                Status = status,
                                Limit = limit,
                                MinOrdersPerCustomer = minOrdersPerCustomer,
                                CreatedAtMin = createdAtMin,
                                CreatedAtMax = createdAtMax
                            }
                        };
                        
                        await _categorizedOrdersRepository.SaveCategorizedOrdersAsync(document);
                        _logger.LogDebug("Saved categorized orders with next purchase predictions to MongoDB for customer {CustomerId}", customerId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save categorized orders to MongoDB for customer {CustomerId}", customerId);
                        // Continue processing other customers even if MongoDB save fails
                    }
                }
            }

            _logger.LogInformation("Successfully categorized orders for {CustomerCount} customers: {AutomationOrders} automation orders, {DogExtraOrders} dog extra orders", 
                response.TotalCustomers, response.TotalAutomationOrders, response.TotalDogExtraOrders);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categorized orders grouped by customer");
            return new ShopifyCategorizedOrdersByCustomerResponse();
        }
    }

    public async Task<List<ShopifyProduct>> GetAllProductsAsync(int limit = 250, long? sinceId = null, string? vendor = null, string? productType = null)
    {
        try
        {
            var allProducts = new List<ShopifyProduct>();
            var currentSinceId = sinceId;
            
            do
            {
                var url = BuildProductsUrl(limit, currentSinceId, vendor, productType);
                var products = await FetchProductsAsync(url);
                
                if (products.Count == 0)
                    break;

                allProducts.AddRange(products);
                
                if (products.Count < limit)
                    break;
                    
                currentSinceId = products.Last().Id;
                
            } while (allProducts.Count < 2000); // Safety limit

            _logger.LogInformation("Retrieved {Count} total products", allProducts.Count);
            return allProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all products");
            return new List<ShopifyProduct>();
        }
    }

    public async Task<ShopifyProduct?> GetProductAsync(long productId)
    {
        try
        {
            var url = $"{_config.BaseUrl}/admin/api/{_config.ApiVersion}/products/{productId}.json";
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

            _logger.LogWarning("Failed to get product {ProductId}. Status: {StatusCode}", productId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", productId);
            return null;
        }
    }

    public async Task<ShopifyCategorizedProductsResponse> GetCategorizedProductsAsync()
    {
        try
        {
            _logger.LogInformation("Starting to fetch and categorize all products with specific fields");
            
            // Fetch all products with only the fields we need: id, handle, tags
            var allProducts = await GetAllProductsWithFieldsAsync("id,handle,tags");
            
            var response = new ShopifyCategorizedProductsResponse
            {
                TotalProductsCount = allProducts.Count,
                Products = allProducts.ToList() // All products
            };
            
            // Categorize products based on tags
            foreach (var product in allProducts)
            {
                var tags = product.Tags.ToLowerInvariant();
                var tagList = tags.Split(',').Select(t => t.Trim()).ToList();
                
                // AutomationProducts: contain "includeautomation" and NOT "dogextra1"
                if (tagList.Contains(ShopifyConstants.ProductTags.IncludeAutomation.ToLowerInvariant()) && !tagList.Contains(ShopifyConstants.ProductTags.DogExtra1.ToLowerInvariant()))
                {
                    response.AutomationProducts.Add(product);
                }
                
                // DogExtraProducts: contain "dogextra1"
                if (tagList.Contains(ShopifyConstants.ProductTags.DogExtra1.ToLowerInvariant()))
                {
                    response.DogExtraProducts.Add(product);
                }
            }
            
            _logger.LogInformation("Successfully categorized {TotalCount} products: {AutomationCount} automation, {DogExtraCount} dog extra",
                response.TotalProductsCount, response.AutomationProducts.Count, response.DogExtraProducts.Count);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching and categorizing products");
            return new ShopifyCategorizedProductsResponse();
        }
    }

    private async Task<List<ShopifyProduct>> GetAllProductsWithFieldsAsync(string fields)
    {
        try
        {
            var allProducts = new List<ShopifyProduct>();
            long? currentSinceId = null;
            const int limit = 250;
            
            do
            {
                var url = BuildProductsUrlWithFields(limit, currentSinceId, fields);
                var products = await FetchProductsAsync(url);
                
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

    public async Task<CustomerAnalytics?> GetCustomerAnalyticsAsync(long customerId)
    {
        try
        {
            var customer = await GetCustomerAsync(customerId);
            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found", customerId);
                return null;
            }

            var orders = await GetCustomerOrdersAsync(customerId);
            
            return CalculateCustomerAnalytics(customer, orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating analytics for customer {CustomerId}", customerId);
            return null;
        }
    }

    public async Task<List<CustomerAnalytics>> GetCustomerAnalyticsByTagsAsync(string tags, int limit = 100)
    {
        try
        {
            var customers = await GetCustomersWithTagsAsync(tags, limit);
            var analytics = new List<CustomerAnalytics>();

            var tasks = customers.Select(async customer =>
            {
                var orders = await GetCustomerOrdersAsync(customer.Id);
                return CalculateCustomerAnalytics(customer, orders);
            });

            var results = await Task.WhenAll(tasks);
            analytics.AddRange(results);

            _logger.LogInformation("Calculated analytics for {Count} customers with tags: {Tags}", analytics.Count, tags);
            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating analytics for customers with tags: {Tags}", tags);
            return new List<CustomerAnalytics>();
        }
    }

    public DateTime? CalculateNextPurchaseDate(List<ShopifyOrder> orders)
    {
        if (orders.Count < 2)
            return null;

        var sortedOrders = orders.OrderBy(o => o.CreatedAt).ToList();
        var intervals = new List<int>();

        for (var i = 1; i < sortedOrders.Count; i++)
        {
            var daysBetween = (sortedOrders[i].CreatedAt - sortedOrders[i - 1].CreatedAt).Days;
            intervals.Add(daysBetween);
        }

        if (intervals.Count == 0)
            return null;

        // Calculate average days between orders
        var averageInterval = intervals.Average();
        var lastOrderDate = sortedOrders.Last().CreatedAt;

        // Add some variability based on customer behavior
        var standardDeviation = CalculateStandardDeviation(intervals);
        var adjustedInterval = averageInterval + (standardDeviation * 0.5); // Add some buffer

        return lastOrderDate.AddDays(adjustedInterval);
    }

    public async Task<List<CustomerAnalytics>> GetCustomersLikelyToPurchaseSoonAsync(string? tags = null, int daysThreshold = 7, int limit = 50)
    {
        try
        {
            List<ShopifyCustomer> customers;
            
            if (!string.IsNullOrEmpty(tags))
            {
                customers = await GetCustomersWithTagsAsync(tags, limit * 2); // Get more to filter
            }
            else
            {
                customers = await GetAllCustomersAsync(limit * 2);
            }
            
            var tasks = customers.Take(limit).Select(async customer =>
            {
                var orders = await GetCustomerOrdersAsync(customer.Id);
                return CalculateCustomerAnalytics(customer, orders);
            });

            var results = await Task.WhenAll(tasks);
            
            // Filter customers likely to purchase within the threshold
            var likelyToPurchase = results
                .Where(a => a.PredictedNextPurchaseDate.HasValue && 
                           a.PredictedNextPurchaseDate.Value <= DateTime.Now.AddDays(daysThreshold) &&
                           a.TotalOrders > 1) // Only customers with purchase history
                .OrderBy(a => a.PredictedNextPurchaseDate)
                .ToList();

            _logger.LogInformation("Found {Count} customers likely to purchase within {Days} days", 
                likelyToPurchase.Count, daysThreshold);
            
            return likelyToPurchase;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers likely to purchase soon");
            return new List<CustomerAnalytics>();
        }
    }

    private CustomerAnalytics CalculateCustomerAnalytics(ShopifyCustomer customer, List<ShopifyOrder> orders)
    {
        var analytics = new CustomerAnalytics
        {
            CustomerId = customer.Id,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Phone = customer.Phone,
            Tags = customer.TagsList,
            TotalOrders = orders.Count,
            RecentOrders = orders.OrderByDescending(o => o.CreatedAt).Take(5).ToList()
        };

        if (orders.Any())
        {
            analytics.TotalSpent = orders.Sum(o => decimal.TryParse(o.TotalPrice, out var price) ? price : 0);
            analytics.LastOrderDate = orders.Max(o => o.CreatedAt);
            analytics.DaysSinceLastOrder = (DateTime.Now - analytics.LastOrderDate.Value).Days;
            analytics.AverageOrderValue = analytics.TotalOrders > 0 ? analytics.TotalSpent / analytics.TotalOrders : 0;
            
            // Calculate average days between orders
            if (orders.Count > 1)
            {
                var sortedOrders = orders.OrderBy(o => o.CreatedAt).ToList();
                var intervals = new List<int>();
                
                for (var i = 1; i < sortedOrders.Count; i++)
                {
                    intervals.Add((sortedOrders[i].CreatedAt - sortedOrders[i - 1].CreatedAt).Days);
                }
                
                analytics.AverageDaysBetweenOrders = (int)intervals.Average();
            }

            // Calculate next purchase prediction
            analytics.PredictedNextPurchaseDate = CalculateNextPurchaseDate(orders);

            // Determine purchase frequency
            if (analytics.AverageDaysBetweenOrders <= 30)
                analytics.PurchaseFrequency = "Regular";
            else if (analytics.AverageDaysBetweenOrders <= 90)
                analytics.PurchaseFrequency = "Occasional";
            else
                analytics.PurchaseFrequency = "One-time";

            // Get favorite products
            var productFrequency = orders
                .SelectMany(o => o.LineItems)
                .Where(li => !string.IsNullOrEmpty(li.Title))
                .GroupBy(li => li.Title)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();
            
            analytics.FavoriteProducts = productFrequency;
        }

        return analytics;
    }

    private string BuildCustomersUrlWithPageInfo(int limit, string? pageInfo)
    {
        // Only fetch the specific fields we need for better performance
        var url = $"{_config.BaseUrl}/admin/api/{_config.ApiVersion}/customers.json?limit={limit}&fields={CustomerFields}";
        if (!string.IsNullOrEmpty(pageInfo))
            url += $"&page_info={pageInfo}";
        return url;
    }

    private string BuildCustomerOrdersUrl(long customerId, string status, int limit, long? sinceId)
    {
        var url = $"{_config.BaseUrl}/admin/api/{_config.ApiVersion}/orders.json?customer_id={customerId}&status={status}&limit={limit}";
        if (sinceId.HasValue)
            url += $"&since_id={sinceId.Value}";
        return url;
    }

    private string BuildOrdersUrlWithPageInfo(string status, int limit, string? pageInfo, DateTime? createdAtMin, DateTime? createdAtMax, long? sinceId = null)
    {
        // According to Shopify docs, when using page_info, we can only include limit parameter
        // All other filters must be applied in the initial request only
        if (!string.IsNullOrEmpty(pageInfo))
        {
            return $"{_config.BaseUrl}/admin/api/{_config.ApiVersion}/orders.json?limit={limit}&page_info={pageInfo}";
        }
        
        // For the first request without page_info, include all parameters
        var url = $"{_config.BaseUrl}/admin/api/{_config.ApiVersion}/orders.json?status={status}&limit={limit}";
        if (sinceId.HasValue)
            url += $"&since_id={sinceId.Value}";
        if (createdAtMin.HasValue)
            url += $"&created_at_min={createdAtMin.Value:yyyy-MM-ddTHH:mm:ssZ}";
        if (createdAtMax.HasValue)
            url += $"&created_at_max={createdAtMax.Value:yyyy-MM-ddTHH:mm:ssZ}";
        return url;
    }

    private string BuildProductsUrl(int limit, long? sinceId, string? vendor, string? productType)
    {
        var url = $"{_config.BaseUrl}/admin/api/{_config.ApiVersion}/products.json?limit={limit}";
        if (sinceId.HasValue)
            url += $"&since_id={sinceId.Value}";
        if (!string.IsNullOrEmpty(vendor))
            url += $"&vendor={Uri.EscapeDataString(vendor)}";
        if (!string.IsNullOrEmpty(productType))
            url += $"&product_type={Uri.EscapeDataString(productType)}";
        return url;
    }

    private string BuildProductsUrlWithFields(int limit, long? sinceId, string fields)
    {
        var url = $"{_config.BaseUrl}/admin/api/{_config.ApiVersion}/products.json?limit={limit}&fields={fields}";
        if (sinceId.HasValue)
            url += $"&since_id={sinceId.Value}";
        return url;
    }

    private async Task<(List<ShopifyCustomer> customers, string? nextPageUrl)> FetchCustomersWithPaginationAsync(int limit, string? pageInfo = null)
    {
        var url = BuildCustomersUrlWithPageInfo(limit, pageInfo);
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var customersResponse = JsonSerializer.Deserialize<ShopifyCustomersResponse>(content, _jsonOptions);
            var customers = customersResponse?.Customers ?? new List<ShopifyCustomer>();
            
            // Parse Link header to get next page URL
            var nextPageUrl = ParseLinkHeader(response.Headers);
            
            return (customers, nextPageUrl);
        }

        _logger.LogWarning("Failed to fetch customers from {Url}. Status: {StatusCode}", url, response.StatusCode);
        return (new List<ShopifyCustomer>(), null);
    }

    private async Task<List<ShopifyOrder>> FetchOrdersAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var ordersResponse = JsonSerializer.Deserialize<ShopifyOrdersResponse>(content, _jsonOptions);
            return ordersResponse?.Orders ?? new List<ShopifyOrder>();
        }

        _logger.LogWarning("Failed to fetch orders from {Url}. Status: {StatusCode}", url, response.StatusCode);
        return new List<ShopifyOrder>();
    }

    private async Task<List<ShopifyProduct>> FetchProductsAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var productsResponse = JsonSerializer.Deserialize<ShopifyProductsResponse>(content, _jsonOptions);
            return productsResponse?.Products ?? new List<ShopifyProduct>();
        }

        _logger.LogWarning("Failed to fetch products from {Url}. Status: {StatusCode}", url, response.StatusCode);
        return new List<ShopifyProduct>();
    }

    private static bool ContainsAnyTag(List<string> customerTags, IEnumerable<string> searchTags)
    {
        return searchTags.Any(searchTag => 
            customerTags.Any(customerTag => 
                customerTag.Equals(searchTag, StringComparison.OrdinalIgnoreCase)));
    }

    private static double CalculateStandardDeviation(List<int> values)
    {
        if (values.Count <= 1) return 0;
        
        var average = values.Average();
        var sumOfSquares = values.Sum(val => Math.Pow(val - average, 2));
        return Math.Sqrt(sumOfSquares / values.Count);
    }

    private string? ParseLinkHeader(System.Net.Http.Headers.HttpResponseHeaders headers)
    {
        if (!headers.TryGetValues("Link", out var linkHeaders))
            return null;

        var linkHeader = linkHeaders.FirstOrDefault();
        if (string.IsNullOrEmpty(linkHeader))
            return null;

        // Parse Link header format: <https://example.com/api/customers.json?page_info=abc123&limit=250>; rel="next"
        var links = linkHeader.Split(',')
            .Select(link => link.Trim())
            .Where(link => !string.IsNullOrEmpty(link));

        foreach (var link in links)
        {
            var parts = link.Split(';');
            if (parts.Length != 2) continue;

            var url = parts[0].Trim();
            var rel = parts[1].Trim();

            // Remove angle brackets from URL
            if (url.StartsWith('<') && url.EndsWith('>'))
                url = url[1..^1];

            // Check if this is the "next" relation
            if (rel.Contains("rel=\"next\"") || rel.Contains("rel='next'"))
            {
                return url;
            }
        }

        return null;
    }

    private string? ExtractPageInfoFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        var uri = new Uri(url);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        return query["page_info"];
    }

    private static ShopifyOrder CreateOrderWithoutCustomer(ShopifyOrder order, Dictionary<long, List<string>>? productTagsLookup = null)
    {
        var lineItemsWithTags = order.LineItems.Select(lineItem =>
        {
            var newLineItem = new ShopifyLineItem
            {
                Id = lineItem.Id,
                AdminGraphqlApiId = lineItem.AdminGraphqlApiId,
                FulfillableQuantity = lineItem.FulfillableQuantity,
                Grams = lineItem.Grams,
                Name = lineItem.Name,
                Price = lineItem.Price,
                ProductId = lineItem.ProductId,
                Quantity = lineItem.Quantity,
                Title = lineItem.Title,
                TotalDiscount = lineItem.TotalDiscount,
                VariantId = lineItem.VariantId,
                VariantInventoryManagement = lineItem.VariantInventoryManagement,
                VariantTitle = lineItem.VariantTitle,
                Vendor = lineItem.Vendor,
            };

            // Add product tags if available
            if (productTagsLookup != null && lineItem.ProductId.HasValue && 
                productTagsLookup.TryGetValue(lineItem.ProductId.Value, out var tags))
            {
                newLineItem.ProductTags = tags;
            }

            return newLineItem;
        }).ToList();

        return new ShopifyOrder
        {
            Id = order.Id,
            CancelledAt = order.CancelledAt,
            ClosedAt = order.ClosedAt,
            CreatedAt = order.CreatedAt,
            FinancialStatus = order.FinancialStatus,
            Note = order.Note,
            OrderNumber = order.OrderNumber,
            ProcessedAt = order.ProcessedAt,
            Test = order.Test,
            TotalPrice = order.TotalPrice,
            UpdatedAt = order.UpdatedAt,
            LineItems = lineItemsWithTags,
            // Intentionally exclude Customer to avoid duplication
            Customer = null
        };
    }

    private NextPurchasePrediction CalculateNextPurchasePrediction(
        List<ShopifyOrder> orders,
        HashSet<long> categoryProductIds,
        Dictionary<long, List<string>> productTagsLookup,
        string categoryName)
    {
        var prediction = new NextPurchasePrediction
        {
            CalculatedAt = DateTime.UtcNow
        };

        try
        {
            if (orders.Count == 0)
            {
                prediction.HasSufficientData = false;
                prediction.PredictionReason = $"No {categoryName} orders found for this customer";
                prediction.ConfidenceLevel = 0.0;
                return prediction;
            }

            // Extract purchase dates
            var purchaseDates = orders
                .Select(o => o.CreatedAt)
                .OrderBy(d => d)
                .ToList();
            
            prediction.PurchaseDates = purchaseDates;

            // Extract product details for this category
            var productsInCategory = new Dictionary<long, ProductSummary>();
            
            foreach (var order in orders)
            {
                foreach (var lineItem in order.LineItems)
                {
                    if (!lineItem.ProductId.HasValue || !categoryProductIds.Contains(lineItem.ProductId.Value))
                        continue;

                    var productId = lineItem.ProductId.Value;
                    
                    if (!productsInCategory.ContainsKey(productId))
                    {
                        productsInCategory[productId] = new ProductSummary
                        {
                            ProductId = productId,
                            Title = lineItem.Title ?? "Unknown Product",
                            Tags = productTagsLookup.TryGetValue(productId, out var tags) ? tags : new List<string>(),
                            PurchaseCount = 0,
                            TotalQuantityPurchased = 0,
                            LastPurchaseDate = order.CreatedAt
                        };
                    }

                    var productSummary = productsInCategory[productId];
                    productSummary.PurchaseCount++;
                    productSummary.TotalQuantityPurchased += lineItem.Quantity;
                    
                    if (order.CreatedAt > productSummary.LastPurchaseDate)
                    {
                        productSummary.LastPurchaseDate = order.CreatedAt;
                    }
                }
            }

            prediction.ProductsInCategory = productsInCategory.Values.ToList();

            // Calculate next purchase date if we have enough data
            if (purchaseDates.Count < 2)
            {
                prediction.HasSufficientData = false;
                prediction.PredictionReason = $"Need at least 2 {categoryName} orders to calculate prediction. Found {purchaseDates.Count} order(s)";
                prediction.ConfidenceLevel = 0.0;
                return prediction;
            }

            // Calculate intervals between purchases
            var intervals = new List<double>();
            for (int i = 1; i < purchaseDates.Count; i++)
            {
                var daysBetween = (purchaseDates[i] - purchaseDates[i - 1]).TotalDays;
                intervals.Add(daysBetween);
            }

            if (intervals.Count == 0)
            {
                prediction.HasSufficientData = false;
                prediction.PredictionReason = "Unable to calculate intervals between purchases";
                prediction.ConfidenceLevel = 0.0;
                return prediction;
            }

            // Calculate average days between purchases
            var averageInterval = intervals.Average();
            prediction.AverageDaysBetweenPurchases = averageInterval;

            // Calculate standard deviation for confidence assessment
            var variance = intervals.Sum(interval => Math.Pow(interval - averageInterval, 2)) / intervals.Count;
            var standardDeviation = Math.Sqrt(variance);

            // Calculate confidence level based on consistency of purchase intervals
            // Lower standard deviation relative to average = higher confidence
            var coefficientOfVariation = standardDeviation / Math.Abs(averageInterval);
            var baseConfidence = Math.Max(0, 1 - (coefficientOfVariation / 2)); // Cap at reasonable level
            
            // Adjust confidence based on number of data points
            var dataPointsFactor = Math.Min(1.0, intervals.Count / 5.0); // Max confidence at 5+ intervals
            prediction.ConfidenceLevel = Math.Round(baseConfidence * dataPointsFactor, 2);

            // Calculate next purchase date
            var lastPurchaseDate = purchaseDates.Last();
            var adjustedInterval = averageInterval;
            
            // Add some variability based on standard deviation for more realistic prediction
            if (intervals.Count >= 3) // Only adjust if we have enough data points
            {
                adjustedInterval += (standardDeviation * 0.25); // Add small buffer
            }

            prediction.NextPurchaseDate = lastPurchaseDate.AddDays(adjustedInterval);
            prediction.HasSufficientData = true;
            
            // Generate prediction reason
            if (prediction.ConfidenceLevel >= 0.7)
            {
                prediction.PredictionReason = $"High confidence prediction based on {intervals.Count} purchase intervals. Consistent {categoryName} purchase pattern.";
            }
            else if (prediction.ConfidenceLevel >= 0.4)
            {
                prediction.PredictionReason = $"Moderate confidence prediction based on {intervals.Count} purchase intervals. Some variation in {categoryName} purchase timing.";
            }
            else
            {
                prediction.PredictionReason = $"Low confidence prediction based on {intervals.Count} purchase intervals. Irregular {categoryName} purchase pattern.";
            }

            _logger.LogDebug("Calculated next purchase prediction for {Category}: {NextDate} (confidence: {Confidence}, avg interval: {AvgInterval} days)", 
                categoryName, prediction.NextPurchaseDate?.ToString("yyyy-MM-dd"), prediction.ConfidenceLevel, Math.Round(averageInterval, 1));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next purchase prediction for {Category}", categoryName);
            prediction.HasSufficientData = false;
            prediction.PredictionReason = $"Error occurred while calculating {categoryName} prediction: {ex.Message}";
            prediction.ConfidenceLevel = 0.0;
        }

        return prediction;
    }
}