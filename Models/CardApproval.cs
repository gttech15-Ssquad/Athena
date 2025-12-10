namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents an approval workflow for high-risk card operations.
    /// </summary>
    public class CardApproval
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
/// Gets or sets the action type (CHANGE_LIMITS, CHANGE_MERCHANTS, ENABLE_INTERNATIONAL, FREEZE_CARD, DELETE_CARD).
     /// </summary>
   public required string ActionType { get; set; }

  /// <summary>
   /// Gets or sets the user ID who requested the approval.
        /// </summary>
        public Guid RequestedBy { get; set; }

        /// <summary>
 /// Gets or sets the requesting user navigation property.
        /// </summary>
   public User? RequestedByUser { get; set; }

    /// <summary>
    /// Gets or sets the user ID who approved/rejected the request.
    /// </summary>
   public Guid? ApprovedBy { get; set; }

      /// <summary>
 /// Gets or sets the approving user navigation property.
 /// </summary>
        public User? ApprovedByUser { get; set; }

  /// <summary>
       /// Gets or sets the approval status (PENDING, APPROVED, REJECTED).
        /// </summary>
     public required string Status { get; set; } = "PENDING";

  /// <summary>
  /// Gets or sets the reason for approval/rejection.
 /// </summary>
        public string? Reason { get; set; }

  /// <summary>
 /// Gets or sets the detailed action data (JSON format).
        /// </summary>
   public string? ActionData { get; set; }

     /// <summary>
  /// Gets or sets the creation timestamp.
  /// </summary>
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
   /// Gets or sets the approval/rejection timestamp.
 /// </summary>
    public DateTime? ResolvedAt { get; set; }

      /// <summary>
   /// Gets or sets the expiration timestamp for this approval request.
        /// </summary>
 public DateTime ExpiresAt { get; set; }
    }
}
