namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents a transaction on the main account balance.
    /// Tracks all account funding and withdrawals.
    /// </summary>
public class AccountTransaction
    {
  /// <summary>
   /// Gets or sets the unique identifier.
        /// </summary>
     public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
   /// Gets or sets the account balance ID.
  /// </summary>
        public Guid AccountBalanceId { get; set; }

        /// <summary>
   /// Gets or sets the account balance navigation property.
      /// </summary>
   public AccountBalance? AccountBalance { get; set; }

  /// <summary>
/// Gets or sets the transaction type (FUNDING, WITHDRAWAL).
        /// </summary>
        public required string TransactionType { get; set; }

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
  /// Gets or sets the transaction description/reason.
   /// </summary>
        public string? Description { get; set; }

 /// <summary>
     /// Gets or sets the reference ID for tracking (e.g., card transaction ID, funding ID).
        /// </summary>
    public string? ReferenceId { get; set; }

     /// <summary>
     /// Gets or sets the user ID who initiated the transaction.
      /// </summary>
 public Guid? InitiatedBy { get; set; }

      /// <summary>
     /// Gets or sets the currency code (ISO 4217).
 /// </summary>
public required string Currency { get; set; } = "NGN";

        /// <summary>
      /// Gets or sets the transaction status (PENDING, COMPLETED, FAILED, REVERSED).
  /// </summary>
 public required string Status { get; set; } = "COMPLETED";

    /// <summary>
        /// Gets or sets the creation timestamp.
    /// </summary>
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

     /// <summary>
    /// Gets or sets the completion timestamp.
     /// </summary>
   public DateTime? CompletedAt { get; set; }

  /// <summary>
 /// Gets or sets the related card ID if this is a withdrawal from a card transaction.
/// </summary>
        public Guid? RelatedCardId { get; set; }

        /// <summary>
        /// Gets or sets the related card transaction ID if this is a withdrawal from a card transaction.
      /// </summary>
        public Guid? RelatedCardTransactionId { get; set; }
    }
}
