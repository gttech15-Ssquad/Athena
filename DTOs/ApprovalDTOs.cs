using System.ComponentModel.DataAnnotations;

namespace virtupay_corporate.DTOs
{
 /// <summary>
    /// DTO for requesting approval.
    /// </summary>
 public class RequestApprovalRequest
  {
/// <summary>
        /// Gets or sets the action type.
   /// </summary>
        [Required(ErrorMessage = "Action type is required")]
        [RegularExpression("^(CHANGE_LIMITS|CHANGE_MERCHANTS|ENABLE_INTERNATIONAL|FREEZE_CARD|DELETE_CARD|CREATE_CARD)$", 
  ErrorMessage = "Invalid action type")]
      public required string ActionType { get; set; }

        /// <summary>
        /// Gets or sets the action data (JSON).
        /// </summary>
        [MaxLength(2000)]
  public string? ActionData { get; set; }

        /// <summary>
        /// Gets or sets the reason for the request.
  /// </summary>
    [MaxLength(500)]
     public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO for approving a request.
    /// </summary>
    public class ApproveApprovalRequest
    {
        /// <summary>
        /// Gets or sets the approval comment.
/// </summary>
    [MaxLength(500)]
        public string? Comment { get; set; }
    }

    /// <summary>
    /// DTO for rejecting a request.
 /// </summary>
    public class RejectApprovalRequest
    {
  /// <summary>
        /// Gets or sets the rejection reason.
  /// </summary>
  [Required(ErrorMessage = "Rejection reason is required")]
        [MaxLength(500)]
 public required string Reason { get; set; }

     /// <summary>
   /// Gets or sets additional details.
        /// </summary>
   [MaxLength(1000)]
  public string? Details { get; set; }
  }

  /// <summary>
    /// DTO for approval response.
    /// </summary>
    public class ApprovalResponse
    {
        /// <summary>
        /// Gets or sets the approval ID.
    /// </summary>
  public Guid Id { get; set; }

 /// <summary>
     /// Gets or sets the card ID.
    /// </summary>
  public Guid CardId { get; set; }

  /// <summary>
    /// Gets or sets the action type.
    /// </summary>
 public string? ActionType { get; set; }

        /// <summary>
        /// Gets or sets who requested the approval.
 /// </summary>
     public UserResponse? RequestedByUser { get; set; }

        /// <summary>
/// Gets or sets who approved/rejected the request.
        /// </summary>
        public UserResponse? ApprovedByUser { get; set; }

  /// <summary>
    /// Gets or sets the approval status.
      /// </summary>
 public string? Status { get; set; }

        /// <summary>
    /// Gets or sets the reason for approval/rejection.
/// </summary>
      public string? Reason { get; set; }

        /// <summary>
        /// Gets or sets the action data.
        /// </summary>
 public string? ActionData { get; set; }

    /// <summary>
     /// Gets or sets the creation timestamp.
 /// </summary>
    public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the resolution timestamp.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// Gets or sets the expiration timestamp.
        /// </summary>
     public DateTime ExpiresAt { get; set; }

     /// <summary>
        /// Gets or sets whether the approval is still pending.
        /// </summary>
   public bool IsPending => Status == "PENDING" && ExpiresAt > DateTime.UtcNow;
    }

/// <summary>
    /// DTO for approval workflow step.
    /// </summary>
    public class ApprovalWorkflowResponse
  {
      /// <summary>
        /// Gets or sets the step number.
/// </summary>
        public int StepNumber { get; set; }

        /// <summary>
   /// Gets or sets the required role.
     /// </summary>
    public string? RequiredRole { get; set; }

        /// <summary>
     /// Gets or sets whether this step is complete.
        /// </summary>
   public bool IsComplete { get; set; }

   /// <summary>
        /// Gets or sets the approval by user.
        /// </summary>
        public UserResponse? ApprovedBy { get; set; }

        /// <summary>
   /// Gets or sets the approval timestamp.
      /// </summary>
   public DateTime? ApprovedAt { get; set; }

      /// <summary>
  /// Gets or sets the approval comment.
      /// </summary>
        [MaxLength(500)]
      public string? Comment { get; set; }
    }

    /// <summary>
    /// DTO for approval requirements.
    /// </summary>
public class ApprovalRequirementsResponse
    {
        /// <summary>
        /// Gets or sets the action type.
        /// </summary>
        public string? ActionType { get; set; }

     /// <summary>
        /// Gets or sets the required approval role.
        /// </summary>
     public string? RequiredRole { get; set; }

/// <summary>
        /// Gets or sets whether approval is required.
  /// </summary>
     public bool IsRequired { get; set; }

   /// <summary>
        /// Gets or sets the approval description.
        /// </summary>
    public string? Description { get; set; }

     /// <summary>
     /// Gets or sets the maximum duration in hours before expiration.
        /// </summary>
   public int MaxDurationHours { get; set; } = 48;
    }
}
