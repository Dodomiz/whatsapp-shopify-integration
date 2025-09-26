using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WhatsAppIntegration.Configuration;
using WhatsAppIntegration.Models;

namespace WhatsAppIntegration.Repositories;

/// <summary>
/// MongoDB repository implementation for categorized orders operations
/// </summary>
public class CategorizedOrdersRepository : ICategorizedOrdersRepository
{
    private readonly IMongoCollection<CategorizedOrdersDocument> _collection;
    private readonly ILogger<CategorizedOrdersRepository> _logger;

    public CategorizedOrdersRepository(IOptions<MongoDbConfig> config, ILogger<CategorizedOrdersRepository> logger)
    {
        _logger = logger;
        
        var client = new MongoClient(config.Value.ConnectionString);
        var database = client.GetDatabase(config.Value.DatabaseName);
        _collection = database.GetCollection<CategorizedOrdersDocument>(config.Value.CategorizedOrdersCollection);
        
        // Create index on customerId for better query performance
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        try
        {
            var indexKeysDefinition = Builders<CategorizedOrdersDocument>.IndexKeys.Ascending(x => x.CustomerId);
            var indexOptions = new CreateIndexOptions { Background = true };
            _collection.Indexes.CreateOneAsync(new CreateIndexModel<CategorizedOrdersDocument>(indexKeysDefinition, indexOptions));
            
            var dateIndexKeysDefinition = Builders<CategorizedOrdersDocument>.IndexKeys.Descending(x => x.CreatedAt);
            _collection.Indexes.CreateOneAsync(new CreateIndexModel<CategorizedOrdersDocument>(dateIndexKeysDefinition, indexOptions));
            
            _logger.LogInformation("MongoDB indexes created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create MongoDB indexes");
        }
    }

    public async Task<CategorizedOrdersDocument> SaveCategorizedOrdersAsync(CategorizedOrdersDocument document)
    {
        try
        {
            document.UpdatedAt = DateTime.UtcNow;
            
            // Check if document for this customer already exists
            var existingDocument = await GetCategorizedOrdersByCustomerIdAsync(document.CustomerId);
            
            if (existingDocument != null)
            {
                // Update existing document
                document.Id = existingDocument.Id;
                document.CreatedAt = existingDocument.CreatedAt; // Preserve original creation time
                
                await _collection.ReplaceOneAsync(
                    Builders<CategorizedOrdersDocument>.Filter.Eq(x => x.CustomerId, document.CustomerId),
                    document);
                
                _logger.LogInformation("Updated categorized orders for customer {CustomerId}", document.CustomerId);
            }
            else
            {
                // Insert new document
                document.CreatedAt = DateTime.UtcNow;
                await _collection.InsertOneAsync(document);
                _logger.LogInformation("Saved new categorized orders for customer {CustomerId}", document.CustomerId);
            }
            
            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving categorized orders for customer {CustomerId}", document.CustomerId);
            throw;
        }
    }

    public async Task<CategorizedOrdersDocument?> GetCategorizedOrdersByCustomerIdAsync(long customerId)
    {
        try
        {
            var filter = Builders<CategorizedOrdersDocument>.Filter.Eq(x => x.CustomerId, customerId);
            var sort = Builders<CategorizedOrdersDocument>.Sort.Descending(x => x.UpdatedAt);
            
            var document = await _collection.Find(filter).Sort(sort).FirstOrDefaultAsync();
            
            if (document != null)
            {
                _logger.LogDebug("Found categorized orders for customer {CustomerId}", customerId);
            }
            
            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categorized orders for customer {CustomerId}", customerId);
            return null;
        }
    }

    public async Task<List<CategorizedOrdersDocument>> GetAllCategorizedOrdersAsync(int? limit = null)
    {
        try
        {
            var sort = Builders<CategorizedOrdersDocument>.Sort.Descending(x => x.UpdatedAt);
            var query = _collection.Find(Builders<CategorizedOrdersDocument>.Filter.Empty).Sort(sort);
            
            if (limit.HasValue)
            {
                query = query.Limit(limit.Value);
            }
            
            var documents = await query.ToListAsync();
            
            _logger.LogInformation("Retrieved {Count} categorized orders documents", documents.Count);
            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all categorized orders");
            return new List<CategorizedOrdersDocument>();
        }
    }

    public async Task<bool> UpdateCategorizedOrdersAsync(CategorizedOrdersDocument document)
    {
        try
        {
            if (string.IsNullOrEmpty(document.Id))
            {
                _logger.LogWarning("Cannot update document without ID for customer {CustomerId}", document.CustomerId);
                return false;
            }
            
            document.UpdatedAt = DateTime.UtcNow;
            
            var result = await _collection.ReplaceOneAsync(
                Builders<CategorizedOrdersDocument>.Filter.Eq(x => x.Id, document.Id),
                document);
            
            var success = result.ModifiedCount > 0;
            if (success)
            {
                _logger.LogInformation("Updated categorized orders document {DocumentId}", document.Id);
            }
            else
            {
                _logger.LogWarning("No document was updated for ID {DocumentId}", document.Id);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating categorized orders document {DocumentId}", document.Id);
            return false;
        }
    }

    public async Task<bool> DeleteCategorizedOrdersByCustomerIdAsync(long customerId)
    {
        try
        {
            var result = await _collection.DeleteManyAsync(
                Builders<CategorizedOrdersDocument>.Filter.Eq(x => x.CustomerId, customerId));
            
            var success = result.DeletedCount > 0;
            if (success)
            {
                _logger.LogInformation("Deleted {Count} categorized orders documents for customer {CustomerId}", 
                    result.DeletedCount, customerId);
            }
            else
            {
                _logger.LogWarning("No documents found to delete for customer {CustomerId}", customerId);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting categorized orders for customer {CustomerId}", customerId);
            return false;
        }
    }

    public async Task<List<CategorizedOrdersDocument>> GetCategorizedOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var filter = Builders<CategorizedOrdersDocument>.Filter.And(
                Builders<CategorizedOrdersDocument>.Filter.Gte(x => x.CreatedAt, startDate),
                Builders<CategorizedOrdersDocument>.Filter.Lte(x => x.CreatedAt, endDate)
            );
            
            var sort = Builders<CategorizedOrdersDocument>.Sort.Descending(x => x.CreatedAt);
            var documents = await _collection.Find(filter).Sort(sort).ToListAsync();
            
            _logger.LogInformation("Retrieved {Count} categorized orders documents between {StartDate} and {EndDate}", 
                documents.Count, startDate, endDate);
            
            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categorized orders by date range {StartDate} to {EndDate}", 
                startDate, endDate);
            return new List<CategorizedOrdersDocument>();
        }
    }
}