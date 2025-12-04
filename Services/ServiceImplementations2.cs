using virtupay_corporate.Helpers;
using virtupay_corporate.Models;
using virtupay_corporate.Repositories;

namespace virtupay_corporate.Services
{
    /// <summary>
/// Implementation of card service.
    /// </summary>
    public class CardServiceImpl : ICardService
    {
  private readonly ICardRepository _cardRepository;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

     public CardServiceImpl(ICardRepository cardRepository, IAuditService auditService, IHttpContextAccessor httpContextAccessor)
    {
       _cardRepository = cardRepository;
    _auditService = auditService;
_httpContextAccessor = httpContextAccessor;
        }

     public async Task<VirtualCard?> CreateCardAsync(int userId, string cardholderName, string? nickname = null, int? departmentId = null)
        {
   var cardNumber = GenerateCardNumber();
    var cvv = GenerateCVV();
   var expiryDate = DateTime.UtcNow.AddYears(4);

     var card = new VirtualCard
     {
   UserId = userId,
     CardNumber = cardNumber,
                CVV = cvv,
    ExpiryDate = expiryDate,
 CardholderName = cardholderName,
          Nickname = nickname,
                Status = "ACTIVE",
            CardType = "CREDIT",
                Currency = "USD",
            AllowInternational = true,
       CreatedAt = DateTime.UtcNow,
     UpdatedAt = DateTime.UtcNow
          };

            var created = await _cardRepository.CreateAsync(card);

            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        await _auditService.LogActionAsync(userId, "CARD_CREATED", "VirtualCard", created.Id, null, ipAddress, null);

      return created;
      }

        public async Task<VirtualCard?> GetCardAsync(int cardId)
        {
         return await _cardRepository.GetByIdAsync(cardId);
        }

        public async Task<List<VirtualCard>> GetUserCardsAsync(int userId)
  {
            return await _cardRepository.GetByUserIdAsync(userId);
        }

   public async Task<VirtualCard?> UpdateCardAsync(int cardId, string? nickname = null, bool? allowInternational = null)
        {
var card = await _cardRepository.GetByIdAsync(cardId);
        if (card == null) return null;

        if (!string.IsNullOrEmpty(nickname))
             card.Nickname = nickname;

    if (allowInternational.HasValue)
     card.AllowInternational = allowInternational.Value;

 card.UpdatedAt = DateTime.UtcNow;
     return await _cardRepository.UpdateAsync(card);
      }

    public async Task<bool> FreezeCardAsync(int cardId, string reason)
        {
      var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null) return false;

            card.Status = "FROZEN";
         card.FreezeReason = reason;
      card.FrozenAt = DateTime.UtcNow;
            card.UpdatedAt = DateTime.UtcNow;

   await _cardRepository.UpdateAsync(card);
    return true;
        }

        public async Task<bool> UnfreezeCardAsync(int cardId)
        {
   var card = await _cardRepository.GetByIdAsync(cardId);
    if (card == null || card.Status != "FROZEN") return false;

            card.Status = "ACTIVE";
            card.FreezeReason = null;
    card.FrozenAt = null;
            card.UpdatedAt = DateTime.UtcNow;

            await _cardRepository.UpdateAsync(card);
            return true;
        }

  public async Task<bool> CancelCardAsync(int cardId)
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
     if (card == null) return false;

        await _cardRepository.DeleteAsync(cardId);
         return true;
        }

        public async Task<(List<VirtualCard> items, int total)> GetUserCardsPaginatedAsync(int userId, int pageNumber, int pageSize)
     {
 return await _cardRepository.GetPaginatedByUserIdAsync(userId, pageNumber, pageSize);
        }

        private string GenerateCardNumber()
   {
      // Generate random 16-digit card number
            var random = new Random();
   return string.Concat(Enumerable.Range(0, 16).Select(_ => random.Next(0, 10)));
        }

        private string GenerateCVV()
        {
            // Generate random 3-digit CVV
       var random = new Random();
            return string.Concat(Enumerable.Range(0, 3).Select(_ => random.Next(0, 10)));
        }
    }

    /// <summary>
    /// Implementation of card limit service.
    /// </summary>
    public class CardLimitServiceImpl : ICardLimitService
    {
        private readonly ICardRepository _cardRepository;

     public CardLimitServiceImpl(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }

        public async Task<CardLimit?> SetLimitAsync(int cardId, string limitType, decimal amount, string period, decimal threshold = 80)
{
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null) return null;

            var limit = new CardLimit
   {
    CardId = cardId,
           LimitType = limitType,
    Amount = amount,
    Period = period,
            Threshold = threshold,
     IsActive = true,
      CreatedAt = DateTime.UtcNow,
 UpdatedAt = DateTime.UtcNow
            };

         card.CardLimits.Add(limit);
            await _cardRepository.UpdateAsync(card);

     return limit;
 }

        public async Task<List<CardLimit>> GetCardLimitsAsync(int cardId)
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            return card?.CardLimits.ToList() ?? new List<CardLimit>();
        }

        public async Task<CardLimit?> UpdateLimitAsync(int cardId, int limitId, decimal amount, decimal threshold)
        {
     var card = await _cardRepository.GetByIdAsync(cardId);
        if (card == null) return null;

      var limit = card.CardLimits.FirstOrDefault(l => l.Id == limitId);
 if (limit == null) return null;

 limit.Amount = amount;
       limit.Threshold = threshold;
            limit.UpdatedAt = DateTime.UtcNow;

            await _cardRepository.UpdateAsync(card);
         return limit;
        }

public async Task<bool> RemoveLimitAsync(int cardId, int limitId)
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
       if (card == null) return false;

            var limit = card.CardLimits.FirstOrDefault(l => l.Id == limitId);
  if (limit == null) return false;

            card.CardLimits.Remove(limit);
            await _cardRepository.UpdateAsync(card);

       return true;
        }

        public async Task<(bool isValid, string? reason)> ValidateTransactionAsync(int cardId, decimal amount)
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
      if (card == null)
        return (false, "Card not found");

      var limits = await GetCardLimitsAsync(cardId);
            foreach (var limit in limits.Where(l => l.IsActive))
 {
          if (amount > limit.Amount)
  return (false, $"Amount exceeds {limit.LimitType} limit of {limit.Amount}");
 }

            return (true, null);
     }

     public async Task<CardLimit?> GetActiveLimitAsync(int cardId)
        {
      var limits = await GetCardLimitsAsync(cardId);
 return limits.FirstOrDefault(l => l.IsActive);
    }
    }

  /// <summary>
    /// Implementation of balance service.
    /// </summary>
    public class BalanceServiceImpl : IBalanceService
    {
        private readonly ICardRepository _cardRepository;

        public BalanceServiceImpl(ICardRepository cardRepository)
        {
        _cardRepository = cardRepository;
    }

        public async Task<CardBalance?> GetBalanceAsync(int cardId)
        {
      var card = await _cardRepository.GetByIdAsync(cardId);
   return card?.CardBalance;
        }

        public async Task<CardBalance> InitializeBalanceAsync(int cardId, decimal initialAmount, string currency = "USD")
        {
      var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null) throw new Exception("Card not found");

       var balance = new CardBalance
        {
   CardId = cardId,
    AvailableBalance = initialAmount,
                ReservedBalance = 0,
        UsedBalance = 0,
         Currency = currency,
   LastUpdated = DateTime.UtcNow
            };

            card.CardBalance = balance;
            await _cardRepository.UpdateAsync(card);

            return balance;
 }

    public async Task<CardBalance?> UpdateBalanceAsync(int cardId, decimal amount, string transactionStatus)
        {
         var balance = await GetBalanceAsync(cardId);
          if (balance == null) return null;

            if (transactionStatus == "PENDING")
   {
    balance.ReservedBalance += amount;
              balance.AvailableBalance -= amount;
  }
            else if (transactionStatus == "COMPLETED")
     {
      balance.ReservedBalance -= amount;
      balance.UsedBalance += amount;
            }
      else if (transactionStatus == "REVERSED")
            {
    balance.ReservedBalance -= amount;
    balance.AvailableBalance += amount;
   }

       balance.LastUpdated = DateTime.UtcNow;
 return balance;
        }

      public async Task<CardBalance?> RecalculateBalanceAsync(int cardId)
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
    if (card?.CardBalance == null) return null;

          var transactions = card.Transactions ?? new List<CardTransaction>();

 card.CardBalance.UsedBalance = transactions
       .Where(t => t.Status == "COMPLETED")
        .Sum(t => t.Amount);

    card.CardBalance.ReservedBalance = transactions
     .Where(t => t.Status == "PENDING")
    .Sum(t => t.Amount);

        card.CardBalance.LastUpdated = DateTime.UtcNow;
   return card.CardBalance;
        }

  /// <summary>
        /// Funds/adds balance to a card account.
        /// </summary>
        public async Task<CardBalance?> FundBalanceAsync(int cardId, decimal amount, string reason, string? referenceId = null)
        {
    var balance = await GetBalanceAsync(cardId);
       if (balance == null) return null;

            balance.AvailableBalance += amount;
     balance.LastUpdated = DateTime.UtcNow;

      var card = await _cardRepository.GetByIdAsync(cardId);
            if (card != null)
{
     await _cardRepository.UpdateAsync(card);
   }

 return balance;
        }

        public async Task<List<(DateTime timestamp, decimal available, decimal reserved, decimal used)>> GetBalanceHistoryAsync(int cardId, DateTime startDate, DateTime endDate)
  {
    // TODO: Implement balance history tracking
            return new List<(DateTime, decimal, decimal, decimal)>();
        }
 }

    /// <summary>
    /// Implementation of transaction service.
    /// </summary>
    public class TransactionServiceImpl : ITransactionService
    {
      private readonly ITransactionRepository _transactionRepository;
  private readonly ICardRepository _cardRepository;

        public TransactionServiceImpl(ITransactionRepository transactionRepository, ICardRepository cardRepository)
        {
    _transactionRepository = transactionRepository;
         _cardRepository = cardRepository;
        }

        public async Task<CardTransaction?> CreateTransactionAsync(int cardId, decimal amount, string merchant, string? merchantCategoryCode = null, string? referenceId = null)
  {
        var transaction = new CardTransaction
            {
       CardId = cardId,
     Amount = amount,
                Merchant = merchant,
         MerchantCategoryCode = merchantCategoryCode,
           ReferenceId = referenceId,
           Status = "PENDING",
          Currency = "USD",
     CanBeDisputed = true,
 CreatedAt = DateTime.UtcNow,
     UpdatedAt = DateTime.UtcNow
       };

   return await _transactionRepository.CreateAsync(transaction);
        }

        public async Task<CardTransaction?> GetTransactionAsync(int transactionId)
      {
            return await _transactionRepository.GetByIdAsync(transactionId);
        }

        public async Task<(List<CardTransaction> items, int total)> GetCardTransactionsPaginatedAsync(int cardId, int pageNumber, int pageSize)
        {
            return await _transactionRepository.GetPaginatedByCardIdAsync(cardId, pageNumber, pageSize);
        }

    public async Task<bool> CompleteTransactionAsync(int transactionId)
        {
      var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null) return false;

     transaction.Status = "COMPLETED";
     transaction.CompletedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;

            await _transactionRepository.UpdateAsync(transaction);
 return true;
        }

    public async Task<bool> ReverseTransactionAsync(int transactionId, string reason)
        {
         var transaction = await _transactionRepository.GetByIdAsync(transactionId);
         if (transaction == null) return false;

            transaction.Status = "REVERSED";
   transaction.UpdatedAt = DateTime.UtcNow;

    await _transactionRepository.UpdateAsync(transaction);
     return true;
        }

        public async Task<bool> DisputeTransactionAsync(int transactionId, string reason)
        {
   var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null || !transaction.CanBeDisputed) return false;

        transaction.DisputeReason = reason;
 transaction.UpdatedAt = DateTime.UtcNow;

    await _transactionRepository.UpdateAsync(transaction);
 return true;
        }

      public async Task<List<CardTransaction>> GetTransactionsByDateRangeAsync(int cardId, DateTime startDate, DateTime endDate)
        {
  return await _transactionRepository.GetByDateRangeAsync(cardId, startDate, endDate);
        }
    }

    /// <summary>
    /// Implementation of approval service.
    /// </summary>
    public class ApprovalServiceImpl : IApprovalService
    {
        private readonly IApprovalRepository _approvalRepository;

        public ApprovalServiceImpl(IApprovalRepository approvalRepository)
        {
    _approvalRepository = approvalRepository;
        }

public async Task<CardApproval?> RequestApprovalAsync(int cardId, string actionType, int requestedBy, string? actionData = null)
        {
            var approval = new CardApproval
            {
            CardId = cardId,
       ActionType = actionType,
            RequestedBy = requestedBy,
   Status = "PENDING",
              ActionData = actionData,
  CreatedAt = DateTime.UtcNow,
          ExpiresAt = DateTime.UtcNow.AddHours(48)
  };

    return await _approvalRepository.CreateAsync(approval);
        }

        public async Task<List<CardApproval>> GetPendingApprovalsAsync()
        {
            return await _approvalRepository.GetPendingAsync();
        }

        public async Task<bool> ApproveAsync(int approvalId, int userId, string? reason = null)
        {
            var approval = await _approvalRepository.GetByIdAsync(approvalId);
       if (approval == null || approval.Status != "PENDING") return false;

        approval.Status = "APPROVED";
            approval.ApprovedBy = userId;
     approval.Reason = reason;
     approval.ResolvedAt = DateTime.UtcNow;

    await _approvalRepository.UpdateAsync(approval);
            return true;
        }

     public async Task<bool> RejectAsync(int approvalId, int userId, string reason)
 {
   var approval = await _approvalRepository.GetByIdAsync(approvalId);
            if (approval == null || approval.Status != "PENDING") return false;

     approval.Status = "REJECTED";
       approval.ApprovedBy = userId;
            approval.Reason = reason;
            approval.ResolvedAt = DateTime.UtcNow;

  await _approvalRepository.UpdateAsync(approval);
    return true;
   }

        public async Task<List<CardApproval>> GetApprovalHistoryAsync(int cardId)
      {
            return await _approvalRepository.GetByCardIdAsync(cardId);
        }

     public async Task<bool> IsApprovalRequiredAsync(string actionType, string userRole)
      {
         return actionType switch
            {
   "DELETE_CARD" => true,
     "FREEZE_CARD" => userRole != "CEO",
            "CHANGE_LIMITS" => userRole != "CFO",
           "ENABLE_INTERNATIONAL" => userRole != "CFO",
    _ => false
     };
        }
    }

    /// <summary>
    /// Implementation of notification service.
    /// </summary>
    public class NotificationServiceImpl : INotificationService
    {
        private readonly ILogger<NotificationServiceImpl> _logger;

  public NotificationServiceImpl(ILogger<NotificationServiceImpl> logger)
        {
        _logger = logger;
        }

        public async Task SendNotificationAsync(int userId, string title, string message, string type)
        {
          await Task.Run(() => 
  {
       _logger.LogInformation("Notification sent to user {UserId}: {Title} - {Message}", userId, title, message);
         // TODO: Implement actual notification sending (email, SMS, push notification)
            });
        }

        public async Task SendLimitWarningAsync(int cardId, string limitType, decimal usedAmount, decimal limit)
        {
        await Task.Run(() => 
          {
             var percentage = (usedAmount / limit) * 100;
     _logger.LogWarning("Limit warning for card {CardId}: {LimitType} usage at {Percentage}%", cardId, limitType, percentage);
            });
        }

        public async Task SendApprovalRequestAsync(int approvalId, List<int> approverIds)
        {
   await Task.Run(() => 
      {
       _logger.LogInformation("Approval request {ApprovalId} sent to {ApproverCount} approvers", approvalId, approverIds.Count);
            });
        }

        public async Task SendTransactionNotificationAsync(int cardId, decimal amount, string merchant, string status)
        {
            await Task.Run(() => 
       {
          _logger.LogInformation("Transaction notification for card {CardId}: ${Amount} at {Merchant} - {Status}", cardId, amount, merchant, status);
  });
        }

public async Task SendCardStatusNotificationAsync(int cardId, string oldStatus, string newStatus)
        {
            await Task.Run(() => 
   {
 _logger.LogInformation("Card {CardId} status changed from {OldStatus} to {NewStatus}", cardId, oldStatus, newStatus);
       });
        }
    }
}
