using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using virtupay_corporate.DTOs;
using virtupay_corporate.Services;
using System.Security.Claims;

namespace virtupay_corporate.Controllers
{
    /// <summary>
    /// Account Balance Management
    /// 
    /// Manages main user account balances and funding operations.
    /// This is the master account from which virtual cards are funded and money can be deducted.
    /// Provides APIs for checking balance, funding the account, and viewing transaction history.
    /// All endpoints require JWT authentication. Some operations require specific roles (CEO, CFO, Admin).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class AccountBalanceController : ControllerBase
    {
        private readonly IAccountBalanceService _accountBalanceService;
    private readonly IAuditService _auditService;
   private readonly ILogger<AccountBalanceController> _logger;

 public AccountBalanceController(
          IAccountBalanceService accountBalanceService,
       IAuditService auditService,
            ILogger<AccountBalanceController> logger)
        {
            _accountBalanceService = accountBalanceService;
  _auditService = auditService;
            _logger = logger;
    }

        /// <summary>
        /// Get the current account balance
        /// 
        /// Retrieves the main account balance for the authenticated user including available funds,
        /// total funded amount, and total withdrawn amount. Returns the complete balance snapshot.
        /// Useful for dashboard and balance monitoring.
        /// </summary>
     /// <returns>Current account balance details</returns>
        [HttpGet]
        [ProducesResponseType(typeof(AccountBalanceResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccountBalance()
 {
         try
       {
    var userId = GetUserId();
        if (!userId.HasValue)
      return Unauthorized();

      var balance = await _accountBalanceService.GetAccountBalanceAsync(userId.Value);
        if (balance == null)
       return NotFound(new ErrorResponse { Code = "ACCOUNT_NOT_FOUND", Message = "Account balance not found" });

      return Ok(new AccountBalanceResponse
       {
         Id = balance.Id,
UserId = balance.UserId ?? Guid.Empty,
  AvailableBalance = balance.AvailableBalance,
 TotalFunded = balance.TotalFunded,
                    TotalWithdrawn = balance.TotalWithdrawn,
   Currency = balance.Currency,
         CreatedAt = balance.CreatedAt,
                    UpdatedAt = balance.UpdatedAt
       });
            }
   catch (Exception ex)
            {
      _logger.LogError(ex, "Error getting account balance");
   return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
     }
     }

        /// <summary>
        /// Get account balance summary for dashboard
        /// 
        /// Retrieves a quick summary of the account including available balance, total funded,
     /// total withdrawn, number of cards funded, and active transactions. Perfect for dashboard widgets.
        /// </summary>
        /// <returns>Account balance summary with key metrics</returns>
     [HttpGet("summary")]
        [ProducesResponseType(typeof(AccountBalanceSummaryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccountSummary()
  {
  try
     {
   var userId = GetUserId();
        if (!userId.HasValue)
        return Unauthorized();

    var (available, totalFunded, totalWithdrawn, cardsFunded, activeTransactions) 
       = await _accountBalanceService.GetAccountSummaryAsync(userId.Value);

     return Ok(new AccountBalanceSummaryResponse
      {
          AvailableBalance = available,
       TotalFunded = totalFunded,
        TotalWithdrawn = totalWithdrawn,
   Currency = "NGN",
     LastUpdated = DateTime.UtcNow,
   TotalCardsFunded = cardsFunded,
          ActiveTransactions = activeTransactions
      });
 }
            catch (Exception ex)
       {
       _logger.LogError(ex, "Error getting account summary");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
      }
        }

        /// <summary>
        /// Fund the main account balance
        /// 
        /// Adds funds to the main account balance. This is how money enters the system and becomes
      /// available for allocation to virtual cards. Requires authentication only.
     /// All funding transactions are logged and audited.
        /// </summary>
/// <param name="request">Funding details including amount, reason, and optional reference ID</param>
     /// <returns>Updated account balance after funding</returns>
        [HttpPost("fund")]
     [Authorize]
        [ProducesResponseType(typeof(AccountBalanceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
   public async Task<IActionResult> FundAccount([FromBody] FundAccountBalanceRequest request)
  {
            try
 {
           if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var userId = GetUserId();
     if (!userId.HasValue)
           return Unauthorized();

    var balance = await _accountBalanceService.FundAccountAsync(
          userId.Value,
       request.Amount,
        request.Reason,
        request.ReferenceId,
       userId.Value);

                if (balance == null)
      return BadRequest(new ErrorResponse { Code = "FUNDING_FAILED", Message = "Failed to fund account" });

  var ipAddress = HttpContext?.Connection.RemoteIpAddress?.ToString();
   await _auditService.LogActionAsync(
        userId,
         "ACCOUNT_FUNDED",
     "AccountBalance",
        balance.Id,
     $"Amount: {request.Amount}, Reason: {request.Reason}",
         ipAddress,
      null);

                _logger.LogInformation("Account {UserId} funded with ${Amount}", userId, request.Amount);

     return Ok(new AccountBalanceResponse
      {
        Id = balance.Id,
         UserId = balance.UserId ?? Guid.Empty,
      AvailableBalance = balance.AvailableBalance,
        TotalFunded = balance.TotalFunded,
 TotalWithdrawn = balance.TotalWithdrawn,
      Currency = balance.Currency,
      CreatedAt = balance.CreatedAt,
   UpdatedAt = balance.UpdatedAt
       });
      }
 catch (Exception ex)
     {
     _logger.LogError(ex, "Error funding account");
  return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred while funding account" });
       }
        }

        /// <summary>
        /// Get account transaction history
        /// 
  /// Retrieves paginated transaction history for the account including all funding and withdrawal transactions.
        /// Can filter by transaction type (FUNDING, WITHDRAWAL). Shows balance before/after each transaction.
        /// Useful for compliance, auditing, and understanding account activity.
        /// </summary>
        /// <param name="pageNumber">Page number (starts at 1)</param>
        /// <param name="pageSize">Number of items per page (1-100)</param>
        /// <param name="transactionType">Optional filter: FUNDING or WITHDRAWAL</param>
        /// <returns>Paginated list of account transactions</returns>
        [HttpGet("transactions")]
        [ProducesResponseType(typeof(List<AccountTransactionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactionHistory(int pageNumber = 1, int pageSize = 20, string? transactionType = null)
        {
            try
  {
         if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
    return BadRequest(new ErrorResponse { Code = "INVALID_PARAMS", Message = "Invalid pagination parameters" });

        var userId = GetUserId();
      if (!userId.HasValue)
      return Unauthorized();

              var (transactions, total) = await _accountBalanceService.GetAccountTransactionsPaginatedAsync(
  userId.Value,
         pageNumber,
 pageSize,
      transactionType);

       var response = transactions.Select(t => new AccountTransactionResponse
 {
              Id = t.Id,
       AccountBalanceId = t.AccountBalanceId,
 TransactionType = t.TransactionType,
         Amount = t.Amount,
        BalanceBefore = t.BalanceBefore,
    BalanceAfter = t.BalanceAfter,
    Description = t.Description,
        ReferenceId = t.ReferenceId,
         InitiatedBy = t.InitiatedBy,
                Currency = t.Currency,
         Status = t.Status,
         CreatedAt = t.CreatedAt,
  CompletedAt = t.CompletedAt,
    RelatedCardId = t.RelatedCardId,
     RelatedCardTransactionId = t.RelatedCardTransactionId
   }).ToList();

        HttpContext.Response.Headers.Add("X-Total-Count", total.ToString());
     HttpContext.Response.Headers.Add("X-Page-Number", pageNumber.ToString());
                HttpContext.Response.Headers.Add("X-Page-Size", pageSize.ToString());

        return Ok(response);
   }
            catch (Exception ex)
            {
     _logger.LogError(ex, "Error getting transaction history");
          return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
            }
        }

        /// <summary>
    /// Get transaction history for a date range
  /// 
        /// Retrieves all account transactions within a specified date range.
        /// Useful for reporting, compliance, and period-based analysis.
 /// </summary>
        /// <param name="startDate">Start date (ISO format: yyyy-MM-dd)</param>
        /// <param name="endDate">End date (ISO format: yyyy-MM-dd)</param>
        /// <returns>List of transactions within the date range</returns>
        [HttpGet("transactions/date-range")]
        [ProducesResponseType(typeof(List<AccountTransactionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactionsByDateRange(DateTime startDate, DateTime endDate)
        {
    try
      {
     if (startDate > endDate)
               return BadRequest(new ErrorResponse { Code = "INVALID_DATE_RANGE", Message = "Start date must be before end date" });

              var userId = GetUserId();
 if (!userId.HasValue)
 return Unauthorized();

    var transactions = await _accountBalanceService.GetTransactionsByDateRangeAsync(userId.Value, startDate, endDate);

       var response = transactions.Select(t => new AccountTransactionResponse
     {
   Id = t.Id,
               AccountBalanceId = t.AccountBalanceId,
        TransactionType = t.TransactionType,
     Amount = t.Amount,
    BalanceBefore = t.BalanceBefore,
                BalanceAfter = t.BalanceAfter,
  Description = t.Description,
       ReferenceId = t.ReferenceId,
         InitiatedBy = t.InitiatedBy,
           Currency = t.Currency,
           Status = t.Status,
         CreatedAt = t.CreatedAt,
                CompletedAt = t.CompletedAt,
          RelatedCardId = t.RelatedCardId,
       RelatedCardTransactionId = t.RelatedCardTransactionId
       }).ToList();

return Ok(response);
            }
      catch (Exception ex)
          {
          _logger.LogError(ex, "Error getting transactions by date range");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
    }
     }

        /// <summary>
  /// Gets the current user ID from JWT claims.
        /// </summary>
        private Guid? GetUserId()
   {
     var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }
    }
}
