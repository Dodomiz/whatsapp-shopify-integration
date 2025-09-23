using System.Text.Json.Serialization;

namespace WhatsAppIntegration.Models;

/// <summary>
/// Shopify customer information
/// </summary>
public class ShopifyCustomer
{
    /// <summary>
    /// Unique customer ID
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Customer email address
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Customer creation date
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Customer last update date
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Customer first name
    /// </summary>
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Customer last name
    /// </summary>
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    /// <summary>
    /// Total number of orders placed by customer
    /// </summary>
    [JsonPropertyName("orders_count")]
    public int OrdersCount { get; set; }

    /// <summary>
    /// Customer account state
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Total amount spent by customer
    /// </summary>
    [JsonPropertyName("total_spent")]
    public string TotalSpent { get; set; } = "0.00";

    [JsonPropertyName("last_order_id")]
    public long? LastOrderId { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    // [JsonPropertyName("verified_email")]
    // public bool VerifiedEmail { get; set; }
    //
    // [JsonPropertyName("multipass_identifier")]
    // public string? MultipassIdentifier { get; set; }
    //
    // [JsonPropertyName("tax_exempt")]
    // public bool TaxExempt { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("tags")]
    public string Tags { get; set; } = string.Empty;

    [JsonPropertyName("last_order_name")]
    public string? LastOrderName { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    // [JsonPropertyName("addresses")]
    // public List<ShopifyAddress> Addresses { get; set; } = new();

    [JsonPropertyName("accepts_marketing")]
    public bool AcceptsMarketing { get; set; }

    [JsonPropertyName("accepts_marketing_updated_at")]
    public DateTime? AcceptsMarketingUpdatedAt { get; set; }

    [JsonPropertyName("marketing_opt_in_level")]
    public string? MarketingOptInLevel { get; set; }

    // [JsonPropertyName("tax_exemptions")]
    // public List<string> TaxExemptions { get; set; } = new();

    // [JsonPropertyName("email_marketing_consent")]
    // public ShopifyMarketingConsent? EmailMarketingConsent { get; set; }
    //
    // [JsonPropertyName("sms_marketing_consent")]
    // public ShopifyMarketingConsent? SmsMarketingConsent { get; set; }

    [JsonPropertyName("admin_graphql_api_id")]
    public string AdminGraphqlApiId { get; set; } = string.Empty;

    // [JsonPropertyName("default_address")]
    // public ShopifyAddress? DefaultAddress { get; set; }

    public List<string> TagsList => Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(tag => tag.Trim()).ToList();
}