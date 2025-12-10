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
        Task<VirtualCard?> CreateCardAsync(Guid userId, string cardholderName, string? nickname = null, Guid? departmentId = null);

        /// <summary>
 /// Gets card details.
        /// </summary>
        Task<VirtualCard?> GetCardAsync(Guid cardId);

      /// <summary>
      /// Gets all cards for a user.
        /// </summary>
 Task<List<VirtualCard>> GetUserCardsAsync(Guid userId);

        /// <summary>
        /// Updates card details.
 /// </summary>
        Task<VirtualCard?> UpdateCardAsync(Guid cardId, string? nickname = null, bool? allowInternational = null);

   /// <summary>
        /// Freezes a card.
   /// </summary>
     Task<bool> FreezeCardAsync(Guid cardId, string reason);

    /// <summary>
  /// Unfreezes a card.
        /// </summary>
        Task<bool> UnfreezeCardAsync(Guid cardId);

 /// <summary>
     /// Cancels a card.
   /// </summary>
 Task<bool> CancelCardAsync(Guid cardId);

        /// <summary>
     /// Gets card with pagination.
   /// </summary>
        Task<(List<VirtualCard> items, int total)> GetUserCardsPaginatedAsync(Guid userId, int pageNumber, int pageSize);
    }

    /// <summary>
    /// Interface for card limit service.
    /// </summary>
    public interface ICardLimitService
    {
 /// <summary>
        /// Sets a spending limit on a card.
        /// </summary>
    Task<CardLimit?> SetLimitAsync(Guid cardId, string limitType, decimal amount, string period, decimal threshold = 80);

  /// <summary>
        /// Gets all limits for a card.
 /// </summary>
   Task<List<CardLimit>> GetCardLimitsAsync(Guid cardId);

        /// <summary>
        /// Updates a limit.
 /// </summary>
    Task<CardLimit?> UpdateLimitAsync(Guid cardId, Guid limitId, decimal amount, decimal threshold);

        /// <summary>
   /// Removes a limit.
      /// </summary>
     Task<bool> RemoveLimitAsync(Guid cardId, Guid limitId);

 /// <summary>
 /// Validates a transaction amount against limits.
        /// </summary>
        Task<(bool isValid, string? reason)> ValidateTransactionAsync(Guid cardId, decimal amount);

      /// <summary>
        /// Gets the limit closest to being exceeded.
        /// </summary>
        Task<CardLimit?> GetActiveLimitAsync(Guid cardId);
  }

    /// <summary>
    /// Interface for balance service.
    /// </summary>
  public interface IBalanceService
    {
/// <summary>
        /// Gets the current balance for a card.
   /// </summary>
        Task<CardBalance?> GetBalanceAsync(Guid cardId);

        /// <summary>
        /// Initializes a balance for a new card.
        /// </summary>
        Task<CardBalance> InitializeBalanceAsync(Guid cardId, decimal initialAmount, string currency = "NGN");

        /// <summary>
      /// Updates balance after transaction.
  /// </summary>
    Task<CardBalance?> UpdateBalanceAsync(Guid cardId, decimal amount, string transactionStatus);

        /// <summary>
/// Recalculates balance based on transactions.
        /// </summary>
    Task<CardBalance?> RecalculateBalanceAsync(Guid cardId);

    /// <summary>
        /// Funds/adds balance to a card account.
        /// </summary>
        Task<CardBalance?> FundBalanceAsync(Guid cardId, decimal amount, string reason, string? referenceId = null);

   /// <summary>
  /// Gets balance history for reporting.
 /// </summary>
     Task<List<(DateTime timestamp, decimal available, decimal reserved, decimal used)>> GetBalanceHistoryAsync(Guid cardId, DateTime startDate, DateTime endDate);
  }

    /// <summary>
    /// Interface for transaction service.
    /// </summary>
    public interface ITransactionService
    {
 /// <summary>
   /// Creates a new transaction.
    /// </summary>
      Task<CardTransaction?> CreateTransactionAsync(Guid cardId, decimal amount, string merchant, string? merchantCategoryCode = null, string? referenceId = null);

        /// <summary>
     /// Gets transaction details.
/// </summary>
        Task<CardTransaction?> GetTransactionAsync(Guid transactionId);

        /// <summary>
   /// Gets transactions for a card with pagination.
        /// </summary>
        Task<(List<CardTransaction> items, int total)> GetCardTransactionsPaginatedAsync(Guid cardId, int pageNumber, int pageSize);

   /// <summary>
    /// Completes a transaction.
  /// </summary>
        Task<bool> CompleteTransactionAsync(Guid transactionId);

   /// <summary>
      /// Reverses a transaction.
      /// </summary>
     Task<bool> ReverseTransactionAsync(Guid transactionId, string reason);

      /// <summary>
  /// Disputes a transaction.
        /// </summary>
        Task<bool> DisputeTransactionAsync(Guid transactionId, string reason);

        /// <summary>
        /// Gets transactions for a date range.
 /// </summary>
        Task<List<CardTransaction>> GetTransactionsByDateRangeAsync(Guid cardId, DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Interface for approval service.
    /// </summary>
    public interface IApprovalService
    {
 /// <summary>
     /// Requests approval for a high-risk action.
   /// </summary>
        Task<CardApproval?> RequestApprovalAsync(Guid cardId, string actionType, Guid requestedBy, string? actionData = null);

        /// <summary>
     /// Gets pending approvals.
        /// </summary>
 Task<List<CardApproval>> GetPendingApprovalsAsync();

/// <summary>
  /// Approves a request.
        /// </summary>
        Task<bool> ApproveAsync(Guid approvalId, Guid userId, string? reason = null);

        /// <summary>
 /// Rejects a request.
        /// </summary>
        Task<bool> RejectAsync(Guid approvalId, Guid userId, string reason);

        /// <summary>
    /// Gets approval history for a card.
    /// </summary>
        Task<List<CardApproval>> GetApprovalHistoryAsync(Guid cardId);

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
     Task LogActionAsync(Guid? userId, string action, string resource, Guid? resourceId, string? changes, string? ipAddress, string? userAgent, string status = "SUCCESS", string? errorMessage = null);

 /// <summary>
   /// Gets audit logs with pagination.
        /// </summary>
   Task<(List<AuditLog> items, int total)> GetAuditLogsAsync(int pageNumber, int pageSize, string? action = null, string? resource = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
 /// Gets audit logs for a user.
        /// </summary>
        Task<List<AuditLog>> GetUserAuditLogsAsync(Guid userId);

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
Task SendNotificationAsync(Guid userId, string title, string message, string type);

        /// <summary>
  /// Sends a limit warning notification.
 /// </summary>
      Task SendLimitWarningAsync(Guid cardId, string limitType, decimal usedAmount, decimal limit);

     /// <summary>
        /// Sends an approval request notification.
        /// </summary>
        Task SendApprovalRequestAsync(Guid approvalId, List<Guid> approverIds);

        /// <summary>
  /// Sends a transaction notification.
/// </summary>
        Task SendTransactionNotificationAsync(Guid cardId, decimal amount, string merchant, string status);

        /// <summary>
    /// Sends a card status change notification.
   /// </summary>
        Task SendCardStatusNotificationAsync(Guid cardId, string oldStatus, string newStatus);
    }

    /// <summary>
    /// Interface for account balance service.
  /// Manages main user account balances and funding.
    /// </summary>
  public interface IAccountBalanceService
 {
  /// <summary>
     /// Gets or creates the account balance for a user.
  /// </summary>
   Task<AccountBalance?> GetOrCreateAccountBalanceAsync(Guid userId, string currency = "NGN");

   /// <summary>
        /// Gets the account balance for a user.
      /// </summary>
        Task<AccountBalance?> GetAccountBalanceAsync(Guid userId);

 /// <summary>
 /// Funds the main account balance.
        /// </summary>
        Task<AccountBalance?> FundAccountAsync(Guid userId, decimal amount, string reason, string? referenceId = null, Guid? initiatedBy = null);

        /// <summary>
  /// Withdraws from the main account balance (for card transactions).
        /// </summary>
 Task<(bool success, string? errorMessage)> WithdrawFromAccountAsync(Guid userId, decimal amount, string reason, Guid? relatedCardId = null, Guid? relatedCardTransactionId = null, string? referenceId = null, Guid? initiatedBy = null);

  /// <summary>
        /// Gets account transaction history with pagination.
        /// </summary>
  Task<(List<AccountTransaction> items, int total)> GetAccountTransactionsPaginatedAsync(Guid userId, int pageNumber, int pageSize, string? transactionType = null);

        /// <summary>
        /// Gets account balance summary for dashboard.
        /// </summary>
        Task<(decimal available, decimal totalFunded, decimal totalWithdrawn, int cardsFunded, int activeTransactions)> GetAccountSummaryAsync(Guid userId);

        /// <summary>
        /// Reverses an account transaction.
    /// </summary>
        Task<bool> ReverseTransactionAsync(Guid transactionId, string reason);

     /// <summary>
 /// Gets transactions by date range.
        /// </summary>
     Task<List<AccountTransaction>> GetTransactionsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
    }
}
