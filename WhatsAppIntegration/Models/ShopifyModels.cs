using System.Text.Json.Serialization;

namespace WhatsAppIntegration.Models;

/// <summary>
/// Shopify configuration settings
/// </summary>
public class ShopifyConfig
{
    /// <summary>
    /// Shopify shop domain (without .myshopify.com)
    /// </summary>
    public string ShopDomain { get; set; } = string.Empty;
    
    /// <summary>
    /// Shopify private app access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Shopify API version
    /// </summary>
    public string ApiVersion { get; set; } = "2024-01";
    
    /// <summary>
    /// Full base URL for Shopify API calls
    /// </summary>
    public string BaseUrl => $"https://{ShopDomain}";
}

/// <summary>
/// Shopify customer address
/// </summary>
public class ShopifyAddress
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("customer_id")]
    public long CustomerId { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("company")]
    public string? Company { get; set; }

    [JsonPropertyName("address1")]
    public string? Address1 { get; set; }

    [JsonPropertyName("address2")]
    public string? Address2 { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("province")]
    public string? Province { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("zip")]
    public string? Zip { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("province_code")]
    public string? ProvinceCode { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("country_name")]
    public string? CountryName { get; set; }

    [JsonPropertyName("default")]
    public bool Default { get; set; }
}

/// <summary>
/// Shopify marketing consent information
/// </summary>
public class ShopifyMarketingConsent
{
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("opt_in_level")]
    public string? OptInLevel { get; set; }

    [JsonPropertyName("consent_updated_at")]
    public DateTime? ConsentUpdatedAt { get; set; }
}

/// <summary>
/// Shopify price set with shop and presentment currency
/// </summary>
public class ShopifyPriceSet
{
    [JsonPropertyName("shop_money")]
    public ShopifyMoney? ShopMoney { get; set; }

    [JsonPropertyName("presentment_money")]
    public ShopifyMoney? PresentmentMoney { get; set; }
}

/// <summary>
/// Shopify money amount with currency
/// </summary>
public class ShopifyMoney
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0.00";

    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; } = "USD";
}

/// <summary>
/// Shopify discount code applied to order
/// </summary>
public class ShopifyDiscountCode
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0.00";

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Shopify note attribute for custom order data
/// </summary>
public class ShopifyNoteAttribute
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Shopify tax line information
/// </summary>
public class ShopifyTaxLine
{
    [JsonPropertyName("price")]
    public string Price { get; set; } = "0.00";

    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("price_set")]
    public ShopifyPriceSet? PriceSet { get; set; }
}

/// <summary>
/// Shopify discount application
/// </summary>
public class ShopifyDiscountApplication
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = "0.00";

    [JsonPropertyName("value_type")]
    public string ValueType { get; set; } = string.Empty;

    [JsonPropertyName("allocation_method")]
    public string AllocationMethod { get; set; } = string.Empty;

    [JsonPropertyName("target_selection")]
    public string TargetSelection { get; set; } = string.Empty;

    [JsonPropertyName("target_type")]
    public string TargetType { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

/// <summary>
/// Shopify order fulfillment
/// </summary>
public class ShopifyFulfillment
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("order_id")]
    public long OrderId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("service")]
    public string Service { get; set; } = string.Empty;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("tracking_company")]
    public string? TrackingCompany { get; set; }

    [JsonPropertyName("tracking_number")]
    public string? TrackingNumber { get; set; }

    [JsonPropertyName("tracking_numbers")]
    public List<string> TrackingNumbers { get; set; } = new();

    [JsonPropertyName("tracking_url")]
    public string? TrackingUrl { get; set; }

    [JsonPropertyName("tracking_urls")]
    public List<string> TrackingUrls { get; set; } = new();

    [JsonPropertyName("receipt")]
    public object? Receipt { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("line_items")]
    public List<ShopifyLineItem> LineItems { get; set; } = new();
}

/// <summary>
/// Shopify order line item
/// </summary>
public class ShopifyLineItem
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("admin_graphql_api_id")]
    public string AdminGraphqlApiId { get; set; } = string.Empty;

    [JsonPropertyName("fulfillable_quantity")]
    public int FulfillableQuantity { get; set; }

    // [JsonPropertyName("fulfillment_service")]
    // public string FulfillmentService { get; set; } = string.Empty;
    //
    // [JsonPropertyName("fulfillment_status")]
    // public string? FulfillmentStatus { get; set; }
    //
    // [JsonPropertyName("gift_card")]
    // public bool GiftCard { get; set; }

    [JsonPropertyName("grams")]
    public int Grams { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = "0.00";

    // [JsonPropertyName("price_set")]
    // public ShopifyPriceSet? PriceSet { get; set; }
    //
    // [JsonPropertyName("product_exists")]
    // public bool ProductExists { get; set; }

    [JsonPropertyName("product_id")]
    public long? ProductId { get; set; }

    // [JsonPropertyName("properties")]
    // public List<ShopifyProperty> Properties { get; set; } = new();

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    // [JsonPropertyName("requires_shipping")]
    // public bool RequiresShipping { get; set; }
    //
    // [JsonPropertyName("sku")]
    // public string? Sku { get; set; }
    //
    // [JsonPropertyName("taxable")]
    // public bool Taxable { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("total_discount")]
    public string TotalDiscount { get; set; } = "0.00";

    // [JsonPropertyName("total_discount_set")]
    // public ShopifyPriceSet? TotalDiscountSet { get; set; }

    [JsonPropertyName("variant_id")]
    public long? VariantId { get; set; }

    [JsonPropertyName("variant_inventory_management")]
    public string? VariantInventoryManagement { get; set; }

    [JsonPropertyName("variant_title")]
    public string? VariantTitle { get; set; }

    [JsonPropertyName("vendor")]
    public string? Vendor { get; set; }

    // [JsonPropertyName("tax_lines")]
    // public List<ShopifyTaxLine> TaxLines { get; set; } = new();

    // [JsonPropertyName("duties")]
    // public List<object> Duties { get; set; } = new();
    //
    // [JsonPropertyName("discount_allocations")]
    // public List<ShopifyDiscountAllocation> DiscountAllocations { get; set; } = new();
    [JsonPropertyName("product_tags")]
    public List<string> ProductTags { get; set; } = new();
}

/// <summary>
/// Shopify line item property
/// </summary>
public class ShopifyProperty
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Shopify discount allocation
/// </summary>
public class ShopifyDiscountAllocation
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0.00";

    [JsonPropertyName("amount_set")]
    public ShopifyPriceSet? AmountSet { get; set; }

    [JsonPropertyName("discount_application_index")]
    public int DiscountApplicationIndex { get; set; }
}

/// <summary>
/// Shopify payment terms
/// </summary>
public class ShopifyPaymentTerms
{
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    [JsonPropertyName("payment_terms_name")]
    public string PaymentTermsName { get; set; } = string.Empty;

    [JsonPropertyName("payment_terms_type")]
    public string PaymentTermsType { get; set; } = string.Empty;

    [JsonPropertyName("due_in_days")]
    public int? DueInDays { get; set; }

    [JsonPropertyName("payment_schedules")]
    public List<ShopifyPaymentSchedule> PaymentSchedules { get; set; } = new();
}

/// <summary>
/// Shopify payment schedule
/// </summary>
public class ShopifyPaymentSchedule
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0.00";

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    [JsonPropertyName("issued_at")]
    public DateTime? IssuedAt { get; set; }

    [JsonPropertyName("due_at")]
    public DateTime? DueAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("expected_payment_method")]
    public string? ExpectedPaymentMethod { get; set; }
}

/// <summary>
/// Shopify order refund
/// </summary>
public class ShopifyRefund
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("admin_graphql_api_id")]
    public string AdminGraphqlApiId { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("order_id")]
    public long OrderId { get; set; }

    [JsonPropertyName("processed_at")]
    public DateTime ProcessedAt { get; set; }

    [JsonPropertyName("restock")]
    public bool Restock { get; set; }

    [JsonPropertyName("total_duties_set")]
    public ShopifyPriceSet? TotalDutiesSet { get; set; }

    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }

    [JsonPropertyName("order_adjustments")]
    public List<object> OrderAdjustments { get; set; } = new();

    [JsonPropertyName("transactions")]
    public List<ShopifyTransaction> Transactions { get; set; } = new();

    [JsonPropertyName("refund_line_items")]
    public List<ShopifyRefundLineItem> RefundLineItems { get; set; } = new();

    [JsonPropertyName("duties")]
    public List<object> Duties { get; set; } = new();
}

/// <summary>
/// Shopify refund transaction
/// </summary>
public class ShopifyTransaction
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("admin_graphql_api_id")]
    public string AdminGraphqlApiId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0.00";

    [JsonPropertyName("authorization")]
    public string? Authorization { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    [JsonPropertyName("device_id")]
    public long? DeviceId { get; set; }

    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("gateway")]
    public string Gateway { get; set; } = string.Empty;

    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("location_id")]
    public long? LocationId { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("order_id")]
    public long OrderId { get; set; }

    [JsonPropertyName("parent_id")]
    public long? ParentId { get; set; }

    [JsonPropertyName("processed_at")]
    public DateTime ProcessedAt { get; set; }

    [JsonPropertyName("receipt")]
    public object? Receipt { get; set; }

    [JsonPropertyName("source_name")]
    public string? SourceName { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("test")]
    public bool Test { get; set; }

    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }

    [JsonPropertyName("currency_exchange_adjustment")]
    public object? CurrencyExchangeAdjustment { get; set; }
}

/// <summary>
/// Shopify refund line item
/// </summary>
public class ShopifyRefundLineItem
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("line_item")]
    public ShopifyLineItem LineItem { get; set; } = new();

    [JsonPropertyName("line_item_id")]
    public long LineItemId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("restock_type")]
    public string RestockType { get; set; } = string.Empty;

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; set; } = 0.00m;

    [JsonPropertyName("subtotal_set")]
    public ShopifyPriceSet? SubtotalSet { get; set; }

    [JsonPropertyName("total_tax")]
    public decimal TotalTax { get; set; } = 0.00m;

    [JsonPropertyName("total_tax_set")]
    public ShopifyPriceSet? TotalTaxSet { get; set; }
}

/// <summary>
/// Shopify shipping line
/// </summary>
public class ShopifyShippingLine
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("carrier_identifier")]
    public string? CarrierIdentifier { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("delivery_category")]
    public string? DeliveryCategory { get; set; }

    [JsonPropertyName("discounted_price")]
    public string DiscountedPrice { get; set; } = "0.00";

    [JsonPropertyName("discounted_price_set")]
    public ShopifyPriceSet? DiscountedPriceSet { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("price")]
    public string Price { get; set; } = "0.00";

    [JsonPropertyName("price_set")]
    public ShopifyPriceSet? PriceSet { get; set; }

    [JsonPropertyName("requested_fulfillment_service_id")]
    public long? RequestedFulfillmentServiceId { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("tax_lines")]
    public List<ShopifyTaxLine> TaxLines { get; set; } = new();

    [JsonPropertyName("discount_allocations")]
    public List<ShopifyDiscountAllocation> DiscountAllocations { get; set; } = new();
}

/// <summary>
/// Shopify product information
/// </summary>
public class ShopifyProduct
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body_html")]
    public string? BodyHtml { get; set; }

    [JsonPropertyName("vendor")]
    public string Vendor { get; set; } = string.Empty;

    [JsonPropertyName("product_type")]
    public string ProductType { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("handle")]
    public string Handle { get; set; } = string.Empty;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("published_at")]
    public DateTime? PublishedAt { get; set; }

    [JsonPropertyName("template_suffix")]
    public string? TemplateSuffix { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("published_scope")]
    public string PublishedScope { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public string Tags { get; set; } = string.Empty;

    [JsonPropertyName("admin_graphql_api_id")]
    public string AdminGraphqlApiId { get; set; } = string.Empty;

    [JsonPropertyName("variants")]
    public List<ShopifyProductVariant> Variants { get; set; } = new();

    [JsonPropertyName("options")]
    public List<ShopifyProductOption> Options { get; set; } = new();

    [JsonPropertyName("images")]
    public List<ShopifyProductImage> Images { get; set; } = new();

    [JsonPropertyName("image")]
    public ShopifyProductImage? Image { get; set; }

    public List<string> TagsList => Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(tag => tag.Trim()).ToList();
}

/// <summary>
/// Shopify product variant
/// </summary>
public class ShopifyProductVariant
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("product_id")]
    public long ProductId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = "0.00";

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("inventory_policy")]
    public string InventoryPolicy { get; set; } = string.Empty;

    [JsonPropertyName("compare_at_price")]
    public string? CompareAtPrice { get; set; }

    [JsonPropertyName("fulfillment_service")]
    public string FulfillmentService { get; set; } = string.Empty;

    [JsonPropertyName("inventory_management")]
    public string? InventoryManagement { get; set; }

    [JsonPropertyName("option1")]
    public string? Option1 { get; set; }

    [JsonPropertyName("option2")]
    public string? Option2 { get; set; }

    [JsonPropertyName("option3")]
    public string? Option3 { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("taxable")]
    public bool Taxable { get; set; }

    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }

    [JsonPropertyName("grams")]
    public int Grams { get; set; }

    [JsonPropertyName("image_id")]
    public long? ImageId { get; set; }

    [JsonPropertyName("weight")]
    public decimal Weight { get; set; }

    [JsonPropertyName("weight_unit")]
    public string WeightUnit { get; set; } = "kg";

    [JsonPropertyName("inventory_item_id")]
    public long InventoryItemId { get; set; }

    [JsonPropertyName("inventory_quantity")]
    public int InventoryQuantity { get; set; }

    [JsonPropertyName("old_inventory_quantity")]
    public int OldInventoryQuantity { get; set; }

    [JsonPropertyName("requires_shipping")]
    public bool RequiresShipping { get; set; }

    [JsonPropertyName("admin_graphql_api_id")]
    public string AdminGraphqlApiId { get; set; } = string.Empty;
}

/// <summary>
/// Shopify product option
/// </summary>
public class ShopifyProductOption
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("product_id")]
    public long ProductId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("values")]
    public List<string> Values { get; set; } = new();
}

/// <summary>
/// Shopify product image
/// </summary>
public class ShopifyProductImage
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("product_id")]
    public long ProductId { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("alt")]
    public string? Alt { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("src")]
    public string Src { get; set; } = string.Empty;

    [JsonPropertyName("variant_ids")]
    public List<long> VariantIds { get; set; } = new();

    [JsonPropertyName("admin_graphql_api_id")]
    public string AdminGraphqlApiId { get; set; } = string.Empty;
}

/// <summary>
/// Shopify API response wrapper for customers
/// </summary>
public class ShopifyCustomersResponse
{
    [JsonPropertyName("count"), JsonPropertyOrder(0)]
    public int Count => Customers.Count;
    
    [JsonPropertyName("customers"), JsonPropertyOrder(1)]
    public List<ShopifyCustomer> Customers { get; set; } = new();
    
}

/// <summary>
/// Shopify API response wrapper for orders
/// </summary>
public class ShopifyOrdersResponse
{
    [JsonPropertyName("orders")]
    public List<ShopifyOrder> Orders { get; set; } = new();
}

/// <summary>
/// Shopify API response wrapper for products
/// </summary>
public class ShopifyProductsResponse
{
    [JsonPropertyName("products")]
    public List<ShopifyProduct> Products { get; set; } = new();
}

/// <summary>
/// Response model for orders grouped by customer ID
/// </summary>
public class ShopifyOrdersByCustomerResponse
{
    [JsonPropertyName("ordersByCustomer"), JsonPropertyOrder(2)]
    public Dictionary<long, List<ShopifyOrder>> OrdersByCustomer { get; set; } = new();
    
    [JsonPropertyName("totalCustomers"), JsonPropertyOrder(0)]
    public int TotalCustomers => OrdersByCustomer.Count;
    
    [JsonPropertyName("totalOrders"), JsonPropertyOrder(1)]
    public int TotalOrders => OrdersByCustomer.Values.Sum(orders => orders.Count);
}

/// <summary>
/// Model for customer orders categorized by product type
/// </summary>
public class CustomerCategorizedOrders
{
    [JsonPropertyName("customer")]
    public ShopifyCustomer Customer { get; set; } = new();
    
    [JsonPropertyName("automationProductsOrders")]
    public List<ShopifyOrder> AutomationProductsOrders { get; set; } = new();
    
    [JsonPropertyName("dogExtraProductsOrders")]
    public List<ShopifyOrder> DogExtraProductsOrders { get; set; } = new();
    
    [JsonPropertyName("automationNextPurchase")]
    public NextPurchasePrediction? AutomationNextPurchase { get; set; }
    
    [JsonPropertyName("dogExtraNextPurchase")]
    public NextPurchasePrediction? DogExtraNextPurchase { get; set; }
    
    [JsonPropertyName("totalOrders")]
    public int TotalOrders => AutomationProductsOrders.Count + DogExtraProductsOrders.Count;
}

/// <summary>
/// Response model for orders grouped by customer ID and categorized by product type
/// </summary>
public class ShopifyCategorizedOrdersByCustomerResponse
{
    [JsonPropertyName("ordersByCustomer"), JsonPropertyOrder(2)]
    public Dictionary<long, CustomerCategorizedOrders> OrdersByCustomer { get; set; } = new();
    
    [JsonPropertyName("totalCustomers"), JsonPropertyOrder(0)]
    public int TotalCustomers => OrdersByCustomer.Count;
    
    [JsonPropertyName("totalOrders"), JsonPropertyOrder(1)]
    public int TotalOrders => OrdersByCustomer.Values.Sum(orders => orders.TotalOrders);
    
    [JsonPropertyName("totalAutomationOrders"), JsonPropertyOrder(3)]
    public int TotalAutomationOrders => OrdersByCustomer.Values.Sum(orders => orders.AutomationProductsOrders.Count);
    
    [JsonPropertyName("totalDogExtraOrders"), JsonPropertyOrder(4)]
    public int TotalDogExtraOrders => OrdersByCustomer.Values.Sum(orders => orders.DogExtraProductsOrders.Count);
}

/// <summary>
/// Response model for categorized products based on tags
/// </summary>
public class ShopifyCategorizedProductsResponse
{
    [JsonPropertyName("automationProducts"), JsonPropertyOrder(1)]
    public List<ShopifyProduct> AutomationProducts { get; set; } = new();
    
    [JsonPropertyName("products"), JsonPropertyOrder(2)]
    public List<ShopifyProduct> Products { get; set; } = new();
    
    [JsonPropertyName("dogExtraProducts"), JsonPropertyOrder(3)]
    public List<ShopifyProduct> DogExtraProducts { get; set; } = new();
    
    [JsonPropertyName("totalProductsCount"), JsonPropertyOrder(0)]
    public int TotalProductsCount { get; set; }
}

/// <summary>
/// Customer analytics data with purchase predictions
/// </summary>
public class CustomerAnalytics
{
    public long CustomerId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public List<string> Tags { get; set; } = new();
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public DateTime? PredictedNextPurchaseDate { get; set; }
    public int DaysSinceLastOrder { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int AverageDaysBetweenOrders { get; set; }
    public List<ShopifyOrder> RecentOrders { get; set; } = new();
    public List<string> FavoriteProducts { get; set; } = new();
    public string PurchaseFrequency { get; set; } = string.Empty; // "Regular", "Occasional", "One-time"
}

/// <summary>
/// Response model for categorized orders processing summary
/// </summary>
public class CategorizedOrdersProcessedResponse
{
    /// <summary>
    /// List of customer IDs that were processed and saved to database
    /// </summary>
    public List<long> ProcessedCustomerIds { get; set; } = new();
    
    /// <summary>
    /// Total count of customers processed
    /// </summary>
    public int ProcessedCustomersCount { get; set; }
    
    /// <summary>
    /// Timestamp when processing was completed
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}