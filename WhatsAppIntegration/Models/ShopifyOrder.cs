using System.Text.Json.Serialization;

namespace WhatsAppIntegration.Models;

/// <summary>
/// Shopify order information
/// </summary>
public class ShopifyOrder
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    // [JsonPropertyName("admin_graphql_api_id")]
    // public string AdminGraphqlApiId { get; set; } = string.Empty;
    //
    // [JsonPropertyName("app_id")]
    // public long? AppId { get; set; }
    //
    // [JsonPropertyName("browser_ip")]
    // public string? BrowserIp { get; set; }
    //
    // [JsonPropertyName("buyer_accepts_marketing")]
    // public bool BuyerAcceptsMarketing { get; set; }

    // [JsonPropertyName("cancel_reason")]
    // public string? CancelReason { get; set; }

    [JsonPropertyName("cancelled_at")]
    public DateTime? CancelledAt { get; set; }

    // [JsonPropertyName("cart_token")]
    // public string? CartToken { get; set; }
    //
    // [JsonPropertyName("checkout_id")]
    // public long? CheckoutId { get; set; }
    //
    // [JsonPropertyName("checkout_token")]
    // public string? CheckoutToken { get; set; }

    [JsonPropertyName("closed_at")]
    public DateTime? ClosedAt { get; set; }

    // [JsonPropertyName("confirmed")]
    // public bool Confirmed { get; set; }
    //
    // [JsonPropertyName("contact_email")]
    // public string? ContactEmail { get; set; }
    //
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    // [JsonPropertyName("currency")]
    // public string Currency { get; set; } = "USD";
    //
    // [JsonPropertyName("current_subtotal_price")]
    // public string CurrentSubtotalPrice { get; set; } = "0.00";

    // [JsonPropertyName("current_subtotal_price_set")]
    // public ShopifyPriceSet? CurrentSubtotalPriceSet { get; set; }

    // [JsonPropertyName("current_total_discounts")]
    // public string CurrentTotalDiscounts { get; set; } = "0.00";

    // [JsonPropertyName("current_total_discounts_set")]
    // public ShopifyPriceSet? CurrentTotalDiscountsSet { get; set; }
    //
    // [JsonPropertyName("current_total_duties_set")]
    // public ShopifyPriceSet? CurrentTotalDutiesSet { get; set; }

    // [JsonPropertyName("current_total_price")]
    // public string CurrentTotalPrice { get; set; } = "0.00";

    // [JsonPropertyName("current_total_price_set")]
    // public ShopifyPriceSet? CurrentTotalPriceSet { get; set; }

    // [JsonPropertyName("current_total_tax")]
    // public string CurrentTotalTax { get; set; } = "0.00";

    // [JsonPropertyName("current_total_tax_set")]
    // public ShopifyPriceSet? CurrentTotalTaxSet { get; set; }

    // [JsonPropertyName("customer_locale")]
    // public string? CustomerLocale { get; set; }
    //
    // [JsonPropertyName("device_id")]
    // public long? DeviceId { get; set; }

    // [JsonPropertyName("discount_codes")]
    // public List<ShopifyDiscountCode> DiscountCodes { get; set; } = new();

    // [JsonPropertyName("email")]
    // public string? Email { get; set; }
    //
    // [JsonPropertyName("estimated_taxes")]
    // public bool EstimatedTaxes { get; set; }
    //
    [JsonPropertyName("financial_status")]
    public string FinancialStatus { get; set; } = string.Empty;
    
    // [JsonPropertyName("fulfillment_status")]
    // public string? FulfillmentStatus { get; set; }
    //
    // [JsonPropertyName("gateway")]
    // public string? Gateway { get; set; }
    //
    // [JsonPropertyName("landing_site")]
    // public string? LandingSite { get; set; }
    //
    // [JsonPropertyName("landing_site_ref")]
    // public string? LandingSiteRef { get; set; }
    //
    // [JsonPropertyName("location_id")]
    // public long? LocationId { get; set; }
    //
    // [JsonPropertyName("name")]
    // public string Name { get; set; } = string.Empty;
    //
    [JsonPropertyName("note")]
    public string? Note { get; set; }

    // [JsonPropertyName("note_attributes")]
    // public List<ShopifyNoteAttribute> NoteAttributes { get; set; } = new();

    // [JsonPropertyName("number")]
    // public int Number { get; set; }
    //
    [JsonPropertyName("order_number")]
    public int OrderNumber { get; set; }
    
    // [JsonPropertyName("order_status_url")]
    // public string? OrderStatusUrl { get; set; }
    //
    // [JsonPropertyName("original_total_duties_set")]
    // public ShopifyPriceSet? OriginalTotalDutiesSet { get; set; }
    //
    // [JsonPropertyName("payment_gateway_names")]
    // public List<string> PaymentGatewayNames { get; set; } = new();
    //
    // [JsonPropertyName("phone")]
    // public string? Phone { get; set; }
    //
    // [JsonPropertyName("presentment_currency")]
    // public string PresentmentCurrency { get; set; } = "USD";
    //
    [JsonPropertyName("processed_at")]
    public DateTime ProcessedAt { get; set; }
    
    // [JsonPropertyName("processing_method")]
    // public string? ProcessingMethod { get; set; }
    //
    // [JsonPropertyName("reference")]
    // public string? Reference { get; set; }
    //
    // [JsonPropertyName("referring_site")]
    // public string? ReferringSite { get; set; }
    //
    // [JsonPropertyName("source_identifier")]
    // public string? SourceIdentifier { get; set; }
    //
    // [JsonPropertyName("source_name")]
    // public string? SourceName { get; set; }
    //
    // [JsonPropertyName("source_url")]
    // public string? SourceUrl { get; set; }
    //
    // [JsonPropertyName("subtotal_price")]
    // public string SubtotalPrice { get; set; } = "0.00";
    //
    // [JsonPropertyName("subtotal_price_set")]
    // public ShopifyPriceSet? SubtotalPriceSet { get; set; }
    //
    [JsonPropertyName("tags")]
    private string Tags { get; set; } = string.Empty;
    
    // [JsonPropertyName("tax_lines")]
    // public List<ShopifyTaxLine> TaxLines { get; set; } = new();
    //
    // [JsonPropertyName("taxes_included")]
    // public bool TaxesIncluded { get; set; }
    //
    [JsonPropertyName("test")]
    public bool Test { get; set; }
    
    // [JsonPropertyName("token")]
    // public string Token { get; set; } = string.Empty;
    //
    // [JsonPropertyName("total_discounts")]
    // public string TotalDiscounts { get; set; } = "0.00";
    //
    // [JsonPropertyName("total_discounts_set")]
    // public ShopifyPriceSet? TotalDiscountsSet { get; set; }
    //
    // [JsonPropertyName("total_line_items_price")]
    // public string TotalLineItemsPrice { get; set; } = "0.00";
    //
    // [JsonPropertyName("total_line_items_price_set")]
    // public ShopifyPriceSet? TotalLineItemsPriceSet { get; set; }
    //
    // [JsonPropertyName("total_outstanding")]
    // public string TotalOutstanding { get; set; } = "0.00";
    //
    [JsonPropertyName("total_price")]
    public string TotalPrice { get; set; } = "0.00";
    
    // [JsonPropertyName("total_price_set")]
    // public ShopifyPriceSet? TotalPriceSet { get; set; }
    //
    // [JsonPropertyName("total_price_usd")]
    // public string? TotalPriceUsd { get; set; }
    //
    // [JsonPropertyName("total_shipping_price_set")]
    // public ShopifyPriceSet? TotalShippingPriceSet { get; set; }
    //
    // [JsonPropertyName("total_tax")]
    // public string TotalTax { get; set; } = "0.00";
    //
    // [JsonPropertyName("total_tax_set")]
    // public ShopifyPriceSet? TotalTaxSet { get; set; }
    //
    // [JsonPropertyName("total_tip_received")]
    // public string TotalTipReceived { get; set; } = "0.00";
    //
    // [JsonPropertyName("total_weight")]
    // public int TotalWeight { get; set; }
    //
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    // [JsonPropertyName("user_id")]
    // public long? UserId { get; set; }
    //
    // [JsonPropertyName("billing_address")]
    // public ShopifyAddress? BillingAddress { get; set; }
    //
    [JsonPropertyName("customer")]
    public ShopifyCustomer? Customer { get; set; }
    
    // [JsonPropertyName("discount_applications")]
    // public List<ShopifyDiscountApplication> DiscountApplications { get; set; } = new();
    //
    // [JsonPropertyName("fulfillments")]
    // public List<ShopifyFulfillment> Fulfillments { get; set; } = new();
    //
    [JsonPropertyName("line_items")]
    public List<ShopifyLineItem> LineItems { get; set; } = new();
    
    // [JsonPropertyName("payment_terms")]
    // public ShopifyPaymentTerms? PaymentTerms { get; set; }
    //
    // [JsonPropertyName("refunds")]
    // public List<ShopifyRefund> Refunds { get; set; } = new();
    //
    // [JsonPropertyName("shipping_address")]
    // public ShopifyAddress? ShippingAddress { get; set; }
    //
    // [JsonPropertyName("shipping_lines")]
    // public List<ShopifyShippingLine> ShippingLines { get; set; } = new();

    public List<string> TagsList => Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(tag => tag.Trim()).ToList();
}