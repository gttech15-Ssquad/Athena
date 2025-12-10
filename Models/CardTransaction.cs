namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents a transaction on a virtual card.
    /// </summary>
  public class CardTransaction
    {
 /// <summary>
        /// Gets or sets the unique identifier.
   /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

  /// <summary>
        /// Gets or sets the card ID.
      /// </summary>
        public Guid CardId { get; set; }

        /// <summary>
 /// Gets or sets the virtual card navigation property.
   /// </summary>
   public VirtualCard? VirtualCard { get; set; }

   /// <summary>
    /// Gets or sets the transaction amount.
    /// </summary>
   public decimal Amount { get; set; }

     /// <summary>
      /// Gets or sets the merchant name.
     /// </summary>
public required string Merchant { get; set; }

     /// <summary>
     /// Gets or sets the merchant category code.
   /// </summary>
   public string? MerchantCategoryCode { get; set; }

 /// <summary>
  /// Gets or sets the transaction status (PENDING, COMPLETED, FAILED, REVERSED).
        /// </summary>
        public required string Status { get; set; } = "PENDING";

 /// <summary>
   /// Gets or sets the currency code.
      /// </summary>
   public required string Currency { get; set; } = "NGN";

        /// <summary>
        /// Gets or sets the transaction reference ID.
        /// </summary>
   public string? ReferenceId { get; set; }

        /// <summary>
     /// Gets or sets a value indicating whether this transaction can be disputed.
  /// </summary>
        public bool CanBeDisputed { get; set; } = true;

        /// <summary>
     /// Gets or sets the dispute reason if applicable.
   /// </summary>
        public string? DisputeReason { get; set; }

   /// <summary>
        /// Gets or sets the creation timestamp.
     /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
      /// Gets or sets the last update timestamp.
     /// </summary>
      public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp when the transaction was completed.
 /// </summary>
      public DateTime? CompletedAt { get; set; }
    }
}
