using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WhatsAppIntegration.Services;
using WhatsAppIntegration.Models;

namespace WhatsAppIntegration.Controllers;

/// <summary>
/// WhatsApp Business API Integration Controller
/// Provides webhook endpoints for receiving messages and REST APIs for sending messages
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("WhatsApp")]
public class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly WhatsAppConfig _config;
    private readonly ILogger<WhatsAppController> _logger;

    /// <summary>
    /// Initializes a new instance of the WhatsAppController
    /// </summary>
    /// <param name="whatsAppService">WhatsApp service instance</param>
    /// <param name="config">WhatsApp configuration</param>
    /// <param name="logger">Logger instance</param>
    public WhatsAppController(IWhatsAppService whatsAppService, IOptions<WhatsAppConfig> config, ILogger<WhatsAppController> logger)
    {
        _whatsAppService = whatsAppService;
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>
    /// Webhook verification endpoint for WhatsApp Business API
    /// </summary>
    /// <param name="mode">Hub mode (should be 'subscribe')</param>
    /// <param name="challenge">Challenge string to return for verification</param>
    /// <param name="verifyToken">Verification token to validate</param>
    /// <returns>Challenge string if verification succeeds, otherwise Unauthorized</returns>
    /// <response code="200">Webhook verification successful</response>
    /// <response code="401">Webhook verification failed</response>
    [HttpGet("webhook")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(401)]
    public IActionResult VerifyWebhook([FromQuery(Name = "hub.mode")] string mode,
                                      [FromQuery(Name = "hub.challenge")] string challenge,
                                      [FromQuery(Name = "hub.verify_token")] string verifyToken)
    {
        _logger.LogInformation("Webhook verification request received. Mode: {Mode}, Token: {Token}", mode, verifyToken);

        if (mode == "subscribe" && verifyToken == _config.WebhookVerifyToken)
        {
            _logger.LogInformation("Webhook verification successful");
            return Ok(challenge);
        }

        _logger.LogWarning("Webhook verification failed. Expected token: {Expected}, Received: {Received}", 
            _config.WebhookVerifyToken, verifyToken);
        return Unauthorized();
    }

    /// <summary>
    /// Webhook endpoint for receiving WhatsApp messages and status updates
    /// </summary>
    /// <returns>Success response</returns>
    /// <response code="200">Webhook processed successfully</response>
    /// <response code="400">Invalid request body</response>
    /// <response code="401">Signature verification failed</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("webhook")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> HandleWebhook()
    {
        try
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            
            if (string.IsNullOrEmpty(body))
            {
                _logger.LogWarning("Received empty webhook body");
                return BadRequest("Empty body");
            }

            // Verify webhook signature if app secret is configured
            if (!string.IsNullOrEmpty(_config.AppSecret))
            {
                var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
                if (string.IsNullOrEmpty(signature) || !VerifySignature(body, signature, _config.AppSecret))
                {
                    _logger.LogWarning("Webhook signature verification failed");
                    return Unauthorized();
                }
            }

            _logger.LogDebug("Received webhook: {Body}", body);

            var webhookEvent = JsonSerializer.Deserialize<WebhookEvent>(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (webhookEvent?.Entry != null)
            {
                await ProcessWebhookEvent(webhookEvent);
            }

            return Ok();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse webhook JSON");
            return BadRequest("Invalid JSON");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Send a text message via WhatsApp
    /// </summary>
    /// <param name="request">Text message details</param>
    /// <returns>Success response with message status</returns>
    /// <response code="200">Message sent successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Failed to send message</response>
    [HttpPost("send-text")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SendTextMessage([FromBody] SendTextRequest request)
    {
        if (string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.Message))
        {
            return BadRequest("To and Message are required");
        }

        var success = await _whatsAppService.SendTextMessageAsync(request.To, request.Message, request.PreviewUrl);
        
        if (success)
        {
            return Ok(new { success = true, message = "Message sent successfully" });
        }

        return StatusCode(500, new { success = false, message = "Failed to send message" });
    }

    /// <summary>
    /// Send an image message via WhatsApp
    /// </summary>
    /// <param name="request">Image message details including URL and optional caption</param>
    /// <returns>Success response with message status</returns>
    /// <response code="200">Image sent successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Failed to send image</response>
    [HttpPost("send-image")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SendImageMessage([FromBody] SendImageRequest request)
    {
        if (string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.ImageUrl))
        {
            return BadRequest("To and ImageUrl are required");
        }

        var success = await _whatsAppService.SendImageMessageAsync(request.To, request.ImageUrl, request.Caption);
        
        if (success)
        {
            return Ok(new { success = true, message = "Image sent successfully" });
        }

        return StatusCode(500, new { success = false, message = "Failed to send image" });
    }

    /// <summary>
    /// Send a document message via WhatsApp
    /// </summary>
    /// <param name="request">Document message details including URL, filename, and optional caption</param>
    /// <returns>Success response with message status</returns>
    /// <response code="200">Document sent successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Failed to send document</response>
    [HttpPost("send-document")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SendDocumentMessage([FromBody] SendDocumentRequest request)
    {
        if (string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.DocumentUrl))
        {
            return BadRequest("To and DocumentUrl are required");
        }

        var success = await _whatsAppService.SendDocumentMessageAsync(request.To, request.DocumentUrl, request.Filename, request.Caption);
        
        if (success)
        {
            return Ok(new { success = true, message = "Document sent successfully" });
        }

        return StatusCode(500, new { success = false, message = "Failed to send document" });
    }

    /// <summary>
    /// Send a location message via WhatsApp
    /// </summary>
    /// <param name="request">Location details including coordinates and optional name/address</param>
    /// <returns>Success response with message status</returns>
    /// <response code="200">Location sent successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Failed to send location</response>
    [HttpPost("send-location")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SendLocationMessage([FromBody] SendLocationRequest request)
    {
        if (string.IsNullOrEmpty(request.To))
        {
            return BadRequest("To is required");
        }

        var success = await _whatsAppService.SendLocationMessageAsync(request.To, request.Latitude, request.Longitude, request.Name, request.Address);
        
        if (success)
        {
            return Ok(new { success = true, message = "Location sent successfully" });
        }

        return StatusCode(500, new { success = false, message = "Failed to send location" });
    }

    /// <summary>
    /// Send an interactive button message via WhatsApp
    /// </summary>
    /// <param name="request">Interactive button message details</param>
    /// <returns>Success response with message status</returns>
    /// <response code="200">Interactive button message sent successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Failed to send interactive button message</response>
    [HttpPost("send-interactive-buttons")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SendInteractiveButtonMessage([FromBody] SendInteractiveButtonRequest request)
    {
        if (string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.BodyText) || request.Buttons == null || !request.Buttons.Any())
        {
            return BadRequest("To, BodyText, and Buttons are required");
        }

        var success = await _whatsAppService.SendInteractiveButtonMessageAsync(request.To, request.BodyText, request.Buttons, request.HeaderText, request.FooterText);
        
        if (success)
        {
            return Ok(new { success = true, message = "Interactive button message sent successfully" });
        }

        return StatusCode(500, new { success = false, message = "Failed to send interactive button message" });
    }

    /// <summary>
    /// Send an interactive list message via WhatsApp
    /// </summary>
    /// <param name="request">Interactive list message details</param>
    /// <returns>Success response with message status</returns>
    /// <response code="200">Interactive list message sent successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Failed to send interactive list message</response>
    [HttpPost("send-interactive-list")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SendInteractiveListMessage([FromBody] SendInteractiveListRequest request)
    {
        if (string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.BodyText) || request.Sections == null || !request.Sections.Any())
        {
            return BadRequest("To, BodyText, and Sections are required");
        }

        var success = await _whatsAppService.SendInteractiveListMessageAsync(request.To, request.BodyText, request.Sections, request.ButtonText, request.HeaderText, request.FooterText);
        
        if (success)
        {
            return Ok(new { success = true, message = "Interactive list message sent successfully" });
        }

        return StatusCode(500, new { success = false, message = "Failed to send interactive list message" });
    }

    private async Task ProcessWebhookEvent(WebhookEvent webhookEvent)
    {
        foreach (var entry in webhookEvent.Entry)
        {
            foreach (var change in entry.Changes)
            {
                if (change.Field == "messages")
                {
                    await ProcessMessagesChange(change.Value);
                }
            }
        }
    }

    private async Task ProcessMessagesChange(Value value)
    {
        // Process incoming messages
        if (value.Messages != null)
        {
            foreach (var message in value.Messages)
            {
                await ProcessIncomingMessage(message, value.Metadata);
            }
        }

        // Process message statuses
        if (value.Statuses != null)
        {
            foreach (var status in value.Statuses)
            {
                ProcessMessageStatus(status);
            }
        }
    }

    private async Task ProcessIncomingMessage(Message message, Metadata metadata)
    {
        _logger.LogInformation("Received message from {From}: Type={Type}, ID={Id}", message.From, message.Type, message.Id);

        // Echo back different message types as examples
        switch (message.Type.ToLower())
        {
            case "text":
                if (message.Text != null)
                {
                    var response = $"Echo: {message.Text.Body}";
                    await _whatsAppService.SendTextMessageAsync(message.From, response);
                }
                break;

            case "image":
                if (message.Image != null)
                {
                    await _whatsAppService.SendTextMessageAsync(message.From, "Thanks for the image!");
                    
                    // Example: Download and process the image
                    var imageUrl = await _whatsAppService.GetMediaUrlAsync(message.Image.Id);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        var imageData = await _whatsAppService.DownloadMediaAsync(imageUrl);
                        if (imageData != null)
                        {
                            _logger.LogInformation("Downloaded image: {Size} bytes", imageData.Length);
                        }
                    }
                }
                break;

            case "document":
                if (message.Document != null)
                {
                    await _whatsAppService.SendTextMessageAsync(message.From, $"Received document: {message.Document.Filename ?? "Unknown"}");
                }
                break;

            case "audio":
                await _whatsAppService.SendTextMessageAsync(message.From, "Thanks for the audio message!");
                break;

            case "video":
                await _whatsAppService.SendTextMessageAsync(message.From, "Thanks for the video!");
                break;

            case "location":
                if (message.Location != null)
                {
                    await _whatsAppService.SendTextMessageAsync(message.From, 
                        $"Thanks for sharing your location: {message.Location.Latitude}, {message.Location.Longitude}");
                }
                break;

            case "interactive":
                if (message.Interactive?.ButtonReply != null)
                {
                    await _whatsAppService.SendTextMessageAsync(message.From, 
                        $"You clicked button: {message.Interactive.ButtonReply.Title}");
                }
                else if (message.Interactive?.ListReply != null)
                {
                    await _whatsAppService.SendTextMessageAsync(message.From, 
                        $"You selected: {message.Interactive.ListReply.Title}");
                }
                break;

            default:
                await _whatsAppService.SendTextMessageAsync(message.From, $"Received message of type: {message.Type}");
                break;
        }
    }

    private void ProcessMessageStatus(Status status)
    {
        _logger.LogInformation("Message status update: ID={Id}, Status={Status}, Recipient={Recipient}", 
            status.Id, status.StatusValue, status.RecipientId);
    }

    private static bool VerifySignature(string body, string signature, string appSecret)
    {
        if (!signature.StartsWith("sha256="))
            return false;

        var expectedSignature = signature[7..]; // Remove "sha256=" prefix
        
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        var computedSignature = Convert.ToHexString(computedHash).ToLower();

        return expectedSignature == computedSignature;
    }
}

// Request models for API endpoints

/// <summary>
/// Request model for sending text messages
/// </summary>
public class SendTextRequest
{
    /// <summary>
    /// WhatsApp phone number to send message to (format: country code + phone number, no + sign)
    /// </summary>
    /// <example>1234567890</example>
    [Required]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Text message content
    /// </summary>
    /// <example>Hello from WhatsApp API!</example>
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Whether to show URL preview for links in the message
    /// </summary>
    /// <example>false</example>
    public bool PreviewUrl { get; set; } = false;
}

/// <summary>
/// Request model for sending image messages
/// </summary>
public class SendImageRequest
{
    /// <summary>
    /// WhatsApp phone number to send image to
    /// </summary>
    /// <example>1234567890</example>
    [Required]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// URL of the image to send
    /// </summary>
    /// <example>https://example.com/image.jpg</example>
    [Required]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional caption for the image
    /// </summary>
    /// <example>Check out this image!</example>
    public string? Caption { get; set; }
}

/// <summary>
/// Request model for sending document messages
/// </summary>
public class SendDocumentRequest
{
    /// <summary>
    /// WhatsApp phone number to send document to
    /// </summary>
    /// <example>1234567890</example>
    [Required]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// URL of the document to send
    /// </summary>
    /// <example>https://example.com/document.pdf</example>
    [Required]
    public string DocumentUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional filename for the document
    /// </summary>
    /// <example>important-document.pdf</example>
    public string? Filename { get; set; }

    /// <summary>
    /// Optional caption for the document
    /// </summary>
    /// <example>Please review this document</example>
    public string? Caption { get; set; }
}

/// <summary>
/// Request model for sending location messages
/// </summary>
public class SendLocationRequest
{
    /// <summary>
    /// WhatsApp phone number to send location to
    /// </summary>
    /// <example>1234567890</example>
    [Required]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Latitude coordinate
    /// </summary>
    /// <example>37.7749</example>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate
    /// </summary>
    /// <example>-122.4194</example>
    public double Longitude { get; set; }

    /// <summary>
    /// Optional name for the location
    /// </summary>
    /// <example>San Francisco</example>
    public string? Name { get; set; }

    /// <summary>
    /// Optional address for the location
    /// </summary>
    /// <example>San Francisco, CA, USA</example>
    public string? Address { get; set; }
}

/// <summary>
/// Request model for sending interactive button messages
/// </summary>
public class SendInteractiveButtonRequest
{
    /// <summary>
    /// WhatsApp phone number to send message to
    /// </summary>
    /// <example>1234567890</example>
    [Required]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Main body text of the message
    /// </summary>
    /// <example>Please choose an option:</example>
    [Required]
    public string BodyText { get; set; } = string.Empty;

    /// <summary>
    /// List of interactive buttons (maximum 3)
    /// </summary>
    [Required]
    public List<InteractiveButton> Buttons { get; set; } = new();

    /// <summary>
    /// Optional header text
    /// </summary>
    /// <example>Menu Selection</example>
    public string? HeaderText { get; set; }

    /// <summary>
    /// Optional footer text
    /// </summary>
    /// <example>Choose wisely!</example>
    public string? FooterText { get; set; }
}

/// <summary>
/// Request model for sending interactive list messages
/// </summary>
public class SendInteractiveListRequest
{
    /// <summary>
    /// WhatsApp phone number to send message to
    /// </summary>
    /// <example>1234567890</example>
    [Required]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Main body text of the message
    /// </summary>
    /// <example>Please select from the following options:</example>
    [Required]
    public string BodyText { get; set; } = string.Empty;

    /// <summary>
    /// List of sections containing interactive options
    /// </summary>
    [Required]
    public List<InteractiveSection> Sections { get; set; } = new();

    /// <summary>
    /// Text displayed on the list button
    /// </summary>
    /// <example>Select Option</example>
    public string ButtonText { get; set; } = "Options";

    /// <summary>
    /// Optional header text
    /// </summary>
    /// <example>Available Options</example>
    public string? HeaderText { get; set; }

    /// <summary>
    /// Optional footer text
    /// </summary>
    /// <example>Select one option</example>
    public string? FooterText { get; set; }
}