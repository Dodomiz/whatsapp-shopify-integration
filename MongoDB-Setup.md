# MongoDB Configuration

This document explains how to configure MongoDB for the WhatsApp-Shopify Integration project using environment variables.

## Environment Variables

The application reads MongoDB configuration from the following environment variables:

### Required Environment Variables

| Variable | Description | Default Value | Example |
|----------|-------------|---------------|---------|
| `MongoDB__ConnectionString` | MongoDB connection string | `mongodb://localhost:27017` | `mongodb://localhost:27017` |
| `MongoDB__DatabaseName` | Database name to use | `WhatsAppIntegration` | `WhatsAppIntegration` |
| `MongoDB__CategorizedOrdersCollection` | Collection name for categorized orders | `CategorizedOrders` | `CategorizedOrders` |

## Configuration Methods

### 1. Using launchSettings.json (Development)

The environment variables are already configured in the `Properties/launchSettings.json` file:

```json
{
  "environmentVariables": {
    "MongoDB__ConnectionString": "mongodb://localhost:27017",
    "MongoDB__DatabaseName": "WhatsAppIntegration",
    "MongoDB__CategorizedOrdersCollection": "CategorizedOrders"
  }
}
```

### 2. Using System Environment Variables

Set these environment variables in your system:

**Windows (Command Prompt):**
```cmd
set MongoDB__ConnectionString=mongodb://localhost:27017
set MongoDB__DatabaseName=WhatsAppIntegration
set MongoDB__CategorizedOrdersCollection=CategorizedOrders
```

**Windows (PowerShell):**
```powershell
$env:MongoDB__ConnectionString="mongodb://localhost:27017"
$env:MongoDB__DatabaseName="WhatsAppIntegration"
$env:MongoDB__CategorizedOrdersCollection="CategorizedOrders"
```

**Linux/macOS (Bash):**
```bash
export MongoDB__ConnectionString="mongodb://localhost:27017"
export MongoDB__DatabaseName="WhatsAppIntegration"
export MongoDB__CategorizedOrdersCollection="CategorizedOrders"
```

### 3. Using Docker Environment Variables

```dockerfile
ENV MongoDB__ConnectionString=mongodb://mongo:27017
ENV MongoDB__DatabaseName=WhatsAppIntegration
ENV MongoDB__CategorizedOrdersCollection=CategorizedOrders
```

### 4. Using Kubernetes ConfigMap/Secret

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: mongodb-config
data:
  MongoDB__ConnectionString: "mongodb://mongodb-service:27017"
  MongoDB__DatabaseName: "WhatsAppIntegration"
  MongoDB__CategorizedOrdersCollection: "CategorizedOrders"
```

## MongoDB Connection String Examples

### Local MongoDB Instance
```
MongoDB__ConnectionString=mongodb://localhost:27017
```

### MongoDB with Authentication
```
MongoDB__ConnectionString=mongodb://username:password@localhost:27017/database
```

### MongoDB Atlas (Cloud)
```
MongoDB__ConnectionString=mongodb+srv://username:password@cluster.mongodb.net/database
```

### MongoDB Replica Set
```
MongoDB__ConnectionString=mongodb://host1:27017,host2:27017,host3:27017/database?replicaSet=rs0
```

## Setting Up MongoDB Locally

### 1. Install MongoDB

**Using Docker:**
```bash
docker run -d --name mongodb -p 27017:27017 mongo:latest
```

**Using MongoDB Community Edition:**
- Download from [MongoDB Download Center](https://www.mongodb.com/try/download/community)
- Follow installation instructions for your platform

### 2. Verify Connection

You can verify your MongoDB connection using:

```bash
# Using MongoDB Compass (GUI tool)
# Connect to: mongodb://localhost:27017

# Using MongoDB Shell
mongosh "mongodb://localhost:27017"
```

## Data Storage

When the application runs, it will:

1. **Automatically create the database** specified in `MongoDB__DatabaseName`
2. **Create the collection** specified in `MongoDB__CategorizedOrdersCollection`
3. **Create indexes** for better query performance:
   - Index on `customerId` for fast customer lookups
   - Index on `createdAt` for date-based queries

## Document Structure

Each document stored in MongoDB contains:

```json
{
  "_id": "ObjectId(...)",
  "customerId": 123,
  "customer": {
    "id": 123,
    "firstName": "John",
    "lastName": "Doe",
    // ... other customer fields
  },
  "automationProductsOrders": [
    {
      "id": 1001,
      "totalPrice": "150.00",
      "lineItems": [
        {
          "productId": 501,
          "productTags": ["includeAutomation", "premium"],
          // ... other line item fields
        }
      ]
      // ... other order fields
    }
  ],
  "dogExtraProductsOrders": [
    // ... similar structure
  ],
  "createdAt": "2025-09-23T...",
  "updatedAt": "2025-09-23T...",
  "filters": {
    "status": "any",
    "limit": null,
    "minOrdersPerCustomer": 2,
    "createdAtMin": null,
    "createdAtMax": null
  }
}
```

## Error Handling

- If MongoDB is **unavailable**, the API will continue to work but log errors
- The application uses **fallback default values** if environment variables are missing
- Failed MongoDB operations **don't break** the main API functionality

## Monitoring and Logging

The application logs MongoDB operations at different levels:

- **Info**: Successful operations and connection status
- **Warning**: Connection issues or failed index creation
- **Error**: Failed save/retrieve operations
- **Debug**: Detailed operation information

Check your application logs to monitor MongoDB integration status.