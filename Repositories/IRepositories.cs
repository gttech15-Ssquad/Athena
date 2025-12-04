using virtupay_corporate.Models;

namespace virtupay_corporate.Repositories
{
    /// <summary>
    /// Repository interface for User operations.
    /// </summary>
public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
    /// Gets a user by email.
      /// </summary>
        Task<User?> GetByEmailAsync(string email);

     /// <summary>
        /// Gets all active users with optional filtering.
        /// </summary>
        Task<List<User>> GetAllAsync(int? departmentId = null, string? role = null);

  /// <summary>
        /// Creates a new user.
     /// </summary>
   Task<User> CreateAsync(User user);

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        Task<User> UpdateAsync(User user);

        /// <summary>
        /// Deletes a user (soft delete).
        /// </summary>
      Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Gets paginated list of users.
        /// </summary>
        Task<(List<User> items, int total)> GetPaginatedAsync(int pageNumber, int pageSize);
    }

    /// <summary>
    /// Repository interface for Department operations.
    /// </summary>
    public interface IDepartmentRepository
    {
        /// <summary>
        /// Gets a department by ID.
        /// </summary>
  Task<Department?> GetByIdAsync(int id);

/// <summary>
     /// Gets all active departments.
      /// </summary>
 Task<List<Department>> GetAllAsync();

  /// <summary>
        /// Creates a new department.
 /// </summary>
        Task<Department> CreateAsync(Department department);

    /// <summary>
        /// Updates an existing department.
        /// </summary>
     Task<Department> UpdateAsync(Department department);

   /// <summary>
        /// Deletes a department (soft delete).
     /// </summary>
     Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Gets paginated list of departments.
        /// </summary>
        Task<(List<Department> items, int total)> GetPaginatedAsync(int pageNumber, int pageSize);
  }

    /// <summary>
    /// Repository interface for VirtualCard operations.
    /// </summary>
    public interface ICardRepository
    {
        /// <summary>
  /// Gets a card by ID.
        /// </summary>
        Task<VirtualCard?> GetByIdAsync(int id);

        /// <summary>
        /// Gets a card by card number.
        /// </summary>
 Task<VirtualCard?> GetByCardNumberAsync(string cardNumber);

  /// <summary>
 /// Gets all cards for a specific user.
        /// </summary>
        Task<List<VirtualCard>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Gets all active cards.
 /// </summary>
   Task<List<VirtualCard>> GetAllAsync();

   /// <summary>
  /// Creates a new virtual card.
        /// </summary>
   Task<VirtualCard> CreateAsync(VirtualCard card);

        /// <summary>
        /// Updates an existing card.
        /// </summary>
     Task<VirtualCard> UpdateAsync(VirtualCard card);

        /// <summary>
        /// Deletes a card (soft delete).
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Gets paginated list of cards for a user.
 /// </summary>
        Task<(List<VirtualCard> items, int total)> GetPaginatedByUserIdAsync(int userId, int pageNumber, int pageSize);
    }

    /// <summary>
   /// Repository interface for CardTransaction operations.
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// Gets a transaction by ID.
    /// </summary>
   Task<CardTransaction?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all transactions for a card.
        /// </summary>
        Task<List<CardTransaction>> GetByCardIdAsync(int cardId);

        /// <summary>
        /// Gets paginated transactions for a card.
        /// </summary>
        Task<(List<CardTransaction> items, int total)> GetPaginatedByCardIdAsync(int cardId, int pageNumber, int pageSize);

        /// <summary>
        /// Creates a new transaction.
      /// </summary>
        Task<CardTransaction> CreateAsync(CardTransaction transaction);

   /// <summary>
        /// Updates a transaction.
        /// </summary>
 Task<CardTransaction> UpdateAsync(CardTransaction transaction);

  /// <summary>
        /// Gets transactions for a date range.
        /// </summary>
    Task<List<CardTransaction>> GetByDateRangeAsync(int cardId, DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Repository interface for CardApproval operations.
    /// </summary>
    public interface IApprovalRepository
{
        /// <summary>
    /// Gets an approval by ID.
        /// </summary>
        Task<CardApproval?> GetByIdAsync(int id);

        /// <summary>
     /// Gets all pending approvals.
        /// </summary>
        Task<List<CardApproval>> GetPendingAsync();

   /// <summary>
        /// Gets approvals for a specific card.
        /// </summary>
        Task<List<CardApproval>> GetByCardIdAsync(int cardId);

     /// <summary>
        /// Gets paginated approvals with optional filtering.
/// </summary>
        Task<(List<CardApproval> items, int total)> GetPaginatedAsync(string? status = null, int pageNumber = 1, int pageSize = 20);

   /// <summary>
  /// Creates a new approval request.
        /// </summary>
        Task<CardApproval> CreateAsync(CardApproval approval);

      /// <summary>
 /// Updates an approval.
        /// </summary>
        Task<CardApproval> UpdateAsync(CardApproval approval);
    }

    /// <summary>
    /// Repository interface for AuditLog operations.
    /// </summary>
    public interface IAuditLogRepository
    {
 /// <summary>
        /// Gets an audit log by ID.
        /// </summary>
   Task<AuditLog?> GetByIdAsync(int id);

        /// <summary>
        /// Gets audit logs for a resource.
        /// </summary>
 Task<List<AuditLog>> GetByResourceAsync(string resource, int? resourceId = null);

      /// <summary>
        /// Gets paginated audit logs with optional filtering.
        /// </summary>
        Task<(List<AuditLog> items, int total)> GetPaginatedAsync(
            int pageNumber = 1,
  int pageSize = 20,
            string? action = null,
 string? resource = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// Creates a new audit log entry.
        /// </summary>
    Task<AuditLog> CreateAsync(AuditLog auditLog);

        /// <summary>
    /// Gets audit logs for a specific user.
      /// </summary>
        Task<List<AuditLog>> GetByUserIdAsync(int userId);
    }
}
