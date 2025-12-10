namespace virtupay_corporate.Models
{
    /// <summary>
/// Represents a merchant category for transaction restrictions.
    /// </summary>
    public class MerchantCategory
    {
  /// <summary>
    /// Gets or sets the unique identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
 /// Gets or sets the category name (e.g., "Gas Stations", "Hotels").
  /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the category code (MCC code).
    /// </summary>
   public required string Code { get; set; }

        /// <summary>
        /// Gets or sets the category description.
        /// </summary>
        public string? Description { get; set; }

      /// <summary>
        /// Gets or sets the creation timestamp.
   /// </summary>
   public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
