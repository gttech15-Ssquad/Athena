namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents an audit log entry for compliance and monitoring.
    /// </summary>
    public class AuditLog
    {
 /// <summary>
/// Gets or sets the unique identifier.
     /// </summary>
  public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
      /// Gets or sets the user ID who performed the action.
  /// </summary>
     public Guid? UserId { get; set; }

        /// <summary>
   /// Gets or sets the user navigation property.
      /// </summary>
     public User? User { get; set; }

  /// <summary>
   /// Gets or sets the action performed (CREATE, UPDATE, DELETE, APPROVE, etc.).
      /// </summary>
   public required string Action { get; set; }

   /// <summary>
   /// Gets or sets the resource type (Card, Transaction, User, etc.).
    /// </summary>
  public required string Resource { get; set; }

     /// <summary>
  /// Gets or sets the resource ID.
    /// </summary>
     public Guid? ResourceId { get; set; }

   /// <summary>
 /// Gets or sets the detailed changes (JSON format).
    /// </summary>
 public string? Changes { get; set; }

    /// <summary>
        /// Gets or sets the IP address from which the action was performed.
      /// </summary>
        public string? IpAddress { get; set; }

 /// <summary>
  /// Gets or sets the user agent.
  /// </summary>
     public string? UserAgent { get; set; }

     /// <summary>
        /// Gets or sets the timestamp of the action.
   /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

 /// <summary>
    /// Gets or sets the result status (SUCCESS, FAILURE).
    /// </summary>
     public required string Status { get; set; } = "SUCCESS";

    /// <summary>
 /// Gets or sets any error message if the action failed.
  /// </summary>
      public string? ErrorMessage { get; set; }
    }
}
