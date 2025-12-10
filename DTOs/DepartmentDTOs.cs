using System.ComponentModel.DataAnnotations;

namespace virtupay_corporate.DTOs
{
    /// <summary>
    /// DTO for creating a department.
    /// </summary>
   public class CreateDepartmentRequest
    {
      /// <summary>
        /// Gets or sets the department name.
        /// </summary>
  [Required(ErrorMessage = "Department name is required")]
   [MaxLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
    public required string Name { get; set; }

        /// <summary>
  /// Gets or sets the department budget.
        /// </summary>
        [Required(ErrorMessage = "Budget is required")]
  [Range(0, 999999999)]
 public decimal Budget { get; set; }

        /// <summary>
/// Gets or sets the manager ID.
      /// </summary>
        public Guid? ManagerId { get; set; }
    }

    /// <summary>
    /// DTO for updating a department.
  /// </summary>
   public class UpdateDepartmentRequest
    {
        /// <summary>
 /// Gets or sets the department name.
      /// </summary>
        [MaxLength(255)]
  public string? Name { get; set; }

     /// <summary>
        /// Gets or sets the department budget.
        /// </summary>
      [Range(0, 999999999)]
    public decimal? Budget { get; set; }

  /// <summary>
     /// Gets or sets the manager ID.
  /// </summary>
        public Guid? ManagerId { get; set; }

        /// <summary>
 /// Gets or sets the department status.
        /// </summary>
 [RegularExpression("^(Active|Inactive)$")]
      public string? Status { get; set; }
    }

  /// <summary>
    /// DTO for department response.
    /// </summary>
    public class DepartmentResponse
    {
/// <summary>
   /// Gets or sets the department ID.
 /// </summary>
        public string Id { get; set; } = "";

   /// <summary>
    /// Gets or sets the department name.
      /// </summary>
  public string? Name { get; set; }

 /// <summary>
     /// Gets or sets the department budget.
   /// </summary>
     public decimal Budget { get; set; }

 /// <summary>
    /// Gets or sets the manager ID.
 /// </summary>
   public string? ManagerId { get; set; }

  /// <summary>
        /// Gets or sets the manager name.
  /// </summary>
     public string? ManagerName { get; set; }

        /// <summary>
  /// Gets or sets the department status.
     /// </summary>
  public string? Status { get; set; }

   /// <summary>
 /// Gets or sets the total users in department.
   /// </summary>
   public int UserCount { get; set; }

   /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

      /// <summary>
        /// Gets or sets the last update timestamp.
    /// </summary>
 public DateTime UpdatedAt { get; set; }
    }

  /// <summary>
    /// DTO for department summary.
    /// </summary>
    public class DepartmentSummaryResponse
    {
        /// <summary>
        /// Gets or sets the department information.
        /// </summary>
 public DepartmentResponse? Department { get; set; }

        /// <summary>
  /// Gets or sets the users in department.
        /// </summary>
       public List<UserResponse> Users { get; set; } = new List<UserResponse>();

  /// <summary>
        /// Gets or sets the total spent.
     /// </summary>
    public decimal TotalSpent { get; set; }

      /// <summary>
   /// Gets or sets the remaining budget.
       /// </summary>
   public decimal RemainingBudget => Department?.Budget - TotalSpent ?? 0;

   /// <summary>
        /// Gets or sets the budget utilization percentage.
        /// </summary>
        public decimal BudgetUtilization => Department?.Budget > 0 ? (TotalSpent / Department.Budget) * 100 : 0;
 }

    /// <summary>
/// DTO for audit log response.
  /// </summary>
    public class AuditLogResponse
    {
        /// <summary>
        /// Gets or sets the audit log ID.
 /// </summary>
  public string Id { get; set; } = "";

     /// <summary>
 /// Gets or sets the user ID.
/// </summary>
  public string? UserId { get; set; }

        /// <summary>
  /// Gets or sets the user email.
   /// </summary>
       public string? UserEmail { get; set; }

    /// <summary>
   /// Gets or sets the action performed.
 /// </summary>
   public string? Action { get; set; }

   /// <summary>
   /// Gets or sets the resource type.
 /// </summary>
 public string? Resource { get; set; }

    /// <summary>
     /// Gets or sets the resource ID.
     /// </summary>
     public string? ResourceId { get; set; }

   /// <summary>
        /// Gets or sets the changes made.
/// </summary>
   public string? Changes { get; set; }

   /// <summary>
     /// Gets or sets the IP address.
     /// </summary>
   public string? IpAddress { get; set; }

      /// <summary>
        /// Gets or sets the status.
   /// </summary>
  public string? Status { get; set; }

  /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

    /// <summary>
   /// Gets or sets any error message.
 /// </summary>
   public string? ErrorMessage { get; set; }
    }

    /// <summary>
 /// DTO for audit log filter.
    /// </summary>
    public class AuditLogFilterRequest
    {
        /// <summary>
   /// Gets or sets the action to filter by.
        /// </summary>
  [MaxLength(255)]
   public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the resource to filter by.
      /// </summary>
  [MaxLength(255)]
  public string? Resource { get; set; }

  /// <summary>
        /// Gets or sets the start date.
 /// </summary>
  public DateTime? StartDate { get; set; }

   /// <summary>
        /// Gets or sets the end date.
  /// </summary>
   public DateTime? EndDate { get; set; }

      /// <summary>
    /// Gets or sets the user ID.
   /// </summary>
        public Guid? UserId { get; set; }

   /// <summary>
 /// Gets or sets the page number.
  /// </summary>
    [Range(1, int.MaxValue)]
   public int PageNumber { get; set; } = 1;

      /// <summary>
        /// Gets or sets the page size.
    /// </summary>
[Range(1, 100)]
       public int PageSize { get; set; } = 20;
}

    /// <summary>
  /// DTO for compliance report.
    /// </summary>
    public class ComplianceReportResponse
    {
        /// <summary>
        /// Gets or sets the report period start.
   /// </summary>
 public DateTime PeriodStart { get; set; }

 /// <summary>
   /// Gets or sets the report period end.
    /// </summary>
 public DateTime PeriodEnd { get; set; }

   /// <summary>
 /// Gets or sets the total users.
        /// </summary>
   public int TotalUsers { get; set; }

  /// <summary>
        /// Gets or sets the total cards.
     /// </summary>
 public int TotalCards { get; set; }

       /// <summary>
     /// Gets or sets the total transactions.
  /// </summary>
    public int TotalTransactions { get; set; }

  /// <summary>
      /// Gets or sets the total transaction value.
      /// </summary>
   public decimal TotalTransactionValue { get; set; }

  /// <summary>
    /// Gets or sets the active approvals.
    /// </summary>
 public int ActiveApprovals { get; set; }

    /// <summary>
    /// Gets or sets high-risk transactions count.
        /// </summary>
 public int HighRiskTransactions { get; set; }

  /// <summary>
        /// Gets or sets the summary by department.
  /// </summary>
     public Dictionary<string, decimal> SummaryByDepartment { get; set; } = new Dictionary<string, decimal>();
    }
}
