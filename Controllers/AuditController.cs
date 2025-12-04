using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using virtupay_corporate.DTOs;
using virtupay_corporate.Services;
using virtupay_corporate.Repositories;

namespace virtupay_corporate.Controllers
{
    /// <summary>
    /// Audit Trail and Compliance Reporting
    /// 
    /// Manages audit logs and compliance records for all system operations. Provides comprehensive audit trail 
    /// for regulatory requirements and security investigations. Supports filtering, searching, and exporting audit data.
    /// All endpoints require authentication. Access restricted to CEO, CFO, Admin, and Auditor roles.
    /// </summary>
  [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "CEO,CFO,Admin,Auditor")]
    [Produces("application/json")]
    public class AuditController : ControllerBase
    {
  private readonly IAuditService _auditService;
     private readonly ILogger<AuditController> _logger;

   public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
            _auditService = auditService;
      _logger = logger;
     }

   /// <summary>
        /// Get all audit logs with filtering and pagination
        /// 
    /// Retrieves audit logs for all system operations with support for filtering by action, resource, 
        /// and date range. Useful for compliance audits and security investigations. Returns paginated results.
        /// </summary>
        /// <param name="pageNumber">Page number for pagination (default: 1)</param>
        /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
        /// <param name="action">Filter by action type (e.g., CREATE, UPDATE, DELETE)</param>
        /// <param name="resource">Filter by resource type (e.g., CARD, USER, TRANSACTION)</param>
        /// <param name="startDate">Filter logs from this date onwards (optional)</param>
        /// <param name="endDate">Filter logs up to this date (optional)</param>
        /// <returns>Paginated list of audit logs matching filters</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
  [FromQuery] int pageNumber = 1,
  [FromQuery] int pageSize = 20,
       [FromQuery] string? action = null,
    [FromQuery] string? resource = null,
  [FromQuery] DateTime? startDate = null,
     [FromQuery] DateTime? endDate = null)
 {
      try
 {
  var (logs, total) = await _auditService.GetAuditLogsAsync(pageNumber, pageSize, action, resource, startDate, endDate);

 var response = new PaginatedResponse<AuditLogResponse>
    {
Items = logs.Select(MapAuditLogToResponse).ToList(),
 Total = total,
      PageNumber = pageNumber,
      PageSize = pageSize
     };

  return Ok(response);
    }
  catch (Exception ex)
 {
_logger.LogError(ex, "Error getting audit logs");
    return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
        }
        }

 /// <summary>
        /// Get audit logs for a specific user
        /// 
      /// Retrieves all audit log entries related to actions performed by or affecting a specific user.
     /// Useful for user activity tracking and compliance verification.
    /// </summary>
        /// <param name="userId">The ID of the user to get audit logs for</param>
        /// <returns>List of audit logs for the specified user</returns>
        [HttpGet("user/{userId}")]
 [ProducesResponseType(typeof(List<AuditLogResponse>), StatusCodes.Status200OK)]
 public async Task<IActionResult> GetUserAuditLogs(int userId)
        {
 try
            {
  var logs = await _auditService.GetUserAuditLogsAsync(userId);
     var response = logs.Select(MapAuditLogToResponse).ToList();
          return Ok(response);
    }
    catch (Exception ex)
  {
      _logger.LogError(ex, "Error getting user audit logs");
  return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
       }
 }

     /// <summary>
        /// Export audit logs in specified format
        /// 
        /// Exports filtered audit logs to CSV or JSON format for external processing, reporting, or archival.
        /// Defaults to last 30 days if date range not specified.
        /// </summary>
        /// <param name="format">Export format: 'csv' or 'json' (default: 'csv')</param>
        /// <param name="startDate">Export logs from this date onwards (optional)</param>
     /// <param name="endDate">Export logs up to this date (optional)</param>
        /// <returns>File download in requested format</returns>
   [HttpGet("export")]
      [ProducesResponseType(StatusCodes.Status200OK)]
   public async Task<IActionResult> ExportAuditLogs(
      [FromQuery] string format = "csv",
        [FromQuery] DateTime? startDate = null,
       [FromQuery] DateTime? endDate = null)
        {
     try
   {
      var exportData = await _auditService.ExportAuditLogsAsync(format, startDate, endDate);

      var fileName = $"audit_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}";
     var contentType = format.ToLower() == "csv" ? "text/csv" : "application/json";

         return File(System.Text.Encoding.UTF8.GetBytes(exportData), contentType, fileName);
     }
  catch (Exception ex)
 {
    _logger.LogError(ex, "Error exporting audit logs");
      return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
    }
    }

 /// <summary>
      /// Maps audit log model to response DTO.
  /// </summary>
 private AuditLogResponse MapAuditLogToResponse(Models.AuditLog log)
        {
     return new AuditLogResponse
      {
   Id = log.Id,
  UserId = log.UserId,
   Action = log.Action,
           Resource = log.Resource,
  ResourceId = log.ResourceId,
  Changes = log.Changes,
    IpAddress = log.IpAddress,
     Status = log.Status,
   Timestamp = log.Timestamp,
         ErrorMessage = log.ErrorMessage
       };
     }
    }

    /// <summary>
  /// Department Management and Organization Structure
    /// 
    /// Manages corporate departments including creation, updates, budget allocation, and manager assignments.
    /// Provides organizational hierarchy and department-level resource management.
    /// Requires JWT authentication. Most operations restricted to CEO role.
    /// </summary>
  [ApiController]
    [Route("api/[controller]")]
   [Authorize]
    [Produces("application/json")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepository;
   private readonly IAuditService _auditService;
        private readonly ILogger<DepartmentsController> _logger;

   public DepartmentsController(
 IDepartmentRepository departmentRepository,
       IAuditService auditService,
  ILogger<DepartmentsController> logger)
        {
           _departmentRepository = departmentRepository;
 _auditService = auditService;
        _logger = logger;
        }

  /// <summary>
        /// Create a new department
        /// 
     /// Establishes a new corporate department with assigned name, budget allocation, and manager.
        /// Only CEO can create departments. Returns department details upon successful creation.
        /// </summary>
        /// <param name="request">Department details including name, budget, and manager ID</param>
        /// <returns>Created department information</returns>
       [HttpPost]
 [Authorize(Roles = "CEO")]
      [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
   try
   {
 if (!ModelState.IsValid)
      return BadRequest(ModelState);

  var department = new Models.Department
    {
Name = request.Name,
       Budget = request.Budget,
  ManagerId = request.ManagerId,
      Status = "Active"
   };

       var created = await _departmentRepository.CreateAsync(department);

    _logger.LogInformation("Department {DepartmentId} created", created.Id);
 return CreatedAtAction(nameof(GetDepartment), new { departmentId = created.Id }, MapDepartmentToResponse(created));
   }
      catch (Exception ex)
   {
        _logger.LogError(ex, "Error creating department");
     return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
    }
        }

    /// <summary>
 /// Get all departments with pagination
 /// 
        /// Retrieves paginated list of all corporate departments with their details and user counts.
        /// Useful for organizational overview and department-level reporting.
  /// </summary>
        /// <param name="pageNumber">Page number for pagination (default: 1)</param>
   /// <param name="pageSize">Number of departments per page (default: 20)</param>
        /// <returns>Paginated list of departments</returns>
     [HttpGet]
       [ProducesResponseType(typeof(PaginatedResponse<DepartmentResponse>), StatusCodes.Status200OK)]
     public async Task<IActionResult> GetDepartments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
     {
 try
  {
    var (departments, total) = await _departmentRepository.GetPaginatedAsync(pageNumber, pageSize);

      var response = new PaginatedResponse<DepartmentResponse>
    {
    Items = departments.Select(MapDepartmentToResponse).ToList(),
   Total = total,
           PageNumber = pageNumber,
    PageSize = pageSize
  };

    return Ok(response);
   }
     catch (Exception ex)
   {
    _logger.LogError(ex, "Error getting departments");
 return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
    }
     }

  /// <summary>
        /// Get a specific department by ID
  /// 
        /// Retrieves detailed information for a single department including budget, manager, and user count.
 /// </summary>
        /// <param name="departmentId">The ID of the department to retrieve</param>
        /// <returns>Department details</returns>
      [HttpGet("{departmentId}")]
   [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
     public async Task<IActionResult> GetDepartment(int departmentId)
       {
     try
     {
       var department = await _departmentRepository.GetByIdAsync(departmentId);
      if (department == null)
         return NotFound(new ErrorResponse { Code = "DEPARTMENT_NOT_FOUND", Message = "Department not found" });

     return Ok(MapDepartmentToResponse(department));
   }
       catch (Exception ex)
 {
     _logger.LogError(ex, "Error getting department");
       return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
   }
        }

  /// <summary>
   /// Update a department
        /// 
        /// Modifies department information such as name, budget allocation, manager assignment, and status.
        /// Only CEO can update departments. Partial updates are supported.
        /// </summary>
        /// <param name="departmentId">The ID of the department to update</param>
        /// <param name="request">Updated department details (only non-null values are updated)</param>
        /// <returns>Updated department information</returns>
        [HttpPut("{departmentId}")]
     [Authorize(Roles = "CEO")]
 [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDepartment(int departmentId, [FromBody] UpdateDepartmentRequest request)
       {
     try
    {
          var department = await _departmentRepository.GetByIdAsync(departmentId);
          if (department == null)
         return NotFound(new ErrorResponse { Code = "DEPARTMENT_NOT_FOUND", Message = "Department not found" });

  if (!string.IsNullOrEmpty(request.Name))
     department.Name = request.Name;

   if (request.Budget.HasValue)
   department.Budget = request.Budget.Value;

       if (request.ManagerId.HasValue)
    department.ManagerId = request.ManagerId;

      if (!string.IsNullOrEmpty(request.Status))
    department.Status = request.Status;

         var updated = await _departmentRepository.UpdateAsync(department);

    _logger.LogInformation("Department {DepartmentId} updated", departmentId);
    return Ok(MapDepartmentToResponse(updated));
        }
catch (Exception ex)
    {
 _logger.LogError(ex, "Error updating department");
  return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
           }
      }

   /// <summary>
    /// Delete a department
    /// 
        /// Permanently removes a department from the system. Only CEO can delete departments.
        /// This action is irreversible and may affect associated users and cards.
        /// </summary>
        /// <param name="departmentId">The ID of the department to delete</param>
        /// <returns>No content if deletion successful</returns>
    [HttpDelete("{departmentId}")]
      [Authorize(Roles = "CEO")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDepartment(int departmentId)
   {
     try
{
 var result = await _departmentRepository.DeleteAsync(departmentId);
 if (!result)
        return NotFound(new ErrorResponse { Code = "DEPARTMENT_NOT_FOUND", Message = "Department not found" });

    _logger.LogInformation("Department {DepartmentId} deleted", departmentId);
          return NoContent();
   }
  catch (Exception ex)
       {
 _logger.LogError(ex, "Error deleting department");
return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
        }
  }

  /// <summary>
        /// Maps department model to response DTO.
      /// </summary>
     private DepartmentResponse MapDepartmentToResponse(Models.Department department)
        {
  return new DepartmentResponse
        {
     Id = department.Id,
      Name = department.Name,
        Budget = department.Budget,
    ManagerId = department.ManagerId,
       Status = department.Status,
        UserCount = department.Users?.Count ?? 0,
           CreatedAt = department.CreatedAt,
    UpdatedAt = department.UpdatedAt
    };
        }
  }
}
