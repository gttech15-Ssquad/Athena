using System;

namespace virtupay_corporate.Models
{
    /// <summary>
    /// Links a user to an organization with an organization-scoped role.
    /// </summary>
    public class OrganizationUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        public Guid UserId { get; set; }
        public User? User { get; set; }

        /// <summary>
        /// Organization-level role (Owner, Admin, Approver, Viewer, Auditor).
        /// </summary>
        public string OrgRole { get; set; } = "Viewer";

        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

