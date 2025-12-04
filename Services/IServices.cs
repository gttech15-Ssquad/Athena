using virtupay_corporate.Models;

namespace virtupay_corporate.Services
{
    /// <summary>
    /// Interface for authentication service.
    /// </summary>
   public interface IAuthService
    {
        /// <summary>
        /// Registers a new user.
 /// </summary>
      Task<User?> RegisterAsync(string email, string password, string role, string? firstName = null, string? lastName = null, int? departmentId = null);

   /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
 Task<string?> LoginAsync(string email, string password);

  /// <summary>
        /// Verifies if a user exists.
     /// </summary>
        Task<bool> UserExistsAsync(string email);

  /// <summary>
      /// Gets user details.
        /// </summary>
  Task<User?> GetUserAsync(int userId);

        /// <summary>
        /// Changes a user's password.
        /// </summary>
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

    /// <summary>
        /// Suspends a user account.
 /// </summary>
      Task<bool> SuspendUserAsync(int userId);

       /// <summary>
        /// Reactivates a user account.
/// </summary>
        Task<bool> ReactivateUserAsync(int userId);
    }

    /// <summary>
    /// Interface for card service.
    /// </summary>
    public interface ICardService
    {
        /// <summary>
 /// Creates a new virtual card.
  /// </summary>
        Task<VirtualCard?> CreateCardAsync(int userId, string cardholderName, string? nickname = null, int? departmentId = null);

 /// <summary>
        /// Gets card details.
     /// </summary>
        Task<VirtualCard?> GetCardAsync(int cardId);

   /// <summary>
    /// Gets all cards for a user.
  /// </summary>
        Task<List<VirtualCard>> GetUserCardsAsync(int userId);

        /// <summary>
      /// Updates card details.
  /// </summary>
        Task<VirtualCard?> UpdateCardAsync(int cardId, string? nickname = null, bool? allowInternational = null);

        /// <summary>
        /// Freezes a card.
        /// </summary>
        Task<bool> FreezeCardAsync(int cardId, string reason);

   /// <summary>
        /// Unfreezes a card.
   /// </summary>
   Task<bool> UnfreezeCardAsync(int cardId);

        /// <summary>
        /// Cancels a card.
        /// </summary>
   Task<bool> CancelCardAsync(int cardId);

        /// <summary>
        /// Gets card with pagination.
  /// </summary>
  Task<(List<VirtualCard> items, int total)> GetUserCardsPaginatedAsync(int userId, int pageNumber, int pageSize);
    }

    /// <summary>
    /// Interface for card limit service.
  /// </summary>
    public interface ICardLimitService
    {
  /// <summary>
        /// Sets a spending limit on a card.
        /// </summary>
   Task<CardLimit?> SetLimitAsync(int cardId, string limitType, decimal amount, string period, decimal threshold = 80);

        /// <summary>
        /// Gets all limits for a card.
      /// </summary>
        Task<List<CardLimit>> GetCardLimitsAsync(int cardId);

   /// <summary>
        /// Updates a limit.
        /// </summary>
   Task<CardLimit?> UpdateLimitAsync(int cardId, int limitId, decimal amount, decimal threshold);

        /// <summary>
   /// Removes a limit.
        /// </summary>
     Task<bool> RemoveLimitAsync(int cardId, int limitId);

     /// <summary>
  /// Validates a transaction amount against limits.
       /// </summary>
     Task<(bool isValid, string? reason)> ValidateTransactionAsync(int cardId, decimal amount);

  /// <summary>
   /// Gets the limit closest to being exceeded.
        /// </summary>
      Task<CardLimit?> GetActiveLimitAsync(int cardId);
    }

    /// <summary>
    /// Interface for balance service.
    /// </summary>
    public interface IBalanceService
    {
   /// <summary>
        /// Gets the current balance for a card.
     /// </summary>
Task<CardBalance?> GetBalanceAsync(int cardId);

        /// <summary>
        /// Initializes a balance for a new card.
    /// </summary>
      Task<CardBalance> InitializeBalanceAsync(int cardId, decimal initialAmount, string currency = "USD");

      /// <summary>
        /// Updates balance after transaction.
/// </summary>
    Task<CardBalance?> UpdateBalanceAsync(int cardId, decimal amount, string transactionStatus);

        /// <summary>
    /// Recalculates balance based on transactions.
        /// </summary>
    Task<CardBalance?> RecalculateBalanceAsync(int cardId);

     /// <summary>
      /// Funds/adds balance to a card account.
        /// </summary>
        Task<CardBalance?> FundBalanceAsync(int cardId, decimal amount, string reason, string? referenceId = null);

 /// <summary>
     /// Gets balance history for reporting.
        /// </summary>
  Task<List<(DateTime timestamp, decimal available, decimal reserved, decimal used)>> GetBalanceHistoryAsync(int cardId, DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Interface for transaction service.
    /// </summary>
    public interface ITransactionService
{
   /// <summary>
        /// Creates a new transaction.
        /// </summary>
        Task<CardTransaction?> CreateTransactionAsync(int cardId, decimal amount, string merchant, string? merchantCategoryCode = null, string? referenceId = null);

     /// <summary>
        /// Gets transaction details.
        /// </summary>
       Task<CardTransaction?> GetTransactionAsync(int transactionId);

     /// <summary>
        /// Gets transactions for a card with pagination.
      /// </summary>
    Task<(List<CardTransaction> items, int total)> GetCardTransactionsPaginatedAsync(int cardId, int pageNumber, int pageSize);

      /// <summary>
    /// Completes a transaction.
     /// </summary>
        Task<bool> CompleteTransactionAsync(int transactionId);

   /// <summary>
        /// Reverses a transaction.
        /// </summary>
      Task<bool> ReverseTransactionAsync(int transactionId, string reason);

     /// <summary>
        /// Disputes a transaction.
     /// </summary>
    Task<bool> DisputeTransactionAsync(int transactionId, string reason);

    /// <summary>
 /// Gets transactions for a date range.
   /// </summary>
   Task<List<CardTransaction>> GetTransactionsByDateRangeAsync(int cardId, DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Interface for approval service.
    /// </summary>
    public interface IApprovalService
    {
 /// <summary>
        /// Requests approval for a high-risk action.
        /// </summary>
    Task<CardApproval?> RequestApprovalAsync(int cardId, string actionType, int requestedBy, string? actionData = null);

 /// <summary>
    /// Gets pending approvals.
   /// </summary>
     Task<List<CardApproval>> GetPendingApprovalsAsync();

  /// <summary>
 /// Approves a request.
        /// </summary>
  Task<bool> ApproveAsync(int approvalId, int userId, string? reason = null);

      /// <summary>
  /// Rejects a request.
        /// </summary>
      Task<bool> RejectAsync(int approvalId, int userId, string reason);

  /// <summary>
        /// Gets approval history for a card.
        /// </summary>
    Task<List<CardApproval>> GetApprovalHistoryAsync(int cardId);

      /// <summary>
    /// Checks if approval is required for an action.
        /// </summary>
        Task<bool> IsApprovalRequiredAsync(string actionType, string userRole);
    }

    /// <summary>
    /// Interface for audit service.
   /// </summary>
    public interface IAuditService
    {
  /// <summary>
        /// Logs an action.
        /// </summary>
  Task LogActionAsync(int? userId, string action, string resource, int? resourceId, string? changes, string? ipAddress, string? userAgent, string status = "SUCCESS", string? errorMessage = null);

    /// <summary>
  /// Gets audit logs with pagination.
      /// </summary>
  Task<(List<AuditLog> items, int total)> GetAuditLogsAsync(int pageNumber, int pageSize, string? action = null, string? resource = null, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
        /// Gets audit logs for a user.
     /// </summary>
  Task<List<AuditLog>> GetUserAuditLogsAsync(int userId);

  /// <summary>
   /// Exports audit logs.
        /// </summary>
        Task<string> ExportAuditLogsAsync(string format, DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
 /// Interface for notification service.
    /// </summary>
    public interface INotificationService
    {
       /// <summary>
        /// Sends a notification.
/// </summary>
   Task SendNotificationAsync(int userId, string title, string message, string type);

    /// <summary>
        /// Sends a limit warning notification.
        /// </summary>
  Task SendLimitWarningAsync(int cardId, string limitType, decimal usedAmount, decimal limit);

        /// <summary>
        /// Sends an approval request notification.
     /// </summary>
    Task SendApprovalRequestAsync(int approvalId, List<int> approverIds);

   /// <summary>
        /// Sends a transaction notification.
        /// </summary>
  Task SendTransactionNotificationAsync(int cardId, decimal amount, string merchant, string status);

        /// <summary>
 /// Sends a card status change notification.
        /// </summary>
   Task SendCardStatusNotificationAsync(int cardId, string oldStatus, string newStatus);
    }
}
