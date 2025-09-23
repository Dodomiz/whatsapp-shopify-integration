using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WhatsAppIntegration.Controllers;
using WhatsAppIntegration.Services;
using WhatsAppIntegration.Models;

namespace WhatsAppIntegration.Tests.Controllers;

public class WhatsAppControllerTests
{
    private readonly Mock<IWhatsAppService> _whatsAppServiceMock;
    private readonly Mock<ILogger<WhatsAppController>> _loggerMock;
    private readonly Mock<IOptions<WhatsAppConfig>> _configMock;
    private readonly WhatsAppConfig _config;
    private readonly WhatsAppController _controller;

    public WhatsAppControllerTests()
    {
        _whatsAppServiceMock = new Mock<IWhatsAppService>();
        _loggerMock = new Mock<ILogger<WhatsAppController>>();
        _configMock = new Mock<IOptions<WhatsAppConfig>>();
        
        _config = new WhatsAppConfig
        {
            WebhookVerifyToken = "test_verify_token",
            AppSecret = "test_app_secret"
        };
        
        _configMock.Setup(x => x.Value).Returns(_config);
        _controller = new WhatsAppController(_whatsAppServiceMock.Object, _configMock.Object, _loggerMock.Object);
    }

    #region Webhook Verification Tests

    [Fact]
    public void VerifyWebhook_WithValidToken_ShouldReturnChallenge()
    {
        // Arrange
        var mode = "subscribe";
        var challenge = "test_challenge_123";
        var verifyToken = "test_verify_token";

        // Act
        var result = _controller.VerifyWebhook(mode, challenge, verifyToken);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(challenge);
    }

    [Fact]
    public void VerifyWebhook_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var mode = "subscribe";
        var challenge = "test_challenge_123";
        var verifyToken = "invalid_token";

        // Act
        var result = _controller.VerifyWebhook(mode, challenge, verifyToken);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public void VerifyWebhook_WithInvalidMode_ShouldReturnUnauthorized()
    {
        // Arrange
        var mode = "invalid_mode";
        var challenge = "test_challenge_123";
        var verifyToken = "test_verify_token";

        // Act
        var result = _controller.VerifyWebhook(mode, challenge, verifyToken);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    #endregion

    #region Webhook Handling Tests

    [Fact]
    public async Task HandleWebhook_WithValidWebhookEvent_ShouldReturnOk()
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
                                        "text": {
                                            "body": "Hello"
                                        }
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

        _whatsAppServiceMock.Setup(x => x.SendTextMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(true);

        SetupRequestBody(webhookJson);

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        result.Should().BeOfType<OkResult>();
        
        _whatsAppServiceMock.Verify(x => x.SendTextMessageAsync("1234567890", "Echo: Hello", false), Times.Once);
    }

    [Fact]
    public async Task HandleWebhook_WithEmptyBody_ShouldReturnBadRequest()
    {
        // Arrange
        SetupRequestBody("");

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Empty body");
    }

    [Fact]
    public async Task HandleWebhook_WithInvalidJson_ShouldReturnBadRequest()
    {
        // Arrange
        SetupRequestBody("invalid json");

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Invalid JSON");
    }

    [Fact]
    public async Task HandleWebhook_WithValidSignature_ShouldProcessWebhook()
    {
        // Arrange
        var webhookJson = """{"object": "whatsapp_business_account", "entry": []}""";
        var signature = GenerateSignature(webhookJson, _config.AppSecret);
        
        SetupRequestBody(webhookJson);
        SetupRequestHeaders("X-Hub-Signature-256", signature);

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task HandleWebhook_WithInvalidSignature_ShouldReturnUnauthorized()
    {
        // Arrange
        var webhookJson = """{"object": "whatsapp_business_account", "entry": []}""";
        var invalidSignature = "sha256=invalid_signature";
        
        SetupRequestBody(webhookJson);
        SetupRequestHeaders("X-Hub-Signature-256", invalidSignature);

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    #endregion

    #region Send Message Tests

    [Fact]
    public async Task SendTextMessage_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new SendTextRequest
        {
            To = "1234567890",
            Message = "Hello World",
            PreviewUrl = true
        };

        _whatsAppServiceMock.Setup(x => x.SendTextMessageAsync("1234567890", "Hello World", true))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendTextMessage(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeEquivalentTo(new { success = true, message = "Message sent successfully" });
    }

    [Fact]
    public async Task SendTextMessage_WithMissingTo_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new SendTextRequest
        {
            To = "",
            Message = "Hello World"
        };

        // Act
        var result = await _controller.SendTextMessage(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("To and Message are required");
    }

    [Fact]
    public async Task SendTextMessage_WithServiceFailure_ShouldReturnServerError()
    {
        // Arrange
        var request = new SendTextRequest
        {
            To = "1234567890",
            Message = "Hello World"
        };

        _whatsAppServiceMock.Setup(x => x.SendTextMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SendTextMessage(request);

        // Assert
        var serverErrorResult = result.Should().BeOfType<ObjectResult>().Subject;
        serverErrorResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SendImageMessage_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new SendImageRequest
        {
            To = "1234567890",
            ImageUrl = "https://example.com/image.jpg",
            Caption = "Test image"
        };

        _whatsAppServiceMock.Setup(x => x.SendImageMessageAsync("1234567890", "https://example.com/image.jpg", "Test image"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendImageMessage(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeEquivalentTo(new { success = true, message = "Image sent successfully" });
    }

    [Fact]
    public async Task SendDocumentMessage_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new SendDocumentRequest
        {
            To = "1234567890",
            DocumentUrl = "https://example.com/doc.pdf",
            Filename = "document.pdf",
            Caption = "Test document"
        };

        _whatsAppServiceMock.Setup(x => x.SendDocumentMessageAsync("1234567890", "https://example.com/doc.pdf", "document.pdf", "Test document"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendDocumentMessage(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeEquivalentTo(new { success = true, message = "Document sent successfully" });
    }

    [Fact]
    public async Task SendLocationMessage_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new SendLocationRequest
        {
            To = "1234567890",
            Latitude = 37.7749,
            Longitude = -122.4194,
            Name = "San Francisco",
            Address = "CA, USA"
        };

        _whatsAppServiceMock.Setup(x => x.SendLocationMessageAsync("1234567890", 37.7749, -122.4194, "San Francisco", "CA, USA"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendLocationMessage(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeEquivalentTo(new { success = true, message = "Location sent successfully" });
    }

    [Fact]
    public async Task SendInteractiveButtonMessage_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new SendInteractiveButtonRequest
        {
            To = "1234567890",
            BodyText = "Choose an option:",
            Buttons = new List<InteractiveButton>
            {
                new() { Reply = new InteractiveButtonReply { Id = "btn1", Title = "Option 1" } }
            },
            HeaderText = "Header",
            FooterText = "Footer"
        };

        _whatsAppServiceMock.Setup(x => x.SendInteractiveButtonMessageAsync(
                "1234567890", 
                "Choose an option:", 
                It.IsAny<List<InteractiveButton>>(), 
                "Header", 
                "Footer"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendInteractiveButtonMessage(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeEquivalentTo(new { success = true, message = "Interactive button message sent successfully" });
    }

    [Fact]
    public async Task SendInteractiveListMessage_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new SendInteractiveListRequest
        {
            To = "1234567890",
            BodyText = "Choose from list:",
            Sections = new List<InteractiveSection>
            {
                new()
                {
                    Title = "Section 1",
                    Rows = new List<InteractiveRow>
                    {
                        new() { Id = "row1", Title = "Row 1" }
                    }
                }
            },
            ButtonText = "Select",
            HeaderText = "Header",
            FooterText = "Footer"
        };

        _whatsAppServiceMock.Setup(x => x.SendInteractiveListMessageAsync(
                "1234567890", 
                "Choose from list:", 
                It.IsAny<List<InteractiveSection>>(), 
                "Select", 
                "Header", 
                "Footer"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendInteractiveListMessage(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeEquivalentTo(new { success = true, message = "Interactive list message sent successfully" });
    }

    [Theory]
    [InlineData("", "Valid body", false)]
    [InlineData("1234567890", "", false)]
    [InlineData("1234567890", "Valid body", true)]
    public async Task SendInteractiveButtonMessage_InputValidation(string to, string bodyText, bool shouldSucceed)
    {
        // Arrange
        var request = new SendInteractiveButtonRequest
        {
            To = to,
            BodyText = bodyText,
            Buttons = shouldSucceed ? new List<InteractiveButton>
            {
                new() { Reply = new InteractiveButtonReply { Id = "btn1", Title = "Option 1" } }
            } : new List<InteractiveButton>()
        };

        if (shouldSucceed)
        {
            _whatsAppServiceMock.Setup(x => x.SendInteractiveButtonMessageAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<List<InteractiveButton>>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>()))
                .ReturnsAsync(true);
        }

        // Act
        var result = await _controller.SendInteractiveButtonMessage(request);

        // Assert
        if (shouldSucceed)
        {
            result.Should().BeOfType<OkObjectResult>();
        }
        else
        {
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("To, BodyText, and Buttons are required");
        }
    }

    #endregion

    #region Message Processing Tests

    [Fact]
    public async Task HandleWebhook_WithImageMessage_ShouldProcessCorrectly()
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
                                        "type": "image",
                                        "image": {
                                            "id": "media123",
                                            "mime_type": "image/jpeg",
                                            "sha256": "abc123"
                                        }
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

        _whatsAppServiceMock.Setup(x => x.SendTextMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(true);
        _whatsAppServiceMock.Setup(x => x.GetMediaUrlAsync("media123"))
            .ReturnsAsync("https://example.com/media.jpg");
        _whatsAppServiceMock.Setup(x => x.DownloadMediaAsync("https://example.com/media.jpg"))
            .ReturnsAsync("fake image data"u8.ToArray());

        SetupRequestBody(webhookJson);

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        result.Should().BeOfType<OkResult>();
        
        _whatsAppServiceMock.Verify(x => x.SendTextMessageAsync("1234567890", "Thanks for the image!", false), Times.Once);
        _whatsAppServiceMock.Verify(x => x.GetMediaUrlAsync("media123"), Times.Once);
        _whatsAppServiceMock.Verify(x => x.DownloadMediaAsync("https://example.com/media.jpg"), Times.Once);
    }

    [Fact]
    public async Task HandleWebhook_WithButtonReply_ShouldProcessCorrectly()
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
                                        "type": "interactive",
                                        "interactive": {
                                            "type": "button_reply",
                                            "button_reply": {
                                                "id": "btn1",
                                                "title": "Option 1"
                                            }
                                        }
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

        _whatsAppServiceMock.Setup(x => x.SendTextMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(true);

        SetupRequestBody(webhookJson);

        // Act
        var result = await _controller.HandleWebhook();

        // Assert
        result.Should().BeOfType<OkResult>();
        
        _whatsAppServiceMock.Verify(x => x.SendTextMessageAsync("1234567890", "You clicked button: Option 1", false), Times.Once);
    }

    #endregion

    #region Helper Methods

    private void SetupRequestBody(string body)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = stream;
        httpContext.Request.ContentLength = body.Length;
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private void SetupRequestHeaders(string headerName, string headerValue)
    {
        _controller.ControllerContext.HttpContext.Request.Headers[headerName] = headerValue;
    }

    private static string GenerateSignature(string body, string appSecret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        return $"sha256={Convert.ToHexString(hash).ToLower()}";
    }

    #endregion
}