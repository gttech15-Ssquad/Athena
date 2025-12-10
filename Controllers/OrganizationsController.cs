using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using virtupay_corporate.Data;
using virtupay_corporate.DTOs;
using virtupay_corporate.Models;
using virtupay_corporate.Services;
using System.Security.Claims;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace virtupay_corporate.Controllers
{
    /// <summary>
    /// Organization management endpoints (multi-tenant entry point).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly CorporateDbContext _db;
        private readonly ILogger<OrganizationsController> _logger;

        public OrganizationsController(
            IOrganizationService organizationService,
            CorporateDbContext db, 
            ILogger<OrganizationsController> logger)
        {
            _organizationService = organizationService;
            _db = db;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = "OrgRoleAdmin")]
        public async Task<ActionResult<OrganizationResponse>> CreateOrganization([FromBody] CreateOrganizationRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var org = new Organization
            {
                Name = request.Name,
                Industry = request.Industry
            };

            _db.Organizations.Add(org);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrganization), new { orgId = org.Id }, new OrganizationResponse
            {
                Id = org.Id,
                Name = org.Name,
                Industry = org.Industry,
                Status = org.Status,
                CreatedAt = org.CreatedAt,
                UpdatedAt = org.UpdatedAt
            });
        }

        [HttpGet("{orgId}")]
        public async Task<ActionResult<OrganizationResponse>> GetOrganization(Guid orgId)
        {
            var org = await _db.Organizations.FindAsync(orgId);
            if (org == null) return NotFound();

            return Ok(new OrganizationResponse
            {
                Id = org.Id,
                Name = org.Name,
                Industry = org.Industry,
                Status = org.Status,
                CreatedAt = org.CreatedAt,
                UpdatedAt = org.UpdatedAt
            });
        }

        /// <summary>
        /// Get all members of an organization
        /// </summary>
        [HttpGet("{orgId}/members")]
        [Authorize]
        [ProducesResponseType(typeof(OrganizationMembersResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrganizationMembers(Guid orgId)
        {
            try
            {
                // Verify user has access to this organization
                var (currentOrgId, _) = GetOrganizationContext();
                if (currentOrgId != orgId)
                {
                    return Forbid();
                }

                var org = await _organizationService.GetOrganizationAsync(orgId);
                if (org == null)
                    return NotFound(new ErrorResponse { Code = "ORGANIZATION_NOT_FOUND", Message = "Organization not found" });

                var members = await _organizationService.GetOrganizationMembersAsync(orgId);
                var response = new OrganizationMembersResponse
                {
                    OrganizationId = orgId,
                    OrganizationName = org.Name,
                    TotalMembers = members.Count,
                    Members = members.Select(m => new OrganizationMemberResponse
                    {
                        Id = m.Id,
                        UserId = m.UserId,
                        UserEmail = m.User?.Email,
                        UserFirstName = m.User?.FirstName,
                        UserLastName = m.User?.LastName,
                        OrgRole = m.OrgRole,
                        Status = m.Status,
                        CreatedAt = m.CreatedAt
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization members");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred"
                });
            }
        }

        [HttpPost("{orgId}/members")]
        [Authorize(Policy = "OrgRoleAdmin")]
        [ProducesResponseType(typeof(OrganizationMemberResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddMember(Guid orgId, [FromBody] AddMemberRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var membership = await _organizationService.AddMemberAsync(orgId, request.UserId, request.OrgRole);
                if (membership == null)
                    return BadRequest(new ErrorResponse { Code = "ADD_MEMBER_FAILED", Message = "Failed to add member" });

                var user = await _db.Users.FindAsync(request.UserId);
                var response = new OrganizationMemberResponse
                {
                    Id = membership.Id,
                    UserId = membership.UserId,
                    UserEmail = user?.Email,
                    UserFirstName = user?.FirstName,
                    UserLastName = user?.LastName,
                    OrgRole = membership.OrgRole,
                    Status = membership.Status,
                    CreatedAt = membership.CreatedAt
                };

                _logger.LogInformation("Member {UserId} added to organization {OrgId} with role {Role}", request.UserId, orgId, request.OrgRole);
                return CreatedAtAction(nameof(GetOrganizationMembers), new { orgId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred"
                });
            }
        }

        /// <summary>
        /// Update organization settings
        /// </summary>
        [HttpPut("{orgId}")]
        [Authorize(Policy = "OrgRoleAdmin")]
        [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrganization(Guid orgId, [FromBody] UpdateOrganizationRequest request)
        {
            try
            {
                var org = await _organizationService.GetOrganizationAsync(orgId);
                if (org == null)
                    return NotFound(new ErrorResponse { Code = "ORGANIZATION_NOT_FOUND", Message = "Organization not found" });

                if (!string.IsNullOrEmpty(request.Name))
                    org.Name = request.Name;
                if (!string.IsNullOrEmpty(request.Industry))
                    org.Industry = request.Industry;
                if (!string.IsNullOrEmpty(request.Status))
                    org.Status = request.Status;

                org.UpdatedAt = DateTime.UtcNow;
                _db.Organizations.Update(org);
                await _db.SaveChangesAsync();

                var response = new OrganizationResponse
                {
                    Id = org.Id,
                    Name = org.Name,
                    Industry = org.Industry,
                    Status = org.Status,
                    CreatedAt = org.CreatedAt,
                    UpdatedAt = org.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred"
                });
            }
        }

        /// <summary>
        /// Update a member's role in the organization
        /// </summary>
        [HttpPut("{orgId}/members/{userId}")]
        [Authorize(Policy = "OrgRoleAdmin")]
        [ProducesResponseType(typeof(OrganizationMemberResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMemberRole(Guid orgId, Guid userId, [FromBody] UpdateMemberRoleRequest request)
        {
            try
            {
                var success = await _organizationService.UpdateMemberRoleAsync(orgId, userId, request.OrgRole);
                if (!success)
                    return NotFound(new ErrorResponse { Code = "MEMBER_NOT_FOUND", Message = "Member not found" });

                var membership = await _organizationService.GetMembershipAsync(orgId, userId);
                var user = await _db.Users.FindAsync(userId);

                var response = new OrganizationMemberResponse
                {
                    Id = membership!.Id,
                    UserId = membership.UserId,
                    UserEmail = user?.Email,
                    UserFirstName = user?.FirstName,
                    UserLastName = user?.LastName,
                    OrgRole = membership.OrgRole,
                    Status = membership.Status,
                    CreatedAt = membership.CreatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member role");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred"
                });
            }
        }

        /// <summary>
        /// Remove a member from the organization
        /// </summary>
        [HttpDelete("{orgId}/members/{userId}")]
        [Authorize(Policy = "OrgRoleAdmin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveMember(Guid orgId, Guid userId)
        {
            try
            {
                var success = await _organizationService.RemoveMemberAsync(orgId, userId);
                if (!success)
                    return NotFound(new ErrorResponse { Code = "MEMBER_NOT_FOUND", Message = "Member not found" });

                _logger.LogInformation("Member {UserId} removed from organization {OrgId}", userId, orgId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred"
                });
            }
        }

        private Guid? GetOrgIdFromClaim()
        {
            var claim = User.FindFirst("orgId");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : (Guid?)null;
        }

        private string? GetOrgRoleFromClaim() => User.FindFirst(ClaimTypes.Role)?.Value;

        private (Guid? organizationId, Guid? membershipId) GetOrganizationContext()
        {
            var orgIdClaim = User.FindFirst("orgId");
            var membershipIdClaim = User.FindFirst("membershipId");
            
            Guid? orgId = orgIdClaim != null && Guid.TryParse(orgIdClaim.Value, out var oid) ? oid : null;
            Guid? membershipId = membershipIdClaim != null && Guid.TryParse(membershipIdClaim.Value, out var mid) ? mid : null;
            
            return (orgId, membershipId);
        }
    }
}

