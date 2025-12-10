using System.ComponentModel.DataAnnotations;

namespace virtupay_corporate.DTOs
{
    /// <summary>
    /// DTO for account balance response.
    /// </summary>
    public class AccountBalanceResponse
    {
/// <summary>
   /// Gets or sets the account balance ID.
 /// </summary>
        public Guid Id { get; set; }

      /// <summary>
     /// Gets or sets the user ID.
   /// </summary>
   public Guid UserId { get; set; }

  /// <summary>
 /// Gets or sets the current available balance.
  /// </summary>
  public decimal AvailableBalance { get; set; }

 /// <summary>
      /// Gets or sets the total amount funded.
     /// </summary>
  public decimal TotalFunded { get; set; }

 /// <summary>
        /// Gets or sets the total amount withdrawn.
        /// </summary>
  public decimal TotalWithdrawn { get; set; }

        /// <summary>
    /// Gets or sets the currency code.
  /// </summary>
   public string? Currency { get; set; }

   /// <summary>
        /// Gets or sets the creation timestamp.
   /// </summary>
   public DateTime CreatedAt { get; set; }

  /// <summary>
      /// Gets or sets the last updated timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for funding the main account balance.
  /// </summary>
   public class FundAccountBalanceRequest
    {
   /// <summary>
  /// Gets or sets the amount to fund.
        /// </summary>
        [Required(ErrorMessage = "Amount is required")]
  [Range(0.01, 9999999.99, ErrorMessage = "Amount must be between 0.01 and 9999999.99")]
        public decimal Amount { get; set; }

    /// <summary>
        /// Gets or sets the reason for funding.
      /// </summary>
        [Required(ErrorMessage = "Reason is required")]
    [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
   public string? Reason { get; set; }

/// <summary>
   /// Gets or sets an optional reference ID for tracking.
        /// </summary>
[MaxLength(100, ErrorMessage = "Reference ID cannot exceed 100 characters")]
 public string? ReferenceId { get; set; }
 }

    /// <summary>
 /// DTO for withdrawing from main account balance (admin/system use).
 /// </summary>
    public class WithdrawAccountBalanceRequest
    {
  /// <summary>
    /// Gets or sets the amount to withdraw.
/// </summary>
     [Required(ErrorMessage = "Amount is required")]
   [Range(0.01, 9999999.99, ErrorMessage = "Amount must be between 0.01 and 9999999.99")]
  public decimal Amount { get; set; }

 /// <summary>
   /// Gets or sets the reason for withdrawal.
      /// </summary>
  [Required(ErrorMessage = "Reason is required")]
     [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; set; }

        /// <summary>
 /// Gets or sets the related card ID (if withdrawal is from a card transaction).
        /// </summary>
    public Guid? RelatedCardId { get; set; }

  /// <summary>
    /// Gets or sets the related card transaction ID.
 /// </summary>
public Guid? RelatedCardTransactionId { get; set; }

      /// <summary>
    /// Gets or sets an optional reference ID for tracking.
 /// </summary>
     [MaxLength(100)]
  public string? ReferenceId { get; set; }
    }

    /// <summary>
   /// DTO for account transaction response.
    /// </summary>
   public class AccountTransactionResponse
    {
  /// <summary>
 /// Gets or sets the transaction ID.
  /// </summary>
public Guid Id { get; set; }

   /// <summary>
      /// Gets or sets the account balance ID.
  /// </summary>
 public Guid AccountBalanceId { get; set; }

     /// <summary>
   /// Gets or sets the transaction type.
     /// </summary>
    public string? TransactionType { get; set; }

    /// <summary>
 /// Gets or sets the transaction amount.
        /// </summary>
   public decimal Amount { get; set; }

        /// <summary>
  /// Gets or sets the balance before the transaction.
   /// </summary>
  public decimal BalanceBefore { get; set; }

        /// <summary>
 /// Gets or sets the balance after the transaction.
   /// </summary>
      public decimal BalanceAfter { get; set; }

   /// <summary>
   /// Gets or sets the description/reason.
   /// </summary>
      public string? Description { get; set; }

  /// <summary>
        /// Gets or sets the reference ID.
   /// </summary>
  public string? ReferenceId { get; set; }

     /// <summary>
   /// Gets or sets the user ID who initiated the transaction.
   /// </summary>
   public Guid? InitiatedBy { get; set; }

  /// <summary>
        /// Gets or sets the currency code.
    /// </summary>
    public string? Currency { get; set; }

        /// <summary>
   /// Gets or sets the transaction status.
      /// </summary>
  public string? Status { get; set; }

 /// <summary>
      /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

     /// <summary>
 /// Gets or sets the completion timestamp.
        /// </summary>
    public DateTime? CompletedAt { get; set; }

 /// <summary>
    /// Gets or sets the related card ID.
        /// </summary>
   public Guid? RelatedCardId { get; set; }

 /// <summary>
 /// Gets or sets the related card transaction ID.
   /// </summary>
public Guid? RelatedCardTransactionId { get; set; }
    }

    /// <summary>
  /// DTO for account balance summary response.
    /// </summary>
public class AccountBalanceSummaryResponse
    {
 /// <summary>
  /// Gets or sets the current available balance.
        /// </summary>
        public decimal AvailableBalance { get; set; }

  /// <summary>
   /// Gets or sets the total amount funded.
  /// </summary>
 public decimal TotalFunded { get; set; }

 /// <summary>
      /// Gets or sets the total amount withdrawn.
   /// </summary>
   public decimal TotalWithdrawn { get; set; }

    /// <summary>
 /// Gets or sets the net balance (funded - withdrawn).
    /// </summary>
  public decimal NetBalance => TotalFunded - TotalWithdrawn;

        /// <summary>
   /// Gets or sets the currency code.
     /// </summary>
    public string? Currency { get; set; }

 /// <summary>
  /// Gets or sets the last updated timestamp.
  /// </summary>
 public DateTime LastUpdated { get; set; }

  /// <summary>
    /// Gets or sets the total number of cards funded from this account.
    /// </summary>
 public int TotalCardsFunded { get; set; }

 /// <summary>
      /// Gets or sets the number of active transactions.
   /// </summary>
        public int ActiveTransactions { get; set; }
    }
}
