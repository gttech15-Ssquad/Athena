using System.ComponentModel.DataAnnotations;

namespace virtupay_corporate.DTOs
{
    public class CreateOrganizationRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Industry { get; set; }
    }

    public class AddMemberRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string OrgRole { get; set; } = "Viewer";
    }

    public class OrganizationResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateOrganizationRequest
    {
        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Industry { get; set; }

        [RegularExpression("^(Active|Inactive)$")]
        public string? Status { get; set; }
    }

    public class OrganizationMembersResponse
    {
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public List<OrganizationMemberResponse> Members { get; set; } = new List<OrganizationMemberResponse>();
        public int TotalMembers { get; set; }
    }

    public class UpdateMemberRoleRequest
    {
        [Required(ErrorMessage = "Organization role is required")]
        [RegularExpression("^(Owner|Admin|Approver|Viewer|Auditor)$", ErrorMessage = "Role must be Owner, Admin, Approver, Viewer, or Auditor")]
        public string OrgRole { get; set; } = string.Empty;
    }
}

