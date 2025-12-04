using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using virtupay_corporate.DTOs;
using virtupay_corporate.Services;
using System.Security.Claims;

namespace virtupay_corporate.Controllers
{
  /// <summary>
    /// Virtual Card Management
    /// 
    /// Comprehensive virtual card lifecycle management including creation, retrieval, status updates, 
  /// spending limit configuration, balance tracking, and international transaction controls.
    /// Supports card freezing, unfreezing, and cancellation with full audit trail.
  /// Requires JWT authentication. Most operations require CEO, CFO, or Admin roles.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class CardsController : ControllerBase
    {
  private readonly ICardService _cardService;
    private readonly ICardLimitService _cardLimitService;
        private readonly IBalanceService _balanceService;
        private readonly IAuditService _auditService;
  private readonly ILogger<CardsController> _logger;

        public CardsController(
         ICardService cardService,
            ICardLimitService cardLimitService,
  IBalanceService balanceService,
      IAuditService auditService,
    ILogger<CardsController> logger)
     {
_cardService = cardService;
     _cardLimitService = cardLimitService;
       _balanceService = balanceService;
            _auditService = auditService;
            _logger = logger;
      }

        /// <summary>
  /// Create a new virtual card
        /// 
        /// Generates a new virtual card for the authenticated user with assigned cardholder name and optional nickname.
     /// Returns complete card details including masked card number, expiry date, and initial status.
        /// Requires CEO, CFO, or Admin role.
        /// </summary>
        [HttpPost]
      [Authorize(Roles = "CEO,CFO,Admin")]
        [ProducesResponseType(typeof(CardResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
     public async Task<IActionResult> CreateCard([FromBody] CreateCardRequest request)
        {
     try
      {
    if (!ModelState.IsValid)
   return BadRequest(ModelState);

            var userId = GetUserId();
    if (!userId.HasValue)
          return Unauthorized();

     var card = await _cardService.CreateCardAsync(userId.Value, request.CardholderName, request.Nickname, request.DepartmentId);

         if (card == null)
        return BadRequest(new ErrorResponse { Code = "CARD_CREATION_FAILED", Message = "Failed to create card" });

            _logger.LogInformation("Card created for user {UserId}", userId);

       return CreatedAtAction(nameof(GetCard), new { cardId = card.Id }, MapCardToResponse(card));
            }
     catch (Exception ex)
        {
          _logger.LogError(ex, "Error creating card");
      return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred while creating the card" });
   }
 }

        /// <summary>
     /// Get all user virtual cards
        /// 
        /// Retrieves paginated list of all virtual cards associated with the authenticated user.
        /// Returns card summaries with masked card numbers and status information.
        /// </summary>
    [HttpGet]
     [ProducesResponseType(typeof(PaginatedResponse<CardResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCards([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
     {
    var userId = GetUserId();
  if (!userId.HasValue)
      return Unauthorized();

    var (cards, total) = await _cardService.GetUserCardsPaginatedAsync(userId.Value, pageNumber, pageSize);

          var response = new PaginatedResponse<CardResponse>
       {
      Items = cards.Select(MapCardToResponse).ToList(),
            Total = total,
PageNumber = pageNumber,
       PageSize = pageSize
     };

         return Ok(response);
        }
  catch (Exception ex)
     {
      _logger.LogError(ex, "Error getting cards");
  return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
            }
    }

        /// <summary>
    /// Get card details by ID
        /// 
     /// Retrieves complete information for a specific card including masked card number, 
        /// status, metadata, creation date, and update history.
        /// </summary>
   [HttpGet("{cardId}")]
        [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCard(int cardId)
        {
            try
       {
          var card = await _cardService.GetCardAsync(cardId);
      if (card == null)
    return NotFound(new ErrorResponse { Code = "CARD_NOT_FOUND", Message = "Card not found" });

            return Ok(MapCardToResponse(card));
  }
            catch (Exception ex)
          {
                _logger.LogError(ex, "Error getting card {CardId}", cardId);
          return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
            }
        }

 /// <summary>
        /// Get card details with balance and limits
        /// 
   /// Comprehensive card information including current balance breakdown (available, reserved, used),
      /// active spending limits, merchant restrictions, and transaction history.
   /// </summary>
        [HttpGet("{cardId}/details")]
   [ProducesResponseType(typeof(CardDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCardDetails(int cardId)
{
   try
         {
 var card = await _cardService.GetCardAsync(cardId);
                if (card == null)
        return NotFound(new ErrorResponse { Code = "CARD_NOT_FOUND", Message = "Card not found" });

        var balance = await _balanceService.GetBalanceAsync(cardId);
          var limits = await _cardLimitService.GetCardLimitsAsync(cardId);

     var response = new CardDetailResponse
       {
          Card = MapCardToResponse(card),
         Balance = balance != null ? new BalanceResponse
       {
      AvailableBalance = balance.AvailableBalance,
                 ReservedBalance = balance.ReservedBalance,
            UsedBalance = balance.UsedBalance,
     Currency = balance.Currency,
      LastUpdated = balance.LastUpdated
    } : null,
         Limits = limits.Select(l => new CardLimitResponse
 {
    Id = l.Id,
         CardId = l.CardId,
          LimitType = l.LimitType,
           Amount = l.Amount,
        Period = l.Period,
      Threshold = l.Threshold,
        IsActive = l.IsActive,
              CreatedAt = l.CreatedAt
        }).ToList()
     };

              return Ok(response);
  }
        catch (Exception ex)
            {
    _logger.LogError(ex, "Error getting card details {CardId}", cardId);
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
     }
        }

     /// <summary>
        /// Update card settings and preferences
    /// 
        /// Modifies card properties such as nickname and international transaction settings for an existing card.
      /// Requires CEO, CFO, or Admin role.
        /// </summary>
        [HttpPut("{cardId}")]
        [Authorize(Roles = "CEO,CFO,Admin")]
        [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> UpdateCard(int cardId, [FromBody] UpdateCardRequest request)
        {
        try
          {
  var card = await _cardService.UpdateCardAsync(cardId, request.Nickname, request.AllowInternational);
                if (card == null)
              return NotFound(new ErrorResponse { Code = "CARD_NOT_FOUND", Message = "Card not found" });

                _logger.LogInformation("Card {CardId} updated", cardId);
        return Ok(MapCardToResponse(card));
  }
      catch (Exception ex)
   {
         _logger.LogError(ex, "Error updating card");
      return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
        }
        }

        /// <summary>
        /// Freeze a virtual card
        /// 
        /// Temporarily disables a virtual card for transactions while maintaining its data and allowing future reactivation.
        /// Requires CEO, CFO, or Admin role.
        /// </summary>
        [HttpPost("{cardId}/freeze")]
        [Authorize(Roles = "CEO,CFO,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
 public async Task<IActionResult> FreezeCard(int cardId, [FromBody] FreezeCardRequest request)
 {
         try
            {
       var result = await _cardService.FreezeCardAsync(cardId, request.Reason);
      if (!result)
  return NotFound(new ErrorResponse { Code = "CARD_NOT_FOUND", Message = "Card not found" });

    _logger.LogInformation("Card {CardId} frozen", cardId);
  return Ok(new { message = "Card frozen successfully" });
    }
            catch (Exception ex)
        {
    _logger.LogError(ex, "Error freezing card");
    return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
            }
        }

        /// <summary>
        /// Unfreeze a virtual card
        /// 
 /// Reactivates a previously frozen virtual card, allowing normal transaction processing to resume.
      /// Requires CEO, CFO, or Admin role.
   /// </summary>
    [HttpPost("{cardId}/unfreeze")]
   [Authorize(Roles = "CEO,CFO,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnfreezeCard(int cardId)
        {
         try
  {
       var result = await _cardService.UnfreezeCardAsync(cardId);
             if (!result)
return NotFound(new ErrorResponse { Code = "CARD_NOT_FOUND", Message = "Card not found" });

                _logger.LogInformation("Card {CardId} unfrozen", cardId);
      return Ok(new { message = "Card unfrozen successfully" });
            }
 catch (Exception ex)
      {
     _logger.LogError(ex, "Error unfreezing card");
             return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
      }
        }

        /// <summary>
        /// Cancel/Delete a virtual card
        /// 
        /// Permanently deactivates and removes a virtual card from the system. This action is irreversible.
     /// Requires CEO, CFO, or Admin role.
        /// </summary>
        [HttpDelete("{cardId}")]
        [Authorize(Roles = "CEO,CFO,Admin")]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelCard(int cardId)
        {
            try
   {
   var result = await _cardService.CancelCardAsync(cardId);
   if (!result)
      return NotFound(new ErrorResponse { Code = "CARD_NOT_FOUND", Message = "Card not found" });

     _logger.LogInformation("Card {CardId} cancelled", cardId);
      return NoContent();
    }
    catch (Exception ex)
    {
          _logger.LogError(ex, "Error cancelling card");
     return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
}
        }

        /// <summary>
        /// Set spending limits on a card
  /// 
 /// Configures spending limits with specified type (daily, monthly, transaction), amount, and threshold for alerts.
        /// Requires CEO, CFO, or Admin role.
        /// </summary>
   [HttpPost("{cardId}/limits")]
        [Authorize(Roles = "CEO,CFO,Admin")]
     [ProducesResponseType(typeof(CardLimitResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetCardLimit(int cardId, [FromBody] SetCardLimitRequest request)
        {
        try
      {
       var limit = await _cardLimitService.SetLimitAsync(cardId, request.LimitType, request.Amount, request.Period, request.Threshold);
        if (limit == null)
     return BadRequest(new ErrorResponse { Code = "LIMIT_SET_FAILED", Message = "Failed to set limit" });

   return CreatedAtAction(nameof(SetCardLimit), new { id = limit.Id }, new CardLimitResponse
              {
   Id = limit.Id,
     CardId = limit.CardId,
     LimitType = limit.LimitType,
            Amount = limit.Amount,
    Period = limit.Period,
   Threshold = limit.Threshold,
     IsActive = limit.IsActive,
            CreatedAt = limit.CreatedAt
   });
      }
            catch (Exception ex)
    {
        _logger.LogError(ex, "Error setting card limit");
      return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
  }
        }

    /// <summary>
 /// Get all spending limits for a card
        /// 
    /// Retrieves all active and inactive spending limits configured for a specific virtual card.
        /// </summary>
    [HttpGet("{cardId}/limits")]
        [ProducesResponseType(typeof(List<CardLimitResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCardLimits(int cardId)
        {
    try
        {
     var limits = await _cardLimitService.GetCardLimitsAsync(cardId);
           var response = limits.Select(l => new CardLimitResponse
   {
         Id = l.Id,
         CardId = l.CardId,
   LimitType = l.LimitType,
Amount = l.Amount,
 Period = l.Period,
       Threshold = l.Threshold,
       IsActive = l.IsActive,
           CreatedAt = l.CreatedAt
       }).ToList();

                return Ok(response);
            }
    catch (Exception ex)
{
         _logger.LogError(ex, "Error getting card limits");
   return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
     }
        }

        /// <summary>
     /// Get card balance
/// 
        /// Retrieves current balance information including available, reserved, and used amounts in the specified currency.
 /// </summary>
        [HttpGet("{cardId}/balance")]
   [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCardBalance(int cardId)
        {
            try
     {
          var balance = await _balanceService.GetBalanceAsync(cardId);
if (balance == null)
       return NotFound(new ErrorResponse { Code = "BALANCE_NOT_FOUND", Message = "Balance not found" });

 return Ok(new BalanceResponse
     {
     AvailableBalance = balance.AvailableBalance,
    ReservedBalance = balance.ReservedBalance,
      UsedBalance = balance.UsedBalance,
     Currency = balance.Currency,
            LastUpdated = balance.LastUpdated
      });
    }
            catch (Exception ex)
 {
   _logger.LogError(ex, "Error getting card balance");
return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
 }
   }

        /// <summary>
        /// Fund/Add balance to a virtual card
        /// 
 /// Adds funds to the available balance of a virtual card. This increases the cardholder's available funds
  /// and can be used for initial setup, periodic top-ups, or corrections. Requires CEO, CFO, or Admin role.
        /// Each funding transaction is logged and tracked for audit purposes.
        /// </summary>
/// <param name="cardId">The ID of the card to fund</param>
        /// <param name="request">Funding details including amount, reason, and optional reference ID</param>
     /// <returns>Updated card balance after funding</returns>
 [HttpPost("{cardId}/balance/fund")]
     [Authorize(Roles = "CEO,CFO,Admin")]
        [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FundCardBalance(int cardId, [FromBody] FundBalanceRequest request)
        {
         try
            {
           if (!ModelState.IsValid)
       return BadRequest(ModelState);

        var card = await _cardService.GetCardAsync(cardId);
     if (card == null)
   return NotFound(new ErrorResponse { Code = "CARD_NOT_FOUND", Message = "Card not found" });

        var balance = await _balanceService.FundBalanceAsync(cardId, request.Amount, request.Reason, request.ReferenceId);
    if (balance == null)
         return BadRequest(new ErrorResponse { Code = "FUNDING_FAILED", Message = "Failed to fund balance" });

    var userId = GetUserId();
                var ipAddress = HttpContext?.Connection.RemoteIpAddress?.ToString();
        await _auditService.LogActionAsync(userId, "BALANCE_FUNDED", "CardBalance", cardId, $"Amount: {request.Amount}, Reason: {request.Reason}", ipAddress, null);

     _logger.LogInformation("Card {CardId} funded with ${Amount} by user {UserId}", cardId, request.Amount, userId);

         return Ok(new BalanceResponse
  {
            AvailableBalance = balance.AvailableBalance,
  ReservedBalance = balance.ReservedBalance,
          UsedBalance = balance.UsedBalance,
          Currency = balance.Currency,
 LastUpdated = balance.LastUpdated
     });
            }
          catch (Exception ex)
      {
  _logger.LogError(ex, "Error funding card balance");
         return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred while funding balance" });
            }
        }

        /// <summary>
    /// Enable/Disable international transactions
   /// 
        /// Controls whether the virtual card can be used for international transactions based on compliance and business requirements.
      /// Requires CEO, CFO, or Admin role.
        /// </summary>
        [HttpPut("{cardId}/international")]
    [Authorize(Roles = "CEO,CFO,Admin")]
   [ProducesResponseType(StatusCodes.Status200OK)]
      public async Task<IActionResult> SetInternationalTransactions(int cardId, [FromBody] SetInternationalTransactionRequest request)
        {
         try
       {
        var card = await _cardService.GetCardAsync(cardId);
  if (card == null)
      return NotFound(new ErrorResponse { Code = "CARD_NOT_FOUND", Message = "Card not found" });

     var updated = await _cardService.UpdateCardAsync(cardId, allowInternational: request.AllowInternational);
    if (updated == null)
     return BadRequest(new ErrorResponse { Code = "UPDATE_FAILED", Message = "Failed to update card" });

    return Ok(new { message = "International transactions updated successfully" });
       }
 catch (Exception ex)
   {
    _logger.LogError(ex, "Error setting international transactions");
   return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Code = "INTERNAL_ERROR", Message = "An error occurred" });
            }
   }
  /// <summary>
        /// Maps card model to response DTO.
  /// </summary>
        private CardResponse MapCardToResponse(Models.VirtualCard card)
        {
       return new CardResponse
  {
      Id = card.Id,
     CardNumber = CardNumberMasking.MaskCardNumber(card.CardNumber),
  CardholderName = card.CardholderName,
Nickname = card.Nickname,
     ExpiryDate = card.ExpiryDate,
     Status = card.Status,
CardType = card.CardType,
       Currency = card.Currency,
    AllowInternational = card.AllowInternational,
      FreezeReason = card.FreezeReason,
                FrozenAt = card.FrozenAt,
      CreatedAt = card.CreatedAt,
       UpdatedAt = card.UpdatedAt
      };
        }

        /// <summary>
     /// Gets the current user ID from claims.
      /// </summary>
        private int? GetUserId()
        {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
  }
    }
}
