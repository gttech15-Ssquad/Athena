namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents the dynamic balance of a virtual card.
 /// </summary>
    public class CardBalance
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
   /// Gets or sets the available balance (account balance - pending transactions).
        /// </summary>
        public decimal AvailableBalance { get; set; }

  /// <summary>
    /// Gets or sets the reserved balance (holds from pending transactions).
     /// </summary>
      public decimal ReservedBalance { get; set; }

        /// <summary>
  /// Gets or sets the used balance (completed transactions in current period).
        /// </summary>
        public decimal UsedBalance { get; set; }

  /// <summary>
      /// Gets or sets the currency code.
/// </summary>
    public required string Currency { get; set; } = "NGN";

    /// <summary>
        /// Gets or sets the last updated timestamp.
  /// </summary>
   public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
/// Calculates the total balance (available + reserved + used).
    /// </summary>
   /// <returns>The total balance.</returns>
 public decimal GetTotalBalance() => AvailableBalance + ReservedBalance + UsedBalance;
    }
}
