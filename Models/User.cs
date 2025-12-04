namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents a user in the system.
  /// </summary>
    public class User
    {
     /// <summary>
      /// Gets or sets the unique identifier for the user.
        /// </summary>
     public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Gets or sets the hashed password.
        /// </summary>
        public required string PasswordHash { get; set; }

     /// <summary>
  /// Gets or sets the user's role (CEO, CFO, Admin, Delegate, Auditor).
        /// </summary>
      public required string Role { get; set; }

        /// <summary>
    /// Gets or sets the user's status (Active, Inactive, Suspended).
   /// </summary>
        public required string Status { get; set; } = "Active";

    /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
  /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the department ID.
   /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Gets or sets the department navigation property.
    /// </summary>
     public Department? Department { get; set; }

        /// <summary>
      /// Gets or sets the creation timestamp.
        /// </summary>
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last update timestamp.
    /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

      /// <summary>
        /// Gets or sets a value indicating whether this user is soft deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the virtual cards owned by this user.
      /// </summary>
        public ICollection<VirtualCard> VirtualCards { get; set; } = new List<VirtualCard>();
    }
}
