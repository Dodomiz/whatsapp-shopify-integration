# WhatsApp Integration API

A comprehensive ASP.NET Core Web API application for integrating with WhatsApp Business API, providing webhook support for receiving messages and APIs for sending various types of messages.

## Features

### Message Types Supported
- **Text Messages** - Send and receive plain text messages
- **Media Messages** - Support for images, documents, audio, and video
- **Location Messages** - Send and receive location data
- **Interactive Messages** - Button and list-based interactive messages
- **Contact Messages** - Share contact information

### Webhook Support
- **Message Receiving** - Handle incoming messages of all types
- **Message Status Updates** - Track delivery, read receipts, etc.
- **Webhook Verification** - Secure webhook endpoint verification
- **Signature Verification** - Validate webhook authenticity with HMAC-SHA256

## Configuration

Update `appsettings.json` with your WhatsApp Business API credentials:

```json
{
  "WhatsApp": {
    "AccessToken": "YOUR_ACCESS_TOKEN",
    "WebhookVerifyToken": "YOUR_WEBHOOK_VERIFY_TOKEN", 
    "PhoneNumberId": "YOUR_PHONE_NUMBER_ID",
    "BusinessAccountId": "YOUR_BUSINESS_ACCOUNT_ID",
    "AppId": "YOUR_APP_ID",
    "AppSecret": "YOUR_APP_SECRET",
    "ApiVersion": "v18.0",
    "BaseUrl": "https://graph.facebook.com"
  }
}
```

## API Endpoints

### Webhook Endpoints
- `GET /api/whatsapp/webhook` - Webhook verification
- `POST /api/whatsapp/webhook` - Receive messages and status updates

### Message Sending Endpoints
- `POST /api/whatsapp/send-text` - Send text message
- `POST /api/whatsapp/send-image` - Send image message
- `POST /api/whatsapp/send-document` - Send document
- `POST /api/whatsapp/send-location` - Send location
- `POST /api/whatsapp/send-interactive-buttons` - Send interactive button message
- `POST /api/whatsapp/send-interactive-list` - Send interactive list message

## Example Usage

### Send Text Message
```bash
curl -X POST "https://localhost:7000/api/whatsapp/send-text" \
  -H "Content-Type: application/json" \
  -d '{
    "to": "1234567890",
    "message": "Hello from WhatsApp API!",
    "previewUrl": false
  }'
```

### Send Interactive Button Message
```bash
curl -X POST "https://localhost:7000/api/whatsapp/send-interactive-buttons" \
  -H "Content-Type: application/json" \
  -d '{
    "to": "1234567890",
    "bodyText": "Please choose an option:",
    "buttons": [
      {
        "type": "reply",
        "reply": {
          "id": "btn1",
          "title": "Option 1"
        }
      },
      {
        "type": "reply", 
        "reply": {
          "id": "btn2",
          "title": "Option 2"
        }
      }
    ],
    "headerText": "Menu Selection",
    "footerText": "Choose wisely!"
  }'
```

## Running the Application

1. **Install Dependencies**
   ```bash
   dotnet restore
   ```

2. **Configure Settings**
   - Update `appsettings.json` with your WhatsApp credentials

3. **Run the Application**
   ```bash
   dotnet run
   ```

4. **Test the API**
   - The application will run on `https://localhost:7000` (or the configured port)
   - Use the OpenAPI/Swagger UI for testing: `https://localhost:7000/swagger`

## Webhook Setup

1. **Configure Webhook URL** in your WhatsApp Business API settings:
   ```
   https://your-domain.com/api/whatsapp/webhook
   ```

2. **Set Verify Token** to match the `WebhookVerifyToken` in your configuration

3. **Enable Required Webhook Fields**:
   - `messages` - for receiving messages
   - `message_deliveries` - for delivery status updates

## Security Features

- **Webhook Signature Verification** - Validates requests using HMAC-SHA256
- **Token-based Authentication** - Secure API access with access tokens
- **Input Validation** - Comprehensive request validation
- **Error Handling** - Robust error handling and logging

## Message Processing

The application includes automatic message processing that:
- **Echoes text messages** back to sender
- **Acknowledges media messages** with confirmation
- **Responds to interactive messages** with selected option
- **Logs all message events** for debugging

## Logging

Comprehensive logging is implemented for:
- Webhook events and processing
- Message sending operations
- Error conditions and debugging
- API request/response details

## Development

The project structure includes:
- `Models/` - WhatsApp API models and configuration
- `Services/` - WhatsApp service for API communication  
- `Controllers/` - REST API controllers for webhook and messaging

Built with:
- **.NET 9.0**
- **ASP.NET Core Web API**
- **System.Text.Json** for JSON serialization
- **HttpClient** for WhatsApp API communication