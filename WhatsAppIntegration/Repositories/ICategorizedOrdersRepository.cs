using WhatsAppIntegration.Models;

namespace WhatsAppIntegration.Repositories;

/// <summary>
/// Repository interface for categorized orders operations
/// </summary>
public interface ICategorizedOrdersRepository
{
    /// <summary>
    /// Save categorized orders for a customer to MongoDB
    /// </summary>
    /// <param name="document">The categorized orders document to save</param>
    /// <returns>The saved document with generated ID</returns>
    Task<CategorizedOrdersDocument> SaveCategorizedOrdersAsync(CategorizedOrdersDocument document);
    
    /// <summary>
    /// Get categorized orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>The most recent categorized orders document for the customer, or null if not found</returns>
    Task<CategorizedOrdersDocument?> GetCategorizedOrdersByCustomerIdAsync(long customerId);
    
    /// <summary>
    /// Get all categorized orders documents
    /// </summary>
    /// <param name="limit">Maximum number of documents to return</param>
    /// <returns>List of categorized orders documents</returns>
    Task<List<CategorizedOrdersDocument>> GetAllCategorizedOrdersAsync(int? limit = null);
    
    /// <summary>
    /// Update existing categorized orders document
    /// </summary>
    /// <param name="document">The document to update</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateCategorizedOrdersAsync(CategorizedOrdersDocument document);
    
    /// <summary>
    /// Delete categorized orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteCategorizedOrdersByCustomerIdAsync(long customerId);
    
    /// <summary>
    /// Get categorized orders within a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>List of categorized orders documents</returns>
    Task<List<CategorizedOrdersDocument>> GetCategorizedOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
}