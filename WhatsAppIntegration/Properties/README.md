# Launch Settings Configuration

## Setup Instructions

1. **Copy the example file:**
   ```bash
   cp launchSettings.example.json launchSettings.json
   ```

2. **Update the configuration with your actual values:**
   
   Replace the following placeholders in `launchSettings.json`:

   ### WhatsApp Configuration
   - `your_whatsapp_access_token_here` → Your WhatsApp Business API access token
   - `your_phone_number_id_here` → Your WhatsApp Business phone number ID
   - `your_webhook_verify_token_here` → Your custom webhook verification token
   - `your_app_secret_here` → Your WhatsApp App secret (for webhook validation)

   ### Shopify Configuration
   - `your_shop_domain_here` → Your Shopify shop domain (e.g., `mystore.myshopify.com`)
   - `your_shopify_access_token_here` → Your Shopify private app access token

   ### MongoDB Configuration (Optional)
   - Default MongoDB settings should work for local development
   - Update connection string if using remote MongoDB

## Security Notes

- ⚠️ **Never commit the actual `launchSettings.json` file** - it contains sensitive credentials
- ✅ The actual file is ignored by Git (`.gitignore`)
- ✅ Only the example file with placeholders is tracked
- 🔄 Always use the example file as a template for new environments

## Environment Variables

You can also set these values as environment variables instead of using launchSettings.json:

```bash
export WhatsApp__AccessToken="your_access_token"
export WhatsApp__PhoneNumberId="your_phone_number_id"
export Shopify__ShopDomain="your_shop_domain"
export Shopify__AccessToken="your_shopify_token"
```

## Multiple Environments

For different environments (dev, staging, prod), create separate configuration files:
- `launchSettings.development.json`
- `launchSettings.staging.json`
- `launchSettings.production.json`

Each can have different API endpoints and credentials appropriate for that environment.