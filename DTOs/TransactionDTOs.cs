using System.ComponentModel.DataAnnotations;

namespace virtupay_corporate.DTOs
{
    /// <summary>
    /// DTO for creating a transaction.
    /// </summary>
    public class CreateTransactionRequest
    {
   /// <summary>
        /// Gets or sets the transaction amount.
      /// </summary>
        [Required(ErrorMessage = "Amount is required")]
  [Range(0.01, 999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999.99")]
 public decimal Amount { get; set; }

    /// <summary>
        /// Gets or sets the merchant name.
        /// </summary>
        [Required(ErrorMessage = "Merchant name is required")]
        [MaxLength(255)]
        public required string Merchant { get; set; }

    /// <summary>
      /// Gets or sets the merchant category code.
        /// </summary>
        [MaxLength(10)]
   public string? MerchantCategoryCode { get; set; }

        /// <summary>
        /// Gets or sets the transaction reference ID.
        /// </summary>
        [MaxLength(50)]
    public string? ReferenceId { get; set; }

        /// <summary>
        /// Gets or sets the currency code.
        /// </summary>
        public string Currency { get; set; } = "USD";
    }

    /// <summary>
    /// DTO for completing a transaction.
    /// </summary>
  public class CompleteTransactionRequest
    {
      /// <summary>
 /// Gets or sets the transaction status.
        /// </summary>
    [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(COMPLETED|FAILED|REVERSED)$")]
  public required string Status { get; set; }

        /// <summary>
        /// Gets or sets optional notes.
    /// </summary>
        [MaxLength(500)]
      public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for disputing a transaction.
    /// </summary>
    public class DisputeTransactionRequest
   {
        /// <summary>
    /// Gets or sets the dispute reason.
        /// </summary>
        [Required(ErrorMessage = "Dispute reason is required")]
      [MaxLength(500)]
  public required string Reason { get; set; }

        /// <summary>
        /// Gets or sets additional dispute details.
        /// </summary>
        [MaxLength(1000)]
      public string? Details { get; set; }
    }

    /// <summary>
  /// DTO for reversing a transaction.
    /// </summary>
    public class ReverseTransactionRequest
{
        /// <summary>
        /// Gets or sets the reversal reason.
 /// </summary>
        [Required(ErrorMessage = "Reversal reason is required")]
        [MaxLength(500)]
     public required string Reason { get; set; }
    }

    /// <summary>
    /// DTO for transaction response.
/// </summary>
   public class TransactionResponse
    {
        /// <summary>
  /// Gets or sets the transaction ID.
      /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the card ID.
        /// </summary>
        public int CardId { get; set; }

        /// <summary>
     /// Gets or sets the transaction amount.
        /// </summary>
   public decimal Amount { get; set; }

     /// <summary>
        /// Gets or sets the merchant name.
        /// </summary>
        public string? Merchant { get; set; }

    /// <summary>
        /// Gets or sets the merchant category code.
        /// </summary>
  public string? MerchantCategoryCode { get; set; }

        /// <summary>
        /// Gets or sets the transaction status.
        /// </summary>
      public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the currency code.
        /// </summary>
     public string? Currency { get; set; }

        /// <summary>
        /// Gets or sets the reference ID.
        /// </summary>
        public string? ReferenceId { get; set; }

     /// <summary>
        /// Gets or sets the dispute reason.
    /// </summary>
        public string? DisputeReason { get; set; }

  /// <summary>
 /// Gets or sets whether the transaction can be disputed.
        /// </summary>
   public bool CanBeDisputed { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
     public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the completion timestamp.
        /// </summary>
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// DTO for transaction history response.
    /// </summary>
    public class TransactionHistoryResponse
    {
   /// <summary>
      /// Gets or sets the total transaction amount.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the transaction count.
        /// </summary>
        public int TransactionCount { get; set; }

 /// <summary>
        /// Gets or sets transactions by status.
        /// </summary>
     public Dictionary<string, int> TransactionsByStatus { get; set; } = new Dictionary<string, int>();

     /// <summary>
        /// Gets or sets the average transaction amount.
        /// </summary>
        public decimal AverageAmount { get; set; }

        /// <summary>
        /// Gets or sets the period start date.
   /// </summary>
    public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Gets or sets the period end date.
        /// </summary>
        public DateTime PeriodEnd { get; set; }
    }

    /// <summary>
    /// DTO for transaction summary.
 /// </summary>
    public class TransactionSummaryResponse
    {
 /// <summary>
        /// Gets or sets the total completed amount.
        /// </summary>
        public decimal CompletedAmount { get; set; }

    /// <summary>
 /// Gets or sets the total pending amount.
        /// </summary>
        public decimal PendingAmount { get; set; }

      /// <summary>
        /// Gets or sets the total reversed amount.
        /// </summary>
      public decimal ReversedAmount { get; set; }

        /// <summary>
    /// Gets or sets the total failed amount.
        /// </summary>
        public decimal FailedAmount { get; set; }

        /// <summary>
        /// Gets or sets the transactions by merchant.
        /// </summary>
        public Dictionary<string, decimal> TransactionsByMerchant { get; set; } = new Dictionary<string, decimal>();
    }
}
