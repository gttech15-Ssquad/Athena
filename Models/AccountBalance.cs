namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents an organization or user balance. OrganizationId is required; user membership is optional.
    /// </summary>
    public class AccountBalance
    {
        /// <summary>
        /// Gets or sets the unique identifier.
    /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        /// <summary>
        /// Optional user-specific balance (legacy/personal). Prefer OrganizationUser for org scope.
        /// </summary>
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        public Guid? OrganizationUserId { get; set; }
        public OrganizationUser? OrganizationUser { get; set; }

      /// <summary>
     /// Gets or sets the current available balance.
   /// </summary>
        public decimal AvailableBalance { get; set; }

/// <summary>
  /// Gets or sets the total balance ever added to the account.
  /// </summary>
        public decimal TotalFunded { get; set; }

   /// <summary>
   /// Gets or sets the total amount withdrawn/spent across all cards.
    /// </summary>
        public decimal TotalWithdrawn { get; set; }

     /// <summary>
   /// Gets or sets the currency code (ISO 4217).
/// </summary>
    public required string Currency { get; set; } = "NGN";

        /// <summary>
        /// Gets or sets the creation timestamp.
  /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

     /// <summary>
  /// Gets or sets the last updated timestamp.
/// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

/// <summary>
 /// Gets or sets the account transactions (funding/withdrawal history).
  /// </summary>
        public ICollection<AccountTransaction> Transactions { get; set; } = new List<AccountTransaction>();

     /// <summary>
/// Calculates the balance after deducting a transaction amount.
        /// </summary>
        /// <param name="amount">The transaction amount to deduct.</param>
    /// <returns>The remaining balance after deduction, or -1 if insufficient funds.</returns>
   public decimal DeductBalance(decimal amount)
     {
     if (amount <= 0)
  return -1;

if (AvailableBalance < amount)
  return -1;

   AvailableBalance -= amount;
         TotalWithdrawn += amount;
        return AvailableBalance;
    }

  /// <summary>
    /// Adds funds to the account balance.
   /// </summary>
        /// <param name="amount">The amount to add.</param>
  /// <returns>The new balance after adding funds.</returns>
      public decimal AddFunds(decimal amount)
   {
     if (amount <= 0)
return -1;

   AvailableBalance += amount;
       TotalFunded += amount;
         return AvailableBalance;
}
    }
}
