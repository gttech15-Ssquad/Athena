namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents a department in the organization.
    /// </summary>
    public class Department
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the department name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the department status (Active, Inactive).
 /// </summary>
        public required string Status { get; set; } = "Active";

        /// <summary>
    /// Gets or sets the department budget.
      /// </summary>
public decimal Budget { get; set; }

        /// <summary>
        /// Gets or sets the manager ID.
     /// </summary>
        public int? ManagerId { get; set; }

        /// <summary>
  /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last update timestamp.
/// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

      /// <summary>
        /// Gets or sets a value indicating whether this department is soft deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

      /// <summary>
        /// Gets or sets the users in this department.
        /// </summary>
   public ICollection<User> Users { get; set; } = new List<User>();
    }
}
