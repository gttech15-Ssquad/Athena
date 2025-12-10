using System.ComponentModel.DataAnnotations;

namespace virtupay_corporate.DTOs
{
    /// <summary>
    /// DTO for balance response.
    /// </summary>
 public class BalanceResponse
    {
 /// <summary>
/// Gets or sets the available balance.
        /// </summary>
        public decimal AvailableBalance { get; set; }

  /// <summary>
        /// Gets or sets the reserved balance.
        /// </summary>
        public decimal ReservedBalance { get; set; }

      /// <summary>
        /// Gets or sets the used balance.
      /// </summary>
        public decimal UsedBalance { get; set; }

        /// <summary>
        /// Gets or sets the total balance.
        /// </summary>
  public decimal TotalBalance => AvailableBalance + ReservedBalance + UsedBalance;

  /// <summary>
        /// Gets or sets the currency.
 /// </summary>
       public string? Currency { get; set; }

      /// <summary>
    /// Gets or sets the last updated timestamp.
      /// </summary>
     public DateTime LastUpdated { get; set; }

      /// <summary>
  /// Gets or sets the percentage used.
 /// </summary>
   public decimal PercentageUsed => TotalBalance > 0 ? (UsedBalance / TotalBalance) * 100 : 0;
    }

    /// <summary>
    /// DTO for fund/add balance request.
    /// </summary>
    public class FundBalanceRequest
  {
/// <summary>
        /// Gets or sets the amount to add to the balance.
        /// </summary>
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be between 0.01 and 999999.99")]
 public decimal Amount { get; set; }

/// <summary>
        /// Gets or sets the reason for adding funds.
        /// </summary>
        [Required(ErrorMessage = "Reason is required")]
        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }

        /// <summary>
        /// Gets or sets the reference ID for the funding transaction.
   /// </summary>
[MaxLength(100)]
        public string? ReferenceId { get; set; }
    }

    /// <summary>
    /// DTO for card limit request.
    /// </summary>
    public class SetCardLimitRequest
    {
 /// <summary>
 /// Gets or sets the limit type.
     /// </summary>
        [Required(ErrorMessage = "Limit type is required")]
        [RegularExpression("^(SOFT_LIMIT|HARD_LIMIT|DAILY|WEEKLY|MONTHLY|PER_TRANSACTION)$")]
 public required string LimitType { get; set; }

        /// <summary>
    /// Gets or sets the maximum amount.
        /// </summary>
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, 999999.99)]
    public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the period.
 /// </summary>
 [Required(ErrorMessage = "Period is required")]
      [RegularExpression("^(DAILY|WEEKLY|MONTHLY|NO_LIMIT)$")]
   public required string Period { get; set; }

     /// <summary>
        /// Gets or sets the warning threshold percentage.
/// </summary>
        [Range(1, 100)]
      public decimal Threshold { get; set; } = 80;
    }

    /// <summary>
    /// DTO for card limit response.
    /// </summary>
    public class CardLimitResponse
    {
  /// <summary>
        /// Gets or sets the limit ID.
   /// </summary>
    public Guid Id { get; set; }

     /// <summary>
        /// Gets or sets the card ID.
        /// </summary>
     public Guid CardId { get; set; }

 /// <summary>
        /// Gets or sets the limit type.
        /// </summary>
 public string? LimitType { get; set; }

        /// <summary>
   /// Gets or sets the maximum amount.
   /// </summary>
        public decimal Amount { get; set; }

  /// <summary>
     /// Gets or sets the period.
     /// </summary>
        public string? Period { get; set; }

  /// <summary>
      /// Gets or sets the warning threshold.
     /// </summary>
        public decimal Threshold { get; set; }

   /// <summary>
   /// Gets or sets the used amount.
        /// </summary>
    public decimal UsedAmount { get; set; }

    /// <summary>
        /// Gets or sets the available amount.
  /// </summary>
        public decimal AvailableAmount => Amount - UsedAmount;

 /// <summary>
    /// Gets or sets the percentage used.
 /// </summary>
     public decimal PercentageUsed => Amount > 0 ? (UsedAmount / Amount) * 100 : 0;

     /// <summary>
 /// Gets or sets whether the warning threshold is reached.
      /// </summary>
   public bool IsWarningReached => PercentageUsed >= Threshold;

 /// <summary>
/// Gets or sets whether this limit is active.
      /// </summary>
     public bool IsActive { get; set; }

        /// <summary>
 /// Gets or sets the creation timestamp.
     /// </summary>
     public DateTime CreatedAt { get; set; }
 }

  /// <summary>
    /// DTO for merchant restriction request.
    /// </summary>
 public class SetMerchantRestrictionRequest
    {
   /// <summary>
        /// Gets or sets the merchant category ID.
        /// </summary>
    [Required(ErrorMessage = "Merchant category ID is required")]
   public Guid MerchantCategoryId { get; set; }

       /// <summary>
        /// Gets or sets whether this merchant is allowed.
    /// </summary>
 public bool IsAllowed { get; set; } = true;
    }

    /// <summary>
 /// DTO for merchant restriction response.
    /// </summary>
    public class MerchantRestrictionResponse
    {
      /// <summary>
     /// Gets or sets the merchant category ID.
    /// </summary>
     public Guid MerchantCategoryId { get; set; }

    /// <summary>
        /// Gets or sets the merchant category name.
/// </summary>
 public string? MerchantCategoryName { get; set; }

        /// <summary>
   /// Gets or sets the merchant category code.
 /// </summary>
 public string? MerchantCategoryCode { get; set; }

        /// <summary>
      /// Gets or sets whether this merchant is allowed.
        /// </summary>
        public bool IsAllowed { get; set; }

    /// <summary>
        /// Gets or sets the creation timestamp.
     /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
/// DTO for merchant category response.
    /// </summary>
    public class MerchantCategoryResponse
    {
     /// <summary>
        /// Gets or sets the category ID.
        /// </summary>
   public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the category name.
 /// </summary>
        public string? Name { get; set; }

        /// <summary>
      /// Gets or sets the category code.
        /// </summary>
   public string? Code { get; set; }

        /// <summary>
     /// Gets or sets the category description.
    /// </summary>
     public string? Description { get; set; }
 }

    /// <summary>
    /// DTO for international transaction request.
    /// </summary>
   public class SetInternationalTransactionRequest
    {
 /// <summary>
    /// Gets or sets whether international transactions are allowed.
        /// </summary>
        [Required(ErrorMessage = "Allow international is required")]
    public bool AllowInternational { get; set; }

       /// <summary>
  /// Gets or sets the reason for the change.
 /// </summary>
     [MaxLength(500)]
      public string? Reason { get; set; }
    }
}
