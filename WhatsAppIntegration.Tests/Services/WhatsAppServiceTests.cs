using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using WhatsAppIntegration.Models;
using WhatsAppIntegration.Services;

namespace WhatsAppIntegration.Tests.Services;

public class WhatsAppServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<ILogger<WhatsAppService>> _loggerMock;
    private readonly Mock<IOptions<WhatsAppConfig>> _configMock;
    private readonly WhatsAppConfig _config;
    private readonly HttpClient _httpClient;
    private readonly WhatsAppService _service;

    public WhatsAppServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _loggerMock = new Mock<ILogger<WhatsAppService>>();
        _configMock = new Mock<IOptions<WhatsAppConfig>>();
        
        _config = new WhatsAppConfig
        {
            AccessToken = "test_access_token",
            PhoneNumberId = "123456789",
            BaseUrl = "https://graph.facebook.com",
            ApiVersion = "v18.0"
        };
        
        _configMock.Setup(x => x.Value).Returns(_config);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _service = new WhatsAppService(_httpClient, _configMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendTextMessageAsync_WithValidInput_ShouldReturnTrue()
    {
        // Arrange
        var responseContent = """{"messages": [{"id": "wamid.test123"}]}""";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.SendTextMessageAsync("1234567890", "Hello World", true);

        // Assert
        result.Should().BeTrue();
        
        _httpMessageHandlerMock
            .Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains($"{_config.BaseUrl}/{_config.ApiVersion}/{_config.PhoneNumberId}/messages") &&
                    req.Headers.Authorization!.Parameter == _config.AccessToken),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendTextMessageAsync_WithHttpError_ShouldReturnFalse()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request", Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.SendTextMessageAsync("1234567890", "Hello World", false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendTextMessageAsync_WithException_ShouldReturnFalse()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.SendTextMessageAsync("1234567890", "Hello World", false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendImageMessageAsync_WithValidInput_ShouldReturnTrue()
    {
        // Arrange
        var responseContent = """{"messages": [{"id": "wamid.test123"}]}""";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.SendImageMessageAsync("1234567890", "https://example.com/image.jpg", "Test caption");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendDocumentMessageAsync_WithValidInput_ShouldReturnTrue()
    {
        // Arrange
        var responseContent = """{"messages": [{"id": "wamid.test123"}]}""";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.SendDocumentMessageAsync("1234567890", "https://example.com/doc.pdf", "document.pdf", "Test document");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendAudioMessageAsync_WithValidInput_ShouldReturnTrue()
    {
        // Arrange
        var responseContent = """{"messages": [{"id": "wamid.test123"}]}""";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.SendAudioMessageAsync("1234567890", "https://example.com/audio.mp3");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendVideoMessageAsync_WithValidInput_ShouldReturnTrue()
    {
        // Arrange
        var responseContent = """{"messages": [{"id": "wamid.test123"}]}""";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.SendVideoMessageAsync("1234567890", "https://example.com/video.mp4", "Test video");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendLocationMessageAsync_WithValidInput_ShouldReturnTrue()
    {
        // Arrange
        var responseContent = """{"messages": [{"id": "wamid.test123"}]}""";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.SendLocationMessageAsync("1234567890", 37.7749, -122.4194, "San Francisco", "CA, USA");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendInteractiveButtonMessageAsync_WithValidInput_ShouldReturnTrue()
    {
        // Arrange
        var buttons = new List<InteractiveButton>
        {
            new() { Reply = new InteractiveButtonReply { Id = "btn1", Title = "Option 1" } },
            new() { Reply = new InteractiveButtonReply { Id = "btn2", Title = "Option 2" } }
        };

        var responseContent = """{"messages": [{"id": "wamid.test123"}]}""";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.SendInteractiveButtonMessageAsync("1234567890", "Choose an option:", buttons, "Header", "Footer");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendInteractiveListMessageAsync_WithValidInput_ShouldReturnTrue()
    {
        // Arrange
        var sections = new List<InteractiveSection>
        {
            new()
            {
                Title = "Section 1",
                Rows = new List<InteractiveRow>
                {
                    new() { Id = "row1", Title = "Row 1", Description = "Description 1" },
                    new() { Id = "row2", Title = "Row 2", Description = "Description 2" }
                }
            }
        };

        var responseContent = """{"messages": [{"id": "wamid.test123"}]}""";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.SendInteractiveListMessageAsync("1234567890", "Choose from list:", sections, "Select", "Header", "Footer");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetMediaUrlAsync_WithValidMediaId_ShouldReturnUrl()
    {
        // Arrange
        var mediaId = "media123";
        var expectedUrl = "https://example.com/media/file.jpg";
        var responseContent = $$"""{ "url": "{{expectedUrl}}" }""";
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"{_config.BaseUrl}/{_config.ApiVersion}/{mediaId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetMediaUrlAsync(mediaId);

        // Assert
        result.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task GetMediaUrlAsync_WithHttpError_ShouldReturnNull()
    {
        // Arrange
        var mediaId = "media123";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.GetMediaUrlAsync(mediaId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DownloadMediaAsync_WithValidUrl_ShouldReturnByteArray()
    {
        // Arrange
        var mediaUrl = "https://example.com/media/file.jpg";
        var expectedData = "fake image data"u8.ToArray();
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(expectedData)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == mediaUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.DownloadMediaAsync(mediaUrl);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedData);
    }

    [Fact]
    public async Task DownloadMediaAsync_WithHttpError_ShouldReturnNull()
    {
        // Arrange
        var mediaUrl = "https://example.com/media/file.jpg";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.DownloadMediaAsync(mediaUrl);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SendTextMessageAsync_ShouldSendCorrectJsonPayload()
    {
        // Arrange
        var to = "1234567890";
        var message = "Test message";
        var previewUrl = true;
        
        HttpRequestMessage? capturedRequest = null;
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"messages": [{"id": "wamid.test123"}]}""")
            });

        // Act
        await _service.SendTextMessageAsync(to, message, previewUrl);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Content.Should().NotBeNull();
        
        var requestBody = await capturedRequest.Content!.ReadAsStringAsync();
        var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
        
        requestData.Should().ContainKey("messagingProduct");
        requestData["messagingProduct"].Should().Be("whatsapp");
        requestData["to"].Should().Be(to);
        requestData["type"].Should().Be("text");
        
        var textElement = (JsonElement)requestData["text"];
        var textData = JsonSerializer.Deserialize<Dictionary<string, object>>(textElement.GetRawText());
        textData!["body"].Should().Be(message);
        textData["previewUrl"].Should().Be(previewUrl);
    }

    [Theory]
    [InlineData("", "Valid message", false)]
    [InlineData("1234567890", "", false)]
    [InlineData("1234567890", "Valid message", true)]
    public async Task SendTextMessageAsync_InputValidation(string to, string message, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"messages": [{"id": "wamid.test123"}]}""")
                });
        }

        // Act
        var result = await _service.SendTextMessageAsync(to, message, false);

        // Assert
        if (shouldSucceed)
        {
            result.Should().BeTrue();
        }
        else
        {
            // Should still attempt to send (validation happens in controller)
            // but will likely fail due to API response
            result.Should().BeFalse();
        }
    }

    [Fact]
    public void Constructor_ShouldSetAuthorizationHeader()
    {
        // Arrange & Act
        var httpClient = new HttpClient();
        var service = new WhatsAppService(httpClient, _configMock.Object, _loggerMock.Object);

        // Assert
        httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be(_config.AccessToken);
    }
}