using WhatsAppIntegration.Models;

namespace WhatsAppIntegration.Services;

/// <summary>
/// Interface for WhatsApp Business API service operations
/// </summary>
public interface IWhatsAppService
{
    /// <summary>
    /// Send a text message via WhatsApp
    /// </summary>
    /// <param name="to">WhatsApp phone number to send message to</param>
    /// <param name="message">Text message content</param>
    /// <param name="previewUrl">Whether to show URL preview for links in the message</param>
    /// <returns>True if message was sent successfully, false otherwise</returns>
    Task<bool> SendTextMessageAsync(string to, string message, bool previewUrl = false);

    /// <summary>
    /// Send an image message via WhatsApp
    /// </summary>
    /// <param name="to">WhatsApp phone number to send image to</param>
    /// <param name="imageUrl">URL of the image to send</param>
    /// <param name="caption">Optional caption for the image</param>
    /// <returns>True if image was sent successfully, false otherwise</returns>
    Task<bool> SendImageMessageAsync(string to, string imageUrl, string? caption = null);

    /// <summary>
    /// Send a document message via WhatsApp
    /// </summary>
    /// <param name="to">WhatsApp phone number to send document to</param>
    /// <param name="documentUrl">URL of the document to send</param>
    /// <param name="filename">Optional filename for the document</param>
    /// <param name="caption">Optional caption for the document</param>
    /// <returns>True if document was sent successfully, false otherwise</returns>
    Task<bool> SendDocumentMessageAsync(string to, string documentUrl, string? filename = null, string? caption = null);

    /// <summary>
    /// Send an audio message via WhatsApp
    /// </summary>
    /// <param name="to">WhatsApp phone number to send audio to</param>
    /// <param name="audioUrl">URL of the audio file to send</param>
    /// <returns>True if audio was sent successfully, false otherwise</returns>
    Task<bool> SendAudioMessageAsync(string to, string audioUrl);

    /// <summary>
    /// Send a video message via WhatsApp
    /// </summary>
    /// <param name="to">WhatsApp phone number to send video to</param>
    /// <param name="videoUrl">URL of the video file to send</param>
    /// <param name="caption">Optional caption for the video</param>
    /// <returns>True if video was sent successfully, false otherwise</returns>
    Task<bool> SendVideoMessageAsync(string to, string videoUrl, string? caption = null);

    /// <summary>
    /// Send a location message via WhatsApp
    /// </summary>
    /// <param name="to">WhatsApp phone number to send location to</param>
    /// <param name="latitude">Latitude coordinate</param>
    /// <param name="longitude">Longitude coordinate</param>
    /// <param name="name">Optional name for the location</param>
    /// <param name="address">Optional address for the location</param>
    /// <returns>True if location was sent successfully, false otherwise</returns>
    Task<bool> SendLocationMessageAsync(string to, double latitude, double longitude, string? name = null, string? address = null);

    /// <summary>
    /// Send an interactive button message via WhatsApp
    /// </summary>
    /// <param name="to">WhatsApp phone number to send message to</param>
    /// <param name="bodyText">Main body text of the message</param>
    /// <param name="buttons">List of interactive buttons (maximum 3)</param>
    /// <param name="headerText">Optional header text</param>
    /// <param name="footerText">Optional footer text</param>
    /// <returns>True if interactive button message was sent successfully, false otherwise</returns>
    Task<bool> SendInteractiveButtonMessageAsync(string to, string bodyText, List<InteractiveButton> buttons, string? headerText = null, string? footerText = null);

    /// <summary>
    /// Send an interactive list message via WhatsApp
    /// </summary>
    /// <param name="to">WhatsApp phone number to send message to</param>
    /// <param name="bodyText">Main body text of the message</param>
    /// <param name="sections">List of sections containing interactive options</param>
    /// <param name="buttonText">Text displayed on the list button</param>
    /// <param name="headerText">Optional header text</param>
    /// <param name="footerText">Optional footer text</param>
    /// <returns>True if interactive list message was sent successfully, false otherwise</returns>
    Task<bool> SendInteractiveListMessageAsync(string to, string bodyText, List<InteractiveSection> sections, string buttonText = "Options", string? headerText = null, string? footerText = null);

    /// <summary>
    /// Get the download URL for a media file by its ID
    /// </summary>
    /// <param name="mediaId">WhatsApp media ID</param>
    /// <returns>Media download URL if successful, null otherwise</returns>
    Task<string?> GetMediaUrlAsync(string mediaId);

    /// <summary>
    /// Download media content from a URL
    /// </summary>
    /// <param name="mediaUrl">Media download URL</param>
    /// <returns>Media content as byte array if successful, null otherwise</returns>
    Task<byte[]?> DownloadMediaAsync(string mediaUrl);
}