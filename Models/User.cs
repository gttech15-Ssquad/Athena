namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents an identity that can belong to one or more organizations.
    /// </summary>
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Gets or sets the hashed password.
        /// </summary>
        public required string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the unique 10-digit account number for the user (legacy compatibility).
        /// </summary>
        public string? AccountNumber { get; set; }

        /// <summary>
        /// Global status of the identity (Active, Suspended). Org-specific status lives on OrganizationUser.
        /// </summary>
        public string GlobalStatus { get; set; } = "Active";

        /// <summary>
        /// Backward compatibility shim for existing code paths expecting a Role/Status on User.
        /// </summary>
        public string Role { get; set; } = "Viewer";
        public string Status
        {
            get => GlobalStatus;
            set => GlobalStatus = value;
        }

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
        public Guid? DepartmentId { get; set; }

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
        /// Organization memberships with org-scoped roles.
        /// </summary>
        public ICollection<OrganizationUser> Memberships { get; set; } = new List<OrganizationUser>();

        /// <summary>
        /// Legacy direct cards list is removed; cards are owned via OrganizationUser (OwnerMembershipId).
        /// </summary>
        public ICollection<VirtualCard> VirtualCards { get; set; } = new List<VirtualCard>();

        /// <summary>
        /// Optional personal account balance; org-level balances are keyed by OrganizationId.
        /// </summary>
        public AccountBalance? AccountBalance { get; set; }
    }
}
