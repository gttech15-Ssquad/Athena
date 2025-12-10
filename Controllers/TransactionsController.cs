using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using virtupay_corporate.DTOs;
using virtupay_corporate.Services;
using System.Security.Claims;

namespace virtupay_corporate.Controllers
{
    /// <summary>
 /// Transaction Management and History
 
 /// Manages virtual card transactions including creation, retrieval, completion, reversal, and dispute handling.
 /// Provides comprehensive transaction tracking, history, and analytics.
 /// All endpoints require JWT authentication. Role-based access control applies to sensitive operations.
 /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
 [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
     private readonly ITransactionService _transactionService;
      private readonly ICardService _cardService;
        private readonly IAccountBalanceService _accountBalanceService;
      private readonly IAuditService _auditService;
  private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
        ITransactionService transactionService,
   ICardService cardService,
            IAccountBalanceService accountBalanceService,
     IAuditService auditService,
            ILogger<TransactionsController> logger)
    {
      _transactionService = transactionService;
  _cardService = cardService;
  _accountBalanceService = accountBalanceService;
 _auditService = auditService;
 _logger = logger;
    }

    /// <summary>
        /// Create a new transaction for a virtual card
  
        /// Initiates a transaction on a specific virtual card. This creates a new transaction record
        /// that can be completed, reversed, or disputed later. Requires authentication only.
        /// 
/// Transaction is typically created in PENDING status and must be completed by authorized users.
   /// The amount is automatically deducted from the user's main account balance.
   /// </summary>
    /// <param name="cardId">The ID of the virtual card to transact on</param>
        /// <param name="request">Transaction details including amount, merchant, and category code</param>
     /// <returns>Created transaction response with transaction ID and status</returns>
        [HttpPost("card/{cardId}")]
 [Authorize]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
   [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTransaction(Guid cardId, [FromBody] CreateTransactionRequest request)
   {
     try
  {
     if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var userId = GetUserId();
  if (!userId.HasValue)
    return Unauthorized();

      // Check if user has sufficient account balance
    var accountBalance = await _accountBalanceService.GetAccountBalanceAsync(userId.Value);
     if (accountBalance == null || accountBalance.AvailableBalance < request.Amount)
   {
            return BadRequest(new ErrorResponse
  {
      Code = "INSUFFICIENT_FUNDS",
Message = $"Insufficient account balance. Required: ?{request.Amount:N2}, Available: ?{accountBalance?.AvailableBalance ?? 0:N2}"
 });
       }

    // Get the card to verify it exists
    var card = await _cardService.GetCardAsync(cardId);
   if (card == null)
    return NotFound(new ErrorResponse { Code = "CARD_NOT_FOUND", Message = "Card not found" });

   var transaction = await _transactionService.CreateTransactionAsync(
cardId,
   request.Amount,
            request.Merchant,
 request.MerchantCategoryCode,
       request.ReferenceId);

 if (transaction == null)
   return BadRequest(new ErrorResponse { Code = "TRANSACTION_FAILED", Message = "Failed to create transaction" });

     // Deduct from main account balance
       var (success, error) = await _accountBalanceService.WithdrawFromAccountAsync(
     userId: userId.Value,
      amount: request.Amount,
   reason: $"Card transaction: {request.Merchant}",
     relatedCardId: cardId,
      relatedCardTransactionId: transaction.Id,
      referenceId: transaction.ReferenceId,
     initiatedBy: userId.Value
         );

if (!success)
 {
     // Rollback transaction if withdrawal failed
       await _transactionService.ReverseTransactionAsync(transaction.Id, "Account withdrawal failed");
     _logger.LogWarning("Failed to deduct from account for transaction {TransactionId}: {Error}", transaction.Id, error);
         return BadRequest(new ErrorResponse
  {
    Code = "INSUFFICIENT_FUNDS",
     Message = error
  });
  }

  var ipAddress = HttpContext?.Connection.RemoteIpAddress?.ToString();
      await _auditService.LogActionAsync(userId, "TRANSACTION_CREATED", "CardTransaction", transaction.Id, 
 $"Amount: ?{request.Amount:N2}, Merchant: {request.Merchant}", ipAddress, null);

       _logger.LogInformation("Transaction created for card {CardId} by user {UserId}, amount deducted from account balance", cardId, userId);
  return CreatedAtAction(nameof(GetTransaction), new { transactionId = transaction.Id }, MapTransactionToResponse(transaction));
          }
       catch (Exception ex)
   {
         _logger.LogError(ex, "Error creating transaction");
     return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
  }
        }

    /// <summary>
 /// Get transaction history for a virtual card
   
     /// Retrieves all transactions for a specific card with pagination support.
        /// Returns transactions in chronological order with status, amount, merchant, and dates.
        /// </summary>
        /// <param name="cardId">The ID of the virtual card</param>
        /// <param name="pageNumber">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
        /// <returns>Paginated list of transactions for the card</returns>
 [HttpGet("card/{cardId}")]
     [ProducesResponseType(typeof(PaginatedResponse<TransactionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCardTransactions(
      Guid cardId,
    [FromQuery] int pageNumber = 1,
   [FromQuery] int pageSize = 20)
     {
     try
            {
    var (transactions, total) = await _transactionService.GetCardTransactionsPaginatedAsync(cardId, pageNumber, pageSize);

         var response = new PaginatedResponse<TransactionResponse>
     {
   Items = transactions.Select(MapTransactionToResponse).ToList(),
        Total = total,
     PageNumber = pageNumber,
        PageSize = pageSize
    };

   return Ok(response);
 }
      catch (Exception ex)
      {
   _logger.LogError(ex, "Error getting transactions");
          return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
 }
   }

        /// <summary>
      /// Get a specific transaction by ID
        /// 
  /// Retrieves detailed information about a single transaction including amount, merchant,
        /// status, dispute information, and timestamps. Useful for reviewing individual transactions.
      /// </summary>
        /// <param name="transactionId">The ID of the transaction to retrieve</param>
        /// <returns>Complete transaction details</returns>
     [HttpGet("{transactionId}")]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransaction(Guid transactionId)
     {
  try
{
  var transaction = await _transactionService.GetTransactionAsync(transactionId);
     if (transaction == null)
   return NotFound(new ErrorResponse { Code = "TRANSACTION_NOT_FOUND", Message = "Transaction not found" });

         return Ok(MapTransactionToResponse(transaction));
}
      catch (Exception ex)
{
        _logger.LogError(ex, "Error getting transaction");
  return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
     }
  }

      /// <summary>
    /// Complete a pending transaction
        /// 
        /// Finalizes a pending transaction, moving it to COMPLETED status. This confirms the transaction
/// has been processed and clears the hold on the card's available balance.
  /// Requires authentication only.
        /// The account balance remains deducted as the transaction is now finalized.
    /// </summary>
        /// <param name="transactionId">The ID of the transaction to complete</param>
        /// <param name="request">Additional completion details (optional)</param>
        /// <returns>Success message if transaction was completed</returns>
        [HttpPost("{transactionId}/complete")]
   [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteTransaction(Guid transactionId, [FromBody] CompleteTransactionRequest request)
    {
   try
  {
  var transaction = await _transactionService.GetTransactionAsync(transactionId);
    if (transaction == null)
 return NotFound(new ErrorResponse { Code = "TRANSACTION_NOT_FOUND", Message = "Transaction not found" });

      var userId = GetUserId();
   if (!userId.HasValue)
  return Unauthorized();

 var card = await _cardService.GetCardAsync(transaction.CardId);
  if (card == null)
        return NotFound(new ErrorResponse { Code = "CARD_NOT_FOUND", Message = "Card not found" });

  var result = await _transactionService.CompleteTransactionAsync(transactionId);
        if (!result)
             return NotFound(new ErrorResponse { Code = "TRANSACTION_NOT_FOUND", Message = "Transaction not found" });

      var ipAddress = HttpContext?.Connection.RemoteIpAddress?.ToString();
      await _auditService.LogActionAsync(userId, "TRANSACTION_COMPLETED", "CardTransaction", transaction.Id,
    $"Amount: ?{transaction.Amount:N2}, Merchant: {transaction.Merchant}", ipAddress, null);

  _logger.LogInformation("Transaction {TransactionId} completed by user {UserId}. Account balance remains deducted.", transactionId, userId);
 return Ok(new { message = "Transaction completed successfully" });
   }
          catch (Exception ex)
   {
    _logger.LogError(ex, "Error completing transaction");
       return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
   }
        }

     /// <summary>
        /// Reverse a transaction
    /// 
      /// Cancels a transaction and reverses its amount back to the card and account. This removes the transaction
        /// from the balance calculation and returns funds to both the card and the main account balance.
        /// Reversals are logged for audit purposes. Requires authentication only.
 /// </summary>
        /// <param name="transactionId">The ID of the transaction to reverse</param>
/// <param name="request">Reversal reason (required for audit trail)</param>
  /// <returns>Success message if transaction was reversed</returns>
      [HttpPost("{transactionId}/reverse")]
        [Authorize]
      [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> ReverseTransaction(Guid transactionId, [FromBody] ReverseTransactionRequest request)
   {
          try
     {
     var transaction = await _transactionService.GetTransactionAsync(transactionId);
    if (transaction == null)
   return NotFound(new ErrorResponse { Code = "TRANSACTION_NOT_FOUND", Message = "Transaction not found" });

    var userId = GetUserId();
 if (!userId.HasValue)
            return Unauthorized();

      var card = await _cardService.GetCardAsync(transaction.CardId);
 if (card == null)
    return NotFound(new ErrorResponse { Code = "CARD_NOT_FOUND", Message = "Card not found" });

       var result = await _transactionService.ReverseTransactionAsync(transactionId, request.Reason);
if (!result)
         return NotFound(new ErrorResponse { Code = "TRANSACTION_NOT_FOUND", Message = "Transaction not found" });

       var (success, error) = await _accountBalanceService.WithdrawFromAccountAsync(
           card.UserId,
      -transaction.Amount,
    $"Transaction reversal: {request.Reason}",
     transaction.CardId,
 transaction.Id,
    $"REVERSE-{transaction.ReferenceId}",
       userId.Value
    );

        if (!success)
 {
        _logger.LogWarning("Failed to refund amount to account for reversed transaction {TransactionId}: {Error}", transactionId, error);
     }

    var ipAddress = HttpContext?.Connection.RemoteIpAddress?.ToString();
        await _auditService.LogActionAsync(userId, "TRANSACTION_REVERSED", "CardTransaction", transaction.Id,
  $"Amount: ?{transaction.Amount:N2}, Reason: {request.Reason}", ipAddress, null);

   _logger.LogInformation("Transaction {TransactionId} reversed by user {UserId}. Amount refunded to account balance.", transactionId, userId);
  return Ok(new { message = "Transaction reversed successfully" });
   }
         catch (Exception ex)
         {
 _logger.LogError(ex, "Error reversing transaction");
return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
      }
   }

        /// <summary>
     /// Dispute a transaction
     /// 
    /// Files a dispute against a transaction (e.g., fraudulent activity, merchant error, or unauthorized use).
   /// Puts the transaction in DISPUTED status pending investigation. Requires authentication only.
    /// The dispute reason is recorded for investigation records.
 /// </summary>
   /// <param name="transactionId">The ID of the transaction to dispute</param>
        /// <param name="request">Dispute reason explaining why the transaction is being disputed</param>
      /// <returns>Success message if dispute was filed</returns>
      [HttpPost("{transactionId}/dispute")]
 [Authorize]
     [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DisputeTransaction(Guid transactionId, [FromBody] DisputeTransactionRequest request)
        {
    try
  {
        var result = await _transactionService.DisputeTransactionAsync(transactionId, request.Reason);
              if (!result)
     return NotFound(new ErrorResponse { Code = "TRANSACTION_NOT_FOUND", Message = "Transaction not found" });

       var userId = GetUserId();
     if (userId.HasValue)
   {
        var ipAddress = HttpContext?.Connection.RemoteIpAddress?.ToString();
     await _auditService.LogActionAsync(userId, "TRANSACTION_DISPUTED", "CardTransaction", transactionId,
    $"Dispute Reason: {request.Reason}", ipAddress, null);
  }

    _logger.LogInformation("Transaction {TransactionId} disputed", transactionId);
      return Ok(new { message = "Transaction disputed successfully" });
      }
            catch (Exception ex)
    {
    _logger.LogError(ex, "Error disputing transaction");
      return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
     }
    }

        /// <summary>
        /// Get transaction summary and analytics for a card
     /// 
    /// Provides a high-level overview of transactions for a card within a date range.
  /// Includes total amounts by status (completed, pending, reversed, failed) and breakdown by merchant.
     /// Useful for financial reporting and spending analysis. Defaults to last 30 days if dates not specified.
 /// </summary>
    /// <param name="cardId">The ID of the virtual card</param>
        /// <param name="startDate">Start date for summary (optional, defaults to 30 days ago)</param>
 /// <param name="endDate">End date for summary (optional, defaults to today)</param>
   /// <returns>Transaction summary with totals and merchant breakdown</returns>
      [HttpGet("card/{cardId}/summary")]
        [ProducesResponseType(typeof(TransactionSummaryResponse), StatusCodes.Status200OK)]
   public async Task<IActionResult> GetTransactionSummary(
Guid cardId,
    [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null)
    {
      try
{
  var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
    var end = endDate ?? DateTime.UtcNow;

  var transactions = await _transactionService.GetTransactionsByDateRangeAsync(cardId, start, end);

 var summary = new TransactionSummaryResponse
          {
      Currency = "NGN",
     CompletedAmount = transactions.Where(t => t.Status == "COMPLETED").Sum(t => t.Amount),
    PendingAmount = transactions.Where(t => t.Status == "PENDING").Sum(t => t.Amount),
    ReversedAmount = transactions.Where(t => t.Status == "REVERSED").Sum(t => t.Amount),
     FailedAmount = transactions.Where(t => t.Status == "FAILED").Sum(t => t.Amount),
   TransactionsByMerchant = transactions
     .GroupBy(t => t.Merchant)
      .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount))
 };

  return Ok(summary);
       }
    catch (Exception ex)
      {
  _logger.LogError(ex, "Error getting transaction summary");
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

 /// <summary>
        /// Maps transaction model to response DTO.
        /// </summary>
        private TransactionResponse MapTransactionToResponse(Models.CardTransaction transaction)
        {
        return new TransactionResponse
            {
          Id = transaction.Id,
    CardId = transaction.CardId,
         Amount = transaction.Amount,
         Merchant = transaction.Merchant,
    MerchantCategoryCode = transaction.MerchantCategoryCode,
Status = transaction.Status,
    Currency = transaction.Currency,
    ReferenceId = transaction.ReferenceId,
 DisputeReason = transaction.DisputeReason,
     CanBeDisputed = transaction.CanBeDisputed,
       CreatedAt = transaction.CreatedAt,
    CompletedAt = transaction.CompletedAt
    };
 }
    }
}
