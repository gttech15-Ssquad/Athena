namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents merchant restrictions applied to a specific card.
    /// </summary>
    public class CardMerchantRestriction
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
        /// Gets or sets the merchant category ID.
        /// </summary>
        public Guid MerchantCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the merchant category navigation property.
        /// </summary>
        public MerchantCategory? MerchantCategory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this merchant category is allowed.
        /// </summary>
        public bool IsAllowed { get; set; } = true;

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
