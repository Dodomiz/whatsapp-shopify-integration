using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WhatsAppIntegration.Services;
using WhatsAppIntegration.Models;

namespace WhatsAppIntegration.Tests.Integration;

public class WebhookIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WebhookIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace WhatsApp service with mock for integration tests
                var descriptors = services.Where(d => d.ServiceType == typeof(IWhatsAppService)).ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                var mockWhatsAppService = new Mock<IWhatsAppService>();
                mockWhatsAppService.Setup(x => x.SendTextMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(true);
                mockWhatsAppService.Setup(x => x.GetMediaUrlAsync(It.IsAny<string>()))
                    .ReturnsAsync("https://example.com/media.jpg");
                mockWhatsAppService.Setup(x => x.DownloadMediaAsync(It.IsAny<string>()))
                    .ReturnsAsync("fake media data"u8.ToArray());

                services.AddScoped<IWhatsAppService>(_ => mockWhatsAppService.Object);

                // Configure test WhatsApp settings
                services.Configure<WhatsAppConfig>(config =>
                {
                    config.WebhookVerifyToken = "test_verify_token";
                    config.AppSecret = "test_app_secret";
                    config.PhoneNumberId = "test_phone_number";
                    config.AccessToken = "test_access_token";
                });
            });
        });

        _client = _factory.CreateClient();
    }

    #region Webhook Verification Tests

    [Fact]
    public async Task GET_WebhookVerification_WithValidToken_ShouldReturnChallenge()
    {
        // Arrange
        var mode = "subscribe";
        var challenge = "test_challenge_123";
        var verifyToken = "test_verify_token";

        // Act
        var response = await _client.GetAsync($"/api/whatsapp/webhook?hub.mode={mode}&hub.challenge={challenge}&hub.verify_token={verifyToken}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be(challenge);
    }

    [Fact]
    public async Task GET_WebhookVerification_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var mode = "subscribe";
        var challenge = "test_challenge_123";
        var verifyToken = "invalid_token";

        // Act
        var response = await _client.GetAsync($"/api/whatsapp/webhook?hub.mode={mode}&hub.challenge={challenge}&hub.verify_token={verifyToken}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_WebhookVerification_WithMissingParameters_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/whatsapp/webhook");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Webhook Message Processing Tests

    [Fact]
    public async Task POST_Webhook_WithTextMessage_ShouldProcessSuccessfully()
    {
        // Arrange
        var webhookPayload = new
        {
            @object = "whatsapp_business_account",
            entry = new[]
            {
                new
                {
                    id = "123456789",
                    changes = new[]
                    {
                        new
                        {
                            value = new
                            {
                                messaging_product = "whatsapp",
                                metadata = new
                                {
                                    display_phone_number = "15551234567",
                                    phone_number_id = "987654321"
                                },
                                messages = new[]
                                {
                                    new
                                    {
                                        from = "1234567890",
                                        id = "wamid.123",
                                        timestamp = "1234567890",
                                        type = "text",
                                        text = new
                                        {
                                            body = "Hello World"
                                        }
                                    }
                                }
                            },
                            field = "messages"
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(webhookPayload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Add signature header
        var signature = GenerateSignature(json, "test_app_secret");
        content.Headers.Add("X-Hub-Signature-256", signature);

        // Act
        var response = await _client.PostAsync("/api/whatsapp/webhook", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_Webhook_WithInvalidSignature_ShouldReturnUnauthorized()
    {
        // Arrange
        var webhookPayload = new { @object = "whatsapp_business_account", entry = Array.Empty<object>() };
        var json = JsonSerializer.Serialize(webhookPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Add invalid signature
        content.Headers.Add("X-Hub-Signature-256", "sha256=invalid_signature");

        // Act
        var response = await _client.PostAsync("/api/whatsapp/webhook", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_Webhook_WithInvalidJson_ShouldReturnBadRequest()
    {
        // Arrange
        var content = new StringContent("invalid json", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/whatsapp/webhook", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Invalid JSON");
    }

    [Fact]
    public async Task POST_Webhook_WithEmptyBody_ShouldReturnBadRequest()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/whatsapp/webhook", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Empty body");
    }

    #endregion

    #region Message Sending Integration Tests

    [Fact]
    public async Task POST_SendTextMessage_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new
        {
            to = "1234567890",
            message = "Hello World",
            previewUrl = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/whatsapp/send-text", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
        
        responseData.Should().ContainKey("success");
        responseData["success"].Should().Be(true);
    }

    [Fact]
    public async Task POST_SendTextMessage_WithMissingData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            to = "",
            message = "Hello World"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/whatsapp/send-text", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_SendImageMessage_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new
        {
            to = "1234567890",
            imageUrl = "https://example.com/image.jpg",
            caption = "Test image"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/whatsapp/send-image", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
        
        responseData.Should().ContainKey("success");
        responseData["success"].Should().Be(true);
    }

    [Fact]
    public async Task POST_SendInteractiveButtons_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new
        {
            to = "1234567890",
            bodyText = "Choose an option:",
            buttons = new[]
            {
                new
                {
                    type = "reply",
                    reply = new
                    {
                        id = "btn1",
                        title = "Option 1"
                    }
                }
            },
            headerText = "Menu",
            footerText = "Choose wisely"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/whatsapp/send-interactive-buttons", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
        
        responseData.Should().ContainKey("success");
        responseData["success"].Should().Be(true);
    }

    #endregion

    #region Complex Webhook Scenarios

    [Fact]
    public async Task POST_Webhook_WithMultipleMessages_ShouldProcessAll()
    {
        // Arrange
        var webhookJson = """
        {
            "object": "whatsapp_business_account",
            "entry": [
                {
                    "id": "123456789",
                    "changes": [
                        {
                            "value": {
                                "messaging_product": "whatsapp",
                                "metadata": {
                                    "display_phone_number": "15551234567",
                                    "phone_number_id": "987654321"
                                },
                                "messages": [
                                    {
                                        "from": "1234567890",
                                        "id": "wamid.123",
                                        "timestamp": "1234567890",
                                        "type": "text",
                                        "text": { "body": "First message" }
                                    },
                                    {
                                        "from": "0987654321",
                                        "id": "wamid.456",
                                        "timestamp": "1234567891",
                                        "type": "text",
                                        "text": { "body": "Second message" }
                                    }
                                ]
                            },
                            "field": "messages"
                        }
                    ]
                }
            ]
        }
        """;

        var content = new StringContent(webhookJson, Encoding.UTF8, "application/json");
        
        var signature = GenerateSignature(webhookJson, "test_app_secret");
        content.Headers.Add("X-Hub-Signature-256", signature);

        // Act
        var response = await _client.PostAsync("/api/whatsapp/webhook", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_Webhook_WithStatusUpdate_ShouldProcessSuccessfully()
    {
        // Arrange
        var webhookJson = """
        {
            "object": "whatsapp_business_account",
            "entry": [
                {
                    "id": "123456789",
                    "changes": [
                        {
                            "value": {
                                "messaging_product": "whatsapp",
                                "metadata": {
                                    "display_phone_number": "15551234567",
                                    "phone_number_id": "987654321"
                                },
                                "statuses": [
                                    {
                                        "id": "wamid.123",
                                        "status": "delivered",
                                        "timestamp": "1234567890",
                                        "recipient_id": "1234567890"
                                    }
                                ]
                            },
                            "field": "messages"
                        }
                    ]
                }
            ]
        }
        """;

        var content = new StringContent(webhookJson, Encoding.UTF8, "application/json");
        
        var signature = GenerateSignature(webhookJson, "test_app_secret");
        content.Headers.Add("X-Hub-Signature-256", signature);

        // Act
        var response = await _client.PostAsync("/api/whatsapp/webhook", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region API Documentation Tests

    [Fact]
    public async Task GET_SwaggerUI_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/api/docs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("WhatsApp Integration API");
    }

    [Fact]
    public async Task GET_SwaggerJson_ShouldReturnApiSpec()
    {
        // Act
        var response = await _client.GetAsync("/api/docs/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        var apiSpec = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
        apiSpec.Should().ContainKey("openapi");
        apiSpec.Should().ContainKey("info");
        apiSpec.Should().ContainKey("paths");
    }

    #endregion

    #region Helper Methods

    private static string GenerateSignature(string body, string appSecret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        return $"sha256={Convert.ToHexString(hash).ToLower()}";
    }

    #endregion
}