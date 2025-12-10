using System.ComponentModel.DataAnnotations;

namespace virtupay_corporate.DTOs
{
    /// <summary>
    /// DTO for creating a virtual card.
    /// </summary>
    public class CreateCardRequest
    {
        /// <summary>
        /// Gets or sets the cardholder name.
        /// </summary>
   [Required(ErrorMessage = "Cardholder name is required")]
  [MaxLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
   public required string CardholderName { get; set; }

        /// <summary>
    /// Gets or sets the card nickname.
        /// </summary>
        [MaxLength(100, ErrorMessage = "Nickname cannot exceed 100 characters")]
        public string? Nickname { get; set; }

     /// <summary>
        /// Gets or sets the department ID.
/// </summary>
        public Guid? DepartmentId { get; set; }

  /// <summary>
 /// Gets or sets the card type.
   /// </summary>
    public string CardType { get; set; } = "CREDIT";

    /// <summary>
    /// Gets or sets the currency code.
        /// </summary>
        public string Currency { get; set; } = "NGN";

    /// <summary>
        /// Gets or sets whether international transactions are allowed.
      /// </summary>
        public bool AllowInternational { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating card details.
    /// </summary>
    public class UpdateCardRequest
 {
     /// <summary>
      /// Gets or sets the card nickname.
     /// </summary>
    [MaxLength(100)]
public string? Nickname { get; set; }

        /// <summary>
   /// Gets or sets whether international transactions are allowed.
     /// </summary>
   public bool? AllowInternational { get; set; }

        /// <summary>
    /// Gets or sets the card status.
        /// </summary>
  [RegularExpression("^(ACTIVE|INACTIVE|FROZEN)$", ErrorMessage = "Invalid card status")]
  public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for freezing/unfreezing a card.
    /// </summary>
 public class FreezeCardRequest
    {
    /// <summary>
        /// Gets or sets the reason for freezing.
        /// </summary>
        [Required(ErrorMessage = "Freeze reason is required")]
        [MaxLength(500)]
    public required string Reason { get; set; }
    }

    /// <summary>
 /// DTO for card response.
    /// </summary>
    public class CardResponse
    {
  /// <summary>
        /// Gets or sets the card ID.
        /// </summary>
        public Guid Id { get; set; }

   /// <summary>
        /// Gets or sets the card number (masked).
        /// </summary>
        public string? CardNumber { get; set; }

        /// <summary>
      /// Gets or sets the cardholder name.
        /// </summary>
        public string? CardholderName { get; set; }

        /// <summary>
  /// Gets or sets the card nickname.
        /// </summary>
        public string? Nickname { get; set; }

   /// <summary>
 /// Gets or sets the expiry date.
        /// </summary>
  public DateTime ExpiryDate { get; set; }

        /// <summary>
     /// Gets or sets the card status.
        /// </summary>
     public string? Status { get; set; }

   /// <summary>
    /// Gets or sets the card type.
        /// </summary>
        public string? CardType { get; set; }

  /// <summary>
    /// Gets or sets the currency.
   /// </summary>
    public string? Currency { get; set; }

/// <summary>
     /// Gets or sets whether international transactions are allowed.
        /// </summary>
        public bool AllowInternational { get; set; }

/// <summary>
    /// Gets or sets the freeze reason.
/// </summary>
        public string? FreezeReason { get; set; }

 /// <summary>
  /// Gets or sets the frozen timestamp.
        /// </summary>
   public DateTime? FrozenAt { get; set; }

  /// <summary>
        /// Gets or sets the creation timestamp.
  /// </summary>
        public DateTime CreatedAt { get; set; }

     /// <summary>
     /// Gets or sets the last update timestamp.
        /// </summary>
    public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
 /// DTO for detailed card information.
    /// </summary>
    public class CardDetailResponse
    {
     /// <summary>
   /// Gets or sets the card ID.
   /// </summary>
        public Guid Id { get; set; }

  /// <summary>
        /// Gets or sets the full 16-digit card number.
        /// </summary>
       public string? CardNumber { get; set; }

     /// <summary>
     /// Gets or sets the 3-digit CVV.
        /// </summary>
      public string? CVV { get; set; }

     /// <summary>
/// Gets or sets the card expiry date (MM/YY format).
        /// </summary>
        public string? ExpiryDateFormatted { get; set; }

    /// <summary>
        /// Gets or sets the cardholder name.
        /// </summary>
  public string? CardholderName { get; set; }

    /// <summary>
        /// Gets or sets the card nickname.
   /// </summary>
   public string? Nickname { get; set; }

   /// <summary>
        /// Gets or sets the card status.
        /// </summary>
   public string? Status { get; set; }

   /// <summary>
    /// Gets or sets the card type.
        /// </summary>
      public string? CardType { get; set; }

  /// <summary>
    /// Gets or sets the currency.
   /// </summary>
    public string? Currency { get; set; }

/// <summary>
     /// Gets or sets whether international transactions are allowed.
    /// </summary>
        public bool AllowInternational { get; set; }

/// <summary>
    /// Gets or sets the freeze reason.
/// </summary>
        public string? FreezeReason { get; set; }

 /// <summary>
  /// Gets or sets the frozen timestamp.
        /// </summary>
   public DateTime? FrozenAt { get; set; }

  /// <summary>
        /// Gets or sets the creation timestamp.
  /// </summary>
        public DateTime CreatedAt { get; set; }

     /// <summary>
     /// Gets or sets the last update timestamp.
        /// </summary>
    public DateTime UpdatedAt { get; set; }

  /// <summary>
      /// Gets or sets the card balance.
    /// </summary>
 public BalanceResponse? Balance { get; set; }

  /// <summary>
        /// Gets or sets the card limits.
  /// </summary>
    public List<CardLimitResponse> Limits { get; set; } = new List<CardLimitResponse>();

   /// <summary>
        /// Gets or sets the merchant restrictions.
     /// </summary>
   public List<MerchantRestrictionResponse> MerchantRestrictions { get; set; } = new List<MerchantRestrictionResponse>();

     /// <summary>
   /// Gets or sets the recent transactions.
    /// </summary>
  public List<TransactionResponse> RecentTransactions { get; set; } = new List<TransactionResponse>();
    }

    /// <summary>
    /// DTO for viewing full card details including sensitive information.
    /// Only Approvers (APP role) can view this information.
    /// </summary>
    public class CardFullDetailsResponse
    {
/// <summary>
     /// Gets or sets the card ID.
      /// </summary>
        public Guid Id { get; set; }

 /// <summary>
        /// Gets or sets the full 16-digit card number.
        /// </summary>
       public string? CardNumber { get; set; }

        /// <summary>
     /// Gets or sets the 3-digit CVV.
        /// </summary>
        public string? CVV { get; set; }

    /// <summary>
     /// Gets or sets the card expiry date.
        /// </summary>
    public DateTime ExpiryDate { get; set; }

/// <summary>
        /// Gets or sets the cardholder name.
      /// </summary>
  public string? CardholderName { get; set; }

  /// <summary>
   /// Gets or sets the card status.
  /// </summary>
        public string? Status { get; set; }

    /// <summary>
        /// Gets or sets the card type.
        /// </summary>
        public string? CardType { get; set; }

        /// <summary>
     /// Gets or sets the currency.
   /// </summary>
        public string? Currency { get; set; }

        /// <summary>
  /// Gets or sets the creation timestamp.
        /// </summary>
    public DateTime CreatedAt { get; set; }
}
    /// <summary>
    /// DTO for masking card number.
    /// </summary>
    public static class CardNumberMasking
    {
    /// <summary>
        /// Masks card number showing only last 4 digits.
      /// </summary>
        public static string MaskCardNumber(string cardNumber)
        {
      if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
  return cardNumber;

  return $"****-****-****-{cardNumber.Substring(cardNumber.Length - 4)}";
    }
    }
}
