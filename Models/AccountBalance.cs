namespace virtupay_corporate.Models
{
    /// <summary>
/// Represents the main account balance for a user.
    /// This is the master account from which virtual cards are funded.
    /// </summary>
    public class AccountBalance
 {
        /// <summary>
        /// Gets or sets the unique identifier.
    /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

      /// <summary>
   /// Gets or sets the user ID (account owner).
    /// </summary>
        public Guid UserId { get; set; }

  /// <summary>
   /// Gets or sets the user navigation property.
  /// </summary>
        public User? User { get; set; }

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
