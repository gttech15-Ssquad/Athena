using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using virtupay_corporate.DTOs;
using virtupay_corporate.Services;
using System.Security.Claims;

namespace virtupay_corporate.Controllers
{
/// <summary>
    /// Approval Workflow Management
    /// 
    /// Manages approval workflows for sensitive card operations. Implements hierarchical approval based on user roles.
    /// Different actions require different approval levels (e.g., card deletion requires CEO, freezing requires CFO).
    /// All endpoints require JWT authentication. Role-based access control is enforced for approval operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ApprovalsController : ControllerBase
    {
        private readonly IApprovalService _approvalService;
        private readonly IAuditService _auditService;
 private readonly ILogger<ApprovalsController> _logger;

        public ApprovalsController(
    IApprovalService approvalService,
     IAuditService auditService,
            ILogger<ApprovalsController> logger)
        {
    _approvalService = approvalService;
       _auditService = auditService;
      _logger = logger;
 }

        /// <summary>
        /// Request approval for a sensitive action
        /// 
        /// Initiates an approval workflow for sensitive card operations (freeze, delete, change limits, etc).
        /// The approval is routed to the appropriate manager based on action type and role hierarchy.
        /// Actions like "FREEZE_CARD" go to CFO, "DELETE_CARD" go to CEO, etc.
        /// The approval request expires after 48 hours if not resolved.
        /// </summary>
  /// <param name="cardId">The ID of the card the action applies to</param>
        /// <param name="request">Approval request with action type and supporting data</param>
  /// <returns>Created approval request with approval ID and routing information</returns>
        [HttpPost]
     [ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
     public async Task<IActionResult> RequestApproval(Guid cardId, [FromBody] RequestApprovalRequest request)
        {
   try
 {
        if (!ModelState.IsValid)
    return BadRequest(ModelState);

   var userId = GetUserId();
      if (!userId.HasValue)
    return Unauthorized();

    var approval = await _approvalService.RequestApprovalAsync(cardId, request.ActionType, userId.Value, request.ActionData);

  if (approval == null)
          return BadRequest(new ErrorResponse { Code = "APPROVAL_REQUEST_FAILED", Message = "Failed to request approval" });

    _logger.LogInformation("Approval requested for card {CardId} by user {UserId}", cardId, userId);
     return CreatedAtAction(nameof(GetApproval), new { approvalId = approval.Id }, MapApprovalToResponse(approval));
}
    catch (Exception ex)
         {
         _logger.LogError(ex, "Error requesting approval");
 return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
         }
        }

        /// <summary>
        /// Get all pending approvals
 /// 
        /// Retrieves all approval requests that are waiting for action. Only accessible by managers and above (CEO, CFO, Admin).
 /// Shows approvals from all cards and team members. Useful for approval dashboards and workflow management.
     /// Sorted by creation date, with oldest requests shown first (FIFO processing).
     /// </summary>
        /// <returns>List of all pending approvals awaiting action</returns>
      [HttpGet("pending")]
        [Authorize(Roles = "CEO,CFO,Admin")]
        [ProducesResponseType(typeof(List<ApprovalResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingApprovals()
     {
    try
  {
        var approvals = await _approvalService.GetPendingApprovalsAsync();
     var response = approvals.Select(MapApprovalToResponse).ToList();
    return Ok(response);
        }
        catch (Exception ex)
       {
     _logger.LogError(ex, "Error getting pending approvals");
      return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
  }
     }

 /// <summary>
        /// Get a specific approval request by ID
     /// 
        /// Retrieves detailed information about a single approval request including action type, status,
        /// requested by, approval comments, and decision timestamps. Useful for tracking individual approval history.
        /// </summary>
        /// <param name="approvalId">The ID of the approval request to retrieve</param>
    /// <returns>Detailed approval request information</returns>
 [HttpGet("{approvalId}")]
   [ProducesResponseType(typeof(ApprovalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetApproval(Guid approvalId)
 {
  try
 {
      var approvals = await _approvalService.GetPendingApprovalsAsync();
   var approval = approvals.FirstOrDefault(a => a.Id == approvalId);

     if (approval == null)
    return NotFound(new ErrorResponse { Code = "APPROVAL_NOT_FOUND", Message = "Approval not found" });

        return Ok(MapApprovalToResponse(approval));
    }
    catch (Exception ex)
     {
             _logger.LogError(ex, "Error getting approval");
           return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
     }
        }

 /// <summary>
    /// Approve an approval request
        /// 
 /// Authorizes a pending approval request, allowing the requested action to proceed.
   /// Only authorized managers (CEO, CFO, Admin) can approve based on action type.
  /// The approver's comment is recorded for the audit trail. Approvals expire after 48 hours.
        /// </summary>
   /// <param name="approvalId">The ID of the approval request to approve</param>
     /// <param name="request">Approval decision with optional comment for the record</param>
    /// <returns>Success message if approval was granted</returns>
    [HttpPut("{approvalId}/approve")]
        [Authorize(Roles = "CEO,CFO,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
      public async Task<IActionResult> ApproveApproval(Guid approvalId, [FromBody] ApproveApprovalRequest request)
        {
        try
         {
   var userId = GetUserId();
if (!userId.HasValue)
return Unauthorized();

      var result = await _approvalService.ApproveAsync(approvalId, userId.Value, request.Comment);
     if (!result)
      return NotFound(new ErrorResponse { Code = "APPROVAL_NOT_FOUND", Message = "Approval not found" });

    _logger.LogInformation("Approval {ApprovalId} approved by user {UserId}", approvalId, userId);
   return Ok(new { message = "Approval approved successfully" });
            }
   catch (Exception ex)
        {
       _logger.LogError(ex, "Error approving approval");
          return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
            }
        }

/// <summary>
        /// Reject an approval request
 /// 
  /// Denies a pending approval request, preventing the requested action from proceeding.
     /// Only authorized managers can reject. The rejection reason is recorded for audit and to inform the requester
  /// why their action was not approved. The requester can submit a new approval request if needed.
        /// </summary>
   /// <param name="approvalId">The ID of the approval request to reject</param>
        /// <param name="request">Rejection reason explaining why the approval was denied</param>
        /// <returns>Success message if rejection was processed</returns>
   [HttpPut("{approvalId}/reject")]
        [Authorize(Roles = "CEO,CFO,Admin")]
 [ProducesResponseType(StatusCodes.Status200OK)]
     [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> RejectApproval(Guid approvalId, [FromBody] RejectApprovalRequest request)
   {
  try
  {
 var userId = GetUserId();
      if (!userId.HasValue)
  return Unauthorized();

   var result = await _approvalService.RejectAsync(approvalId, userId.Value, request.Reason);
       if (!result)
   return NotFound(new ErrorResponse { Code = "APPROVAL_NOT_FOUND", Message = "Approval not found" });

      _logger.LogInformation("Approval {ApprovalId} rejected by user {UserId}", approvalId, userId);
       return Ok(new { message = "Approval rejected successfully" });
        }
     catch (Exception ex)
            {
    _logger.LogError(ex, "Error rejecting approval");
      return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
        }
     }

   /// <summary>
      /// Get approval history for a specific card
      /// 
  /// Retrieves all approval requests (past and present) associated with a particular card.
        /// Includes approved, rejected, and pending requests. Useful for compliance audits and understanding card action history.
        /// Shows decision comments and who approved/rejected each request.
        /// </summary>
        /// <param name="cardId">The ID of the card to get approval history for</param>
      /// <returns>Complete approval history for the card</returns>
      [HttpGet("card/{cardId}/history")]
    [ProducesResponseType(typeof(List<ApprovalResponse>), StatusCodes.Status200OK)]
     public async Task<IActionResult> GetApprovalHistory(Guid cardId)
        {
      try
       {
        var approvals = await _approvalService.GetApprovalHistoryAsync(cardId);
          var response = approvals.Select(MapApprovalToResponse).ToList();
    return Ok(response);
    }
  catch (Exception ex)
            {
    _logger.LogError(ex, "Error getting approval history");
      return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
     }
    }

        /// <summary>
        /// Get approval requirements for an action type
        /// 
        /// Shows what role is required to approve a specific action and whether approval is needed at all.
    /// Different actions have different approval requirements (e.g., CEO for card deletion, CFO for freezing).
 /// Use this to inform users what approval level is needed before submitting a request.
        /// Also shows typical maximum duration for approval (usually 48 hours).
        /// </summary>
        /// <param name="actionType">The type of action (FREEZE_CARD, DELETE_CARD, CHANGE_LIMITS, etc)</param>
      /// <returns>Approval requirements and workflow information for the action</returns>
        [HttpGet("requirements/{actionType}")]
        [ProducesResponseType(typeof(ApprovalRequirementsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetApprovalRequirements(string actionType)
        {
            try
         {
      var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        var isRequired = await _approvalService.IsApprovalRequiredAsync(actionType, userRole);

         var requirements = new ApprovalRequirementsResponse
      {
           ActionType = actionType,
       IsRequired = isRequired,
       RequiredRole = GetRequiredRoleForAction(actionType),
    Description = GetActionDescription(actionType),
   MaxDurationHours = 48
        };

       return Ok(requirements);
   }
       catch (Exception ex)
{
_logger.LogError(ex, "Error getting approval requirements");
          return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
   }
   }

        /// <summary>
        /// Maps approval model to response DTO.
        /// </summary>
        private ApprovalResponse MapApprovalToResponse(Models.CardApproval approval)
   {
          return new ApprovalResponse
        {
     Id = approval.Id,
      CardId = approval.CardId,
    ActionType = approval.ActionType,
   Status = approval.Status,
        Reason = approval.Reason,
           ActionData = approval.ActionData,
  CreatedAt = approval.CreatedAt,
           ResolvedAt = approval.ResolvedAt,
     ExpiresAt = approval.ExpiresAt
            };
      }

 /// <summary>
/// Gets required role for an action.
        /// </summary>
        private string GetRequiredRoleForAction(string actionType)
        {
            return actionType switch
  {
                "FREEZE_CARD" => "CFO",
                "DELETE_CARD" => "CEO",
       "CHANGE_LIMITS" => "CFO",
 "CHANGE_MERCHANTS" => "Admin",
      "ENABLE_INTERNATIONAL" => "CFO",
         "CREATE_CARD" => "Admin",
       _ => "Admin"
     };
        }

        /// <summary>
        /// Gets action description.
        /// </summary>
        private string GetActionDescription(string actionType)
        {
            return actionType switch
 {
                "FREEZE_CARD" => "Freeze the virtual card",
          "DELETE_CARD" => "Delete the virtual card",
      "CHANGE_LIMITS" => "Change card spending limits",
       "CHANGE_MERCHANTS" => "Change merchant restrictions",
  "ENABLE_INTERNATIONAL" => "Enable/disable international transactions",
            "CREATE_CARD" => "Create a new card",
                _ => "Perform action"
       };
        }

        /// <summary>
    /// Gets the current user ID from claims.
        /// </summary>
     private Guid? GetUserId()
    {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
     return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
        }
    }
}
