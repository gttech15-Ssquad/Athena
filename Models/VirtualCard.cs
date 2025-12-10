namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents a virtual card in the system.
    /// </summary>
    public class VirtualCard
    {
        /// <summary>
     /// Gets or sets the unique identifier.
  /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

   /// <summary>
    /// Gets or sets the user ID (card owner).
     /// </summary>
     public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the user navigation property.
        /// </summary>
        public User? User { get; set; }

  /// <summary>
    /// Gets or sets the unique 16-digit card number.
        /// </summary>
  public required string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets the 3-digit CVV.
        /// </summary>
 public required string CVV { get; set; }

  /// <summary>
   /// Gets or sets the card expiry date.
   /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the cardholder name.
        /// </summary>
        public required string CardholderName { get; set; }

   /// <summary>
      /// Gets or sets the card status (ACTIVE, INACTIVE, FROZEN, PENDING, APPROVED, REJECTED).
     /// </summary>
   public required string Status { get; set; } = "ACTIVE";

        /// <summary>
        /// Gets or sets the card type (CREDIT, DEBIT).
        /// </summary>
   public required string CardType { get; set; } = "CREDIT";

        /// <summary>
        /// Gets or sets the currency code (ISO 4217).
        /// </summary>
 public required string Currency { get; set; } = "NGN";

        /// <summary>
        /// Gets or sets the card nickname for user reference.
  /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
 /// Gets or sets a value indicating whether international transactions are allowed.
   /// </summary>
 public bool AllowInternational { get; set; } = true;

      /// <summary>
    /// Gets or sets the reason for card freeze (if applicable).
        /// </summary>
   public string? FreezeReason { get; set; }

        /// <summary>
   /// Gets or sets the timestamp when the card was frozen.
        /// </summary>
        public DateTime? FrozenAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
 /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

 /// <summary>
     /// Gets or sets the last update timestamp.
     /// </summary>
     public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets a value indicating whether this card is soft deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the card limits.
        /// </summary>
        public ICollection<CardLimit> CardLimits { get; set; } = new List<CardLimit>();

        /// <summary>
        /// Gets or sets the merchant restrictions for this card.
      /// </summary>
        public ICollection<CardMerchantRestriction> MerchantRestrictions { get; set; } = new List<CardMerchantRestriction>();

   /// <summary>
    /// Gets or sets the transactions for this card.
        /// </summary>
        public ICollection<CardTransaction> Transactions { get; set; } = new List<CardTransaction>();

        /// <summary>
        /// Gets or sets the approvals for this card.
        /// </summary>
      public ICollection<CardApproval> Approvals { get; set; } = new List<CardApproval>();

        /// <summary>
        /// Gets or sets the card balance.
     /// </summary>
      public CardBalance? CardBalance { get; set; }
    }
}
