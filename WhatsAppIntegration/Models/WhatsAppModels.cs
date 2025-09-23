using System.Text.Json.Serialization;

namespace WhatsAppIntegration.Models;

public class WebhookEvent
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("entry")]
    public List<Entry> Entry { get; set; } = new();
}

public class Entry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("changes")]
    public List<Change> Changes { get; set; } = new();
}

public class Change
{
    [JsonPropertyName("value")]
    public Value Value { get; set; } = new();

    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;
}

public class Value
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; set; } = new();

    [JsonPropertyName("contacts")]
    public List<Contact>? Contacts { get; set; }

    [JsonPropertyName("messages")]
    public List<Message>? Messages { get; set; }

    [JsonPropertyName("statuses")]
    public List<Status>? Statuses { get; set; }
}

public class Metadata
{
    [JsonPropertyName("display_phone_number")]
    public string DisplayPhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("phone_number_id")]
    public string PhoneNumberId { get; set; } = string.Empty;
}

public class Contact
{
    [JsonPropertyName("profile")]
    public Profile Profile { get; set; } = new();

    [JsonPropertyName("wa_id")]
    public string WaId { get; set; } = string.Empty;
}

public class Profile
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class Message
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public TextMessage? Text { get; set; }

    [JsonPropertyName("image")]
    public MediaMessage? Image { get; set; }

    [JsonPropertyName("document")]
    public MediaMessage? Document { get; set; }

    [JsonPropertyName("audio")]
    public MediaMessage? Audio { get; set; }

    [JsonPropertyName("video")]
    public MediaMessage? Video { get; set; }

    [JsonPropertyName("location")]
    public LocationMessage? Location { get; set; }

    [JsonPropertyName("contacts")]
    public List<ContactMessage>? Contacts { get; set; }

    [JsonPropertyName("interactive")]
    public InteractiveMessage? Interactive { get; set; }
}

public class TextMessage
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

public class MediaMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("mime_type")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    [JsonPropertyName("caption")]
    public string? Caption { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
}

public class LocationMessage
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }
}

public class ContactMessage
{
    [JsonPropertyName("name")]
    public ContactName Name { get; set; } = new();

    [JsonPropertyName("phones")]
    public List<ContactPhone>? Phones { get; set; }

    [JsonPropertyName("emails")]
    public List<ContactEmail>? Emails { get; set; }
}

public class ContactName
{
    [JsonPropertyName("formatted_name")]
    public string FormattedName { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }
}

public class ContactPhone
{
    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class ContactEmail
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class InteractiveMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("button_reply")]
    public ButtonReply? ButtonReply { get; set; }

    [JsonPropertyName("list_reply")]
    public ListReply? ListReply { get; set; }
}

public class ButtonReply
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}

public class ListReply
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class Status
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string StatusValue { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("recipient_id")]
    public string RecipientId { get; set; } = string.Empty;
}

public class OutgoingMessage
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = "whatsapp";

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public OutgoingTextMessage? Text { get; set; }

    [JsonPropertyName("image")]
    public OutgoingMediaMessage? Image { get; set; }

    [JsonPropertyName("document")]
    public OutgoingMediaMessage? Document { get; set; }

    [JsonPropertyName("audio")]
    public OutgoingMediaMessage? Audio { get; set; }

    [JsonPropertyName("video")]
    public OutgoingMediaMessage? Video { get; set; }

    [JsonPropertyName("location")]
    public OutgoingLocationMessage? Location { get; set; }

    [JsonPropertyName("interactive")]
    public OutgoingInteractiveMessage? Interactive { get; set; }
}

public class OutgoingTextMessage
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("preview_url")]
    public bool PreviewUrl { get; set; } = false;
}

public class OutgoingMediaMessage
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }

    [JsonPropertyName("caption")]
    public string? Caption { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
}

public class OutgoingLocationMessage
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }
}

public class OutgoingInteractiveMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("header")]
    public InteractiveHeader? Header { get; set; }

    [JsonPropertyName("body")]
    public InteractiveBody Body { get; set; } = new();

    [JsonPropertyName("footer")]
    public InteractiveFooter? Footer { get; set; }

    [JsonPropertyName("action")]
    public InteractiveAction Action { get; set; } = new();
}

public class InteractiveHeader
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("image")]
    public OutgoingMediaMessage? Image { get; set; }

    [JsonPropertyName("document")]
    public OutgoingMediaMessage? Document { get; set; }

    [JsonPropertyName("video")]
    public OutgoingMediaMessage? Video { get; set; }
}

public class InteractiveBody
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class InteractiveFooter
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class InteractiveAction
{
    [JsonPropertyName("buttons")]
    public List<InteractiveButton>? Buttons { get; set; }

    [JsonPropertyName("sections")]
    public List<InteractiveSection>? Sections { get; set; }
}

public class InteractiveButton
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "reply";

    [JsonPropertyName("reply")]
    public InteractiveButtonReply Reply { get; set; } = new();
}

public class InteractiveButtonReply
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}

public class InteractiveSection
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("rows")]
    public List<InteractiveRow> Rows { get; set; } = new();
}

public class InteractiveRow
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}