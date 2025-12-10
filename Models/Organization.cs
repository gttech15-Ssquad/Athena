using System;
using System.Collections.Generic;

namespace virtupay_corporate.Models
{
    /// <summary>
    /// Represents a tenant/organization. All other entities are scoped to an organization.
    /// </summary>
    public class Organization
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? Industry { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrganizationUser> Members { get; set; } = new List<OrganizationUser>();
        public ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}

