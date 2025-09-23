using System.Text.Json;
using FluentAssertions;
using WhatsAppIntegration.Models;

namespace WhatsAppIntegration.Tests.Models;

public class WhatsAppModelsTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void WebhookEvent_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
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
                                }
                            },
                            "field": "messages"
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<WebhookEvent>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result.Object.Should().Be("whatsapp_business_account");
        result.Entry.Should().HaveCount(1);
        result.Entry[0].Id.Should().Be("123456789");
        result.Entry[0].Changes.Should().HaveCount(1);
        result.Entry[0].Changes[0].Field.Should().Be("messages");
        result.Entry[0].Changes[0].Value.MessagingProduct.Should().Be("whatsapp");
        result.Entry[0].Changes[0].Value.Metadata.DisplayPhoneNumber.Should().Be("15551234567");
        result.Entry[0].Changes[0].Value.Metadata.PhoneNumberId.Should().Be("987654321");
    }

    [Fact]
    public void TextMessage_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "messaging_product": "whatsapp",
            "metadata": {
                "display_phone_number": "15551234567",
                "phone_number_id": "987654321"
            },
            "contacts": [
                {
                    "profile": {
                        "name": "John Doe"
                    },
                    "wa_id": "1234567890"
                }
            ],
            "messages": [
                {
                    "from": "1234567890",
                    "id": "wamid.123",
                    "timestamp": "1234567890",
                    "type": "text",
                    "text": {
                        "body": "Hello World"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<WhatsAppIntegration.Models.Value>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result.MessagingProduct.Should().Be("whatsapp");
        result.Contacts.Should().HaveCount(1);
        result.Contacts![0].Profile.Name.Should().Be("John Doe");
        result.Contacts[0].WaId.Should().Be("1234567890");
        result.Messages.Should().HaveCount(1);
        result.Messages![0].From.Should().Be("1234567890");
        result.Messages[0].Type.Should().Be("text");
        result.Messages[0].Text!.Body.Should().Be("Hello World");
    }

    [Fact]
    public void ImageMessage_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
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
                        "sha256": "abc123",
                        "caption": "Nice photo!"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<WhatsAppIntegration.Models.Value>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result.Messages.Should().HaveCount(1);
        result.Messages![0].Type.Should().Be("image");
        result.Messages[0].Image.Should().NotBeNull();
        result.Messages[0].Image!.Id.Should().Be("media123");
        result.Messages[0].Image.MimeType.Should().Be("image/jpeg");
        result.Messages[0].Image.Sha256.Should().Be("abc123");
        result.Messages[0].Image.Caption.Should().Be("Nice photo!");
    }

    [Fact]
    public void LocationMessage_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
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
                    "type": "location",
                    "location": {
                        "latitude": 37.7749,
                        "longitude": -122.4194,
                        "name": "San Francisco",
                        "address": "San Francisco, CA, USA"
                    }
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<WhatsAppIntegration.Models.Value>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result.Messages.Should().HaveCount(1);
        result.Messages![0].Type.Should().Be("location");
        result.Messages[0].Location.Should().NotBeNull();
        result.Messages[0].Location!.Latitude.Should().Be(37.7749);
        result.Messages[0].Location.Longitude.Should().Be(-122.4194);
        result.Messages[0].Location.Name.Should().Be("San Francisco");
        result.Messages[0].Location.Address.Should().Be("San Francisco, CA, USA");
    }

    [Fact]
    public void InteractiveButtonReply_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
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
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<WhatsAppIntegration.Models.Value>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result.Messages.Should().HaveCount(1);
        result.Messages![0].Type.Should().Be("interactive");
        result.Messages[0].Interactive.Should().NotBeNull();
        result.Messages[0].Interactive!.Type.Should().Be("button_reply");
        result.Messages[0].Interactive.ButtonReply.Should().NotBeNull();
        result.Messages[0].Interactive.ButtonReply!.Id.Should().Be("btn1");
        result.Messages[0].Interactive.ButtonReply.Title.Should().Be("Option 1");
    }

    [Fact]
    public void StatusUpdate_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
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
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<WhatsAppIntegration.Models.Value>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result.Statuses.Should().HaveCount(1);
        result.Statuses![0].Id.Should().Be("wamid.123");
        result.Statuses[0].StatusValue.Should().Be("delivered");
        result.Statuses[0].Timestamp.Should().Be("1234567890");
        result.Statuses[0].RecipientId.Should().Be("1234567890");
    }

    [Fact]
    public void OutgoingTextMessage_ShouldSerializeCorrectly()
    {
        // Arrange
        var message = new OutgoingMessage
        {
            To = "1234567890",
            Type = "text",
            Text = new OutgoingTextMessage
            {
                Body = "Hello World!",
                PreviewUrl = true
            }
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

        // Assert
        result.Should().NotBeNull();
        result["messagingProduct"].Should().Be("whatsapp");
        result["to"].Should().Be("1234567890");
        result["type"].Should().Be("text");
        
        var textElement = (JsonElement)result["text"];
        var textDict = JsonSerializer.Deserialize<Dictionary<string, object>>(textElement.GetRawText());
        textDict!["body"].Should().Be("Hello World!");
        textDict["previewUrl"].Should().Be(true);
    }

    [Fact]
    public void OutgoingImageMessage_ShouldSerializeCorrectly()
    {
        // Arrange
        var message = new OutgoingMessage
        {
            To = "1234567890",
            Type = "image",
            Image = new OutgoingMediaMessage
            {
                Link = "https://example.com/image.jpg",
                Caption = "Check this out!"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

        // Assert
        result.Should().NotBeNull();
        result["type"].Should().Be("image");
        
        var imageElement = (JsonElement)result["image"];
        var imageDict = JsonSerializer.Deserialize<Dictionary<string, object>>(imageElement.GetRawText());
        imageDict!["link"].Should().Be("https://example.com/image.jpg");
        imageDict["caption"].Should().Be("Check this out!");
    }

    [Fact]
    public void OutgoingInteractiveButtonMessage_ShouldSerializeCorrectly()
    {
        // Arrange
        var message = new OutgoingMessage
        {
            To = "1234567890",
            Type = "interactive",
            Interactive = new OutgoingInteractiveMessage
            {
                Type = "button",
                Body = new InteractiveBody { Text = "Choose an option:" },
                Action = new InteractiveAction
                {
                    Buttons = new List<InteractiveButton>
                    {
                        new() { Reply = new InteractiveButtonReply { Id = "btn1", Title = "Option 1" } },
                        new() { Reply = new InteractiveButtonReply { Id = "btn2", Title = "Option 2" } }
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

        // Assert
        result.Should().NotBeNull();
        result["type"].Should().Be("interactive");
        
        var interactiveElement = (JsonElement)result["interactive"];
        var interactiveDict = JsonSerializer.Deserialize<Dictionary<string, object>>(interactiveElement.GetRawText());
        interactiveDict!["type"].Should().Be("button");
        
        var bodyElement = (JsonElement)interactiveDict["body"];
        var bodyDict = JsonSerializer.Deserialize<Dictionary<string, object>>(bodyElement.GetRawText());
        bodyDict!["text"].Should().Be("Choose an option:");
        
        var actionElement = (JsonElement)interactiveDict["action"];
        var actionDict = JsonSerializer.Deserialize<Dictionary<string, object>>(actionElement.GetRawText());
        var buttonsElement = (JsonElement)actionDict!["buttons"];
        var buttons = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(buttonsElement.GetRawText());
        buttons.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("1234567890", true)]
    public void WhatsAppConfig_PhoneNumberValidation(string phoneNumber, bool expectedIsValid)
    {
        // Arrange
        var config = new WhatsAppConfig
        {
            PhoneNumberId = phoneNumber,
            AccessToken = "test_token",
            WebhookVerifyToken = "test_verify"
        };

        // Act & Assert
        var isValid = !string.IsNullOrWhiteSpace(config.PhoneNumberId);
        isValid.Should().Be(expectedIsValid);
    }

    [Fact]
    public void WhatsAppConfig_DefaultValues_ShouldBeSet()
    {
        // Arrange & Act
        var config = new WhatsAppConfig();

        // Assert
        config.ApiVersion.Should().Be("v18.0");
        config.BaseUrl.Should().Be("https://graph.facebook.com");
        config.AccessToken.Should().BeEmpty();
        config.WebhookVerifyToken.Should().BeEmpty();
        config.PhoneNumberId.Should().BeEmpty();
        config.BusinessAccountId.Should().BeEmpty();
        config.AppId.Should().BeEmpty();
        config.AppSecret.Should().BeEmpty();
    }
}