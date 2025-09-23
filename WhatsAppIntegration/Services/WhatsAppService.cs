using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WhatsAppIntegration.Models;

namespace WhatsAppIntegration.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppConfig _config;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public WhatsAppService(HttpClient httpClient, IOptions<WhatsAppConfig> config, ILogger<WhatsAppService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.AccessToken);
    }

    public async Task<bool> SendTextMessageAsync(string to, string message, bool previewUrl = false)
    {
        var outgoingMessage = new OutgoingMessage
        {
            To = to,
            Type = "text",
            Text = new OutgoingTextMessage
            {
                Body = message,
                PreviewUrl = previewUrl
            }
        };

        return await SendMessageAsync(outgoingMessage);
    }

    public async Task<bool> SendImageMessageAsync(string to, string imageUrl, string? caption = null)
    {
        var outgoingMessage = new OutgoingMessage
        {
            To = to,
            Type = "image",
            Image = new OutgoingMediaMessage
            {
                Link = imageUrl,
                Caption = caption
            }
        };

        return await SendMessageAsync(outgoingMessage);
    }

    public async Task<bool> SendDocumentMessageAsync(string to, string documentUrl, string? filename = null, string? caption = null)
    {
        var outgoingMessage = new OutgoingMessage
        {
            To = to,
            Type = "document",
            Document = new OutgoingMediaMessage
            {
                Link = documentUrl,
                Filename = filename,
                Caption = caption
            }
        };

        return await SendMessageAsync(outgoingMessage);
    }

    public async Task<bool> SendAudioMessageAsync(string to, string audioUrl)
    {
        var outgoingMessage = new OutgoingMessage
        {
            To = to,
            Type = "audio",
            Audio = new OutgoingMediaMessage
            {
                Link = audioUrl
            }
        };

        return await SendMessageAsync(outgoingMessage);
    }

    public async Task<bool> SendVideoMessageAsync(string to, string videoUrl, string? caption = null)
    {
        var outgoingMessage = new OutgoingMessage
        {
            To = to,
            Type = "video",
            Video = new OutgoingMediaMessage
            {
                Link = videoUrl,
                Caption = caption
            }
        };

        return await SendMessageAsync(outgoingMessage);
    }

    public async Task<bool> SendLocationMessageAsync(string to, double latitude, double longitude, string? name = null, string? address = null)
    {
        var outgoingMessage = new OutgoingMessage
        {
            To = to,
            Type = "location",
            Location = new OutgoingLocationMessage
            {
                Latitude = latitude,
                Longitude = longitude,
                Name = name,
                Address = address
            }
        };

        return await SendMessageAsync(outgoingMessage);
    }

    public async Task<bool> SendInteractiveButtonMessageAsync(string to, string bodyText, List<InteractiveButton> buttons, string? headerText = null, string? footerText = null)
    {
        var interactive = new OutgoingInteractiveMessage
        {
            Type = "button",
            Body = new InteractiveBody { Text = bodyText },
            Action = new InteractiveAction { Buttons = buttons }
        };

        if (!string.IsNullOrEmpty(headerText))
        {
            interactive.Header = new InteractiveHeader
            {
                Type = "text",
                Text = headerText
            };
        }

        if (!string.IsNullOrEmpty(footerText))
        {
            interactive.Footer = new InteractiveFooter { Text = footerText };
        }

        var outgoingMessage = new OutgoingMessage
        {
            To = to,
            Type = "interactive",
            Interactive = interactive
        };

        return await SendMessageAsync(outgoingMessage);
    }

    public async Task<bool> SendInteractiveListMessageAsync(string to, string bodyText, List<InteractiveSection> sections, string buttonText = "Options", string? headerText = null, string? footerText = null)
    {
        var interactive = new OutgoingInteractiveMessage
        {
            Type = "list",
            Body = new InteractiveBody { Text = bodyText },
            Action = new InteractiveAction { Sections = sections }
        };

        if (!string.IsNullOrEmpty(headerText))
        {
            interactive.Header = new InteractiveHeader
            {
                Type = "text",
                Text = headerText
            };
        }

        if (!string.IsNullOrEmpty(footerText))
        {
            interactive.Footer = new InteractiveFooter { Text = footerText };
        }

        var outgoingMessage = new OutgoingMessage
        {
            To = to,
            Type = "interactive",
            Interactive = interactive
        };

        return await SendMessageAsync(outgoingMessage);
    }

    public async Task<string?> GetMediaUrlAsync(string mediaId)
    {
        try
        {
            var url = $"{_config.BaseUrl}/{_config.ApiVersion}/{mediaId}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);
                
                if (document.RootElement.TryGetProperty("url", out var urlProperty))
                {
                    return urlProperty.GetString();
                }
            }

            _logger.LogWarning("Failed to get media URL for ID: {MediaId}. Status: {StatusCode}", mediaId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media URL for ID: {MediaId}", mediaId);
            return null;
        }
    }

    public async Task<byte[]?> DownloadMediaAsync(string mediaUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(mediaUrl);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            _logger.LogWarning("Failed to download media from URL: {MediaUrl}. Status: {StatusCode}", mediaUrl, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading media from URL: {MediaUrl}", mediaUrl);
            return null;
        }
    }

    private async Task<bool> SendMessageAsync(OutgoingMessage message)
    {
        try
        {
            var url = $"{_config.BaseUrl}/{_config.ApiVersion}/{_config.PhoneNumberId}/messages";
            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending WhatsApp message to {To}: {Json}", message.To, json);

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("WhatsApp message sent successfully to {To}", message.To);
                return true;
            }

            _logger.LogWarning("Failed to send WhatsApp message to {To}. Status: {StatusCode}, Response: {Response}", 
                message.To, response.StatusCode, responseContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message to {To}", message.To);
            return false;
        }
    }
}