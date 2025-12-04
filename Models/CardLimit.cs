namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents spending limits applied to a virtual card.
    /// </summary>
    public class CardLimit
  {
/// <summary>
   /// Gets or sets the unique identifier.
 /// </summary>
  public int Id { get; set; }

     /// <summary>
      /// Gets or sets the card ID.
     /// </summary>
        public int CardId { get; set; }

        /// <summary>
      /// Gets or sets the virtual card navigation property.
        /// </summary>
  public VirtualCard? VirtualCard { get; set; }

        /// <summary>
        /// Gets or sets the limit type (SOFT_LIMIT, HARD_LIMIT, DAILY, WEEKLY, MONTHLY).
        /// </summary>
      public required string LimitType { get; set; }

      /// <summary>
  /// Gets or sets the maximum amount allowed.
        /// </summary>
  public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the period for the limit (DAILY, WEEKLY, MONTHLY, NO_LIMIT).
      /// </summary>
   public required string Period { get; set; }

        /// <summary>
        /// Gets or sets the warning threshold percentage (e.g., 80 for 80%).
   /// </summary>
        public decimal Threshold { get; set; } = 80;

        /// <summary>
        /// Gets or sets the creation timestamp.
   /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets a value indicating whether this limit is active.
  /// </summary>
        public bool IsActive { get; set; } = true;
}
}
