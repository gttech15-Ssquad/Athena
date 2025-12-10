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

     public async Task<VirtualCard?> CreateCardAsync(Guid userId, string cardholderName, string? nickname = null, Guid? departmentId = null)
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
      Currency = "NGN",
            AllowInternational = true,
       CreatedAt = DateTime.UtcNow,
 UpdatedAt = DateTime.UtcNow
      };

  var created = await _cardRepository.CreateAsync(card);

         var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        await _auditService.LogActionAsync(userId, "CARD_CREATED", "VirtualCard", created.Id, null, ipAddress, null);

      return created;
      }

        public async Task<VirtualCard?> GetCardAsync(Guid cardId)
        {
  return await _cardRepository.GetByIdAsync(cardId);
        }

     public async Task<List<VirtualCard>> GetUserCardsAsync(Guid userId)
  {
        return await _cardRepository.GetByUserIdAsync(userId);
        }

   public async Task<VirtualCard?> UpdateCardAsync(Guid cardId, string? nickname = null, bool? allowInternational = null)
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

    public async Task<bool> FreezeCardAsync(Guid cardId, string reason)
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

        public async Task<bool> UnfreezeCardAsync(Guid cardId)
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

  public async Task<bool> CancelCardAsync(Guid cardId)
    {
            var card = await _cardRepository.GetByIdAsync(cardId);
     if (card == null) return false;

        await _cardRepository.DeleteAsync(cardId);
       return true;
        }

        public async Task<(List<VirtualCard> items, int total)> GetUserCardsPaginatedAsync(Guid userId, int pageNumber, int pageSize)
     {
 return await _cardRepository.GetPaginatedByUserIdAsync(userId, pageNumber, pageSize);
    }

  private string GenerateCardNumber()
   {
      // Use CardNumberGenerator to create a valid Mastercard number with Luhn algorithm
  return CardNumberGenerator.GenerateMastercardNumber();
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

        public async Task<CardLimit?> SetLimitAsync(Guid cardId, string limitType, decimal amount, string period, decimal threshold = 80)
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

public async Task<List<CardLimit>> GetCardLimitsAsync(Guid cardId)
   {
            var card = await _cardRepository.GetByIdAsync(cardId);
   return card?.CardLimits.ToList() ?? new List<CardLimit>();
        }

        public async Task<CardLimit?> UpdateLimitAsync(Guid cardId, Guid limitId, decimal amount, decimal threshold)
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

public async Task<bool> RemoveLimitAsync(Guid cardId, Guid limitId)
      {
      var card = await _cardRepository.GetByIdAsync(cardId);
       if (card == null) return false;

       var limit = card.CardLimits.FirstOrDefault(l => l.Id == limitId);
  if (limit == null) return false;

            card.CardLimits.Remove(limit);
await _cardRepository.UpdateAsync(card);

     return true;
        }

        public async Task<(bool isValid, string? reason)> ValidateTransactionAsync(Guid cardId, decimal amount)
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

     public async Task<CardLimit?> GetActiveLimitAsync(Guid cardId)
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

        public async Task<CardBalance?> GetBalanceAsync(Guid cardId)
        {
      var card = await _cardRepository.GetByIdAsync(cardId);
   return card?.CardBalance;
   }

        public async Task<CardBalance> InitializeBalanceAsync(Guid cardId, decimal initialAmount, string currency = "NGN")
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

    public async Task<CardBalance?> UpdateBalanceAsync(Guid cardId, decimal amount, string transactionStatus)
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

      public async Task<CardBalance?> RecalculateBalanceAsync(Guid cardId)
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

  public async Task<CardBalance?> FundBalanceAsync(Guid cardId, decimal amount, string reason, string? referenceId = null)
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

    public async Task<List<(DateTime timestamp, decimal available, decimal reserved, decimal used)>> GetBalanceHistoryAsync(Guid cardId, DateTime startDate, DateTime endDate)
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

     public async Task<CardTransaction?> CreateTransactionAsync(Guid cardId, decimal amount, string merchant, string? merchantCategoryCode = null, string? referenceId = null)
  {
        var transaction = new CardTransaction
            {
   CardId = cardId,
     Amount = amount,
    Merchant = merchant,
         MerchantCategoryCode = merchantCategoryCode,
         ReferenceId = referenceId,
     Status = "PENDING",
          Currency = "NGN",
 CanBeDisputed = true,
 CreatedAt = DateTime.UtcNow,
     UpdatedAt = DateTime.UtcNow
       };

   return await _transactionRepository.CreateAsync(transaction);
  }

      public async Task<CardTransaction?> GetTransactionAsync(Guid transactionId)
      {
  return await _transactionRepository.GetByIdAsync(transactionId);
      }

        public async Task<(List<CardTransaction> items, int total)> GetCardTransactionsPaginatedAsync(Guid cardId, int pageNumber, int pageSize)
    {
            return await _transactionRepository.GetPaginatedByCardIdAsync(cardId, pageNumber, pageSize);
        }

    public async Task<bool> CompleteTransactionAsync(Guid transactionId)
      {
      var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null) return false;

     transaction.Status = "COMPLETED";
     transaction.CompletedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;

       await _transactionRepository.UpdateAsync(transaction);
 return true;
      }

    public async Task<bool> ReverseTransactionAsync(Guid transactionId, string reason)
        {
     var transaction = await _transactionRepository.GetByIdAsync(transactionId);
         if (transaction == null) return false;

     transaction.Status = "REVERSED";
   transaction.UpdatedAt = DateTime.UtcNow;

    await _transactionRepository.UpdateAsync(transaction);
     return true;
        }

      public async Task<bool> DisputeTransactionAsync(Guid transactionId, string reason)
      {
   var transaction = await _transactionRepository.GetByIdAsync(transactionId);
    if (transaction == null || !transaction.CanBeDisputed) return false;

        transaction.DisputeReason = reason;
 transaction.UpdatedAt = DateTime.UtcNow;

    await _transactionRepository.UpdateAsync(transaction);
 return true;
      }

      public async Task<List<CardTransaction>> GetTransactionsByDateRangeAsync(Guid cardId, DateTime startDate, DateTime endDate)
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

public async Task<CardApproval?> RequestApprovalAsync(Guid cardId, string actionType, Guid requestedBy, string? actionData = null)
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

      public async Task<bool> ApproveAsync(Guid approvalId, Guid userId, string? reason = null)
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

     public async Task<bool> RejectAsync(Guid approvalId, Guid userId, string reason)
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

        public async Task<List<CardApproval>> GetApprovalHistoryAsync(Guid cardId)
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

  public async Task SendNotificationAsync(Guid userId, string title, string message, string type)
        {
    await Task.Run(() => 
  {
    _logger.LogInformation("Notification sent to user {UserId}: {Title} - {Message}", userId, title, message);
   // TODO: Implement actual notification sending (email, SMS, push notification)
      });
        }

      public async Task SendLimitWarningAsync(Guid cardId, string limitType, decimal usedAmount, decimal limit)
        {
        await Task.Run(() => 
    {
           var percentage = (usedAmount / limit) * 100;
     _logger.LogWarning("Limit warning for card {CardId}: {LimitType} usage at {Percentage}%", cardId, limitType, percentage);
      });
        }

      public async Task SendApprovalRequestAsync(Guid approvalId, List<Guid> approverIds)
        {
   await Task.Run(() => 
      {
       _logger.LogInformation("Approval request {ApprovalId} sent to {ApproverCount} approvers", approvalId, approverIds.Count);
     });
        }

   public async Task SendTransactionNotificationAsync(Guid cardId, decimal amount, string merchant, string status)
        {
            await Task.Run(() => 
       {
          _logger.LogInformation("Transaction notification for card {CardId}: ${Amount} at {Merchant} - {Status}", cardId, amount, merchant, status);
  });
        }

public async Task SendCardStatusNotificationAsync(Guid cardId, string oldStatus, string newStatus)
  {
        await Task.Run(() => 
 {
 _logger.LogInformation("Card {CardId} status changed from {OldStatus} to {NewStatus}", cardId, oldStatus, newStatus);
   });
        }
    }

    /// <summary>
    /// Implementation of account balance service.
 /// </summary>
    public class AccountBalanceServiceImpl : IAccountBalanceService
    {
  private readonly IRepository<AccountBalance> _accountBalanceRepository;
        private readonly IRepository<AccountTransaction> _accountTransactionRepository;
        private readonly IRepository<User> _userRepository;
        private readonly ILogger<AccountBalanceServiceImpl> _logger;

   public AccountBalanceServiceImpl(
    IRepository<AccountBalance> accountBalanceRepository,
  IRepository<AccountTransaction> accountTransactionRepository,
    IRepository<User> userRepository,
        ILogger<AccountBalanceServiceImpl> logger)
 {
          _accountBalanceRepository = accountBalanceRepository;
    _accountTransactionRepository = accountTransactionRepository;
     _userRepository = userRepository;
    _logger = logger;
  }

 public async Task<AccountBalance?> GetOrCreateAccountBalanceAsync(Guid userId, string currency = "NGN")
   {
  var balance = await GetAccountBalanceAsync(userId);
 if (balance != null)
    return balance;

     var newBalance = new AccountBalance
   {
  UserId = userId,
  AvailableBalance = 0,
   TotalFunded = 0,
TotalWithdrawn = 0,
    Currency = currency,
       CreatedAt = DateTime.UtcNow,
   UpdatedAt = DateTime.UtcNow
   };

     try
       {
return await _accountBalanceRepository.CreateAsync(newBalance);
  }
      catch (Exception ex)
   {
   _logger.LogError(ex, "Error creating account balance for user {UserId}", userId);
         return null;
  }
    }

 public async Task<AccountBalance?> GetAccountBalanceAsync(Guid userId)
    {
 try
      {
      var balances = await _accountBalanceRepository.GetAllAsync();
   return balances.FirstOrDefault(b => b.UserId == userId);
    }
     catch (Exception ex)
       {
    _logger.LogError(ex, "Error retrieving account balance for user {UserId}", userId);
    return null;
}
     }

     public async Task<AccountBalance?> FundAccountAsync(Guid userId, decimal amount, string reason, string? referenceId = null, Guid? initiatedBy = null)
    {
    if (amount <= 0)
{
  _logger.LogWarning("Invalid funding amount {Amount} for user {UserId}", amount, userId);
       return null;
       }

 var balance = await GetOrCreateAccountBalanceAsync(userId);
    if (balance == null)
          return null;

    var balanceBefore = balance.AvailableBalance;
  balance.AddFunds(amount);
     balance.UpdatedAt = DateTime.UtcNow;

     try
       {
     await _accountBalanceRepository.UpdateAsync(balance);

         var transaction = new AccountTransaction
  {
   AccountBalanceId = balance.Id,
        TransactionType = "FUNDING",
      Amount = amount,
   BalanceBefore = balanceBefore,
         BalanceAfter = balance.AvailableBalance,
 Description = reason,
         ReferenceId = referenceId,
       InitiatedBy = initiatedBy,
        Currency = balance.Currency,
      Status = "COMPLETED",
     CreatedAt = DateTime.UtcNow,
       CompletedAt = DateTime.UtcNow
 };

    await _accountTransactionRepository.CreateAsync(transaction);

     _logger.LogInformation(
         "Account {UserId} funded with ${Amount}. Balance: ${BalanceBefore} -> ${BalanceAfter}",
       userId, amount, balanceBefore, balance.AvailableBalance);

         return balance;
      }
    catch (Exception ex)
        {
  _logger.LogError(ex, "Error funding account for user {UserId}", userId);
         return null;
   }
        }

      public async Task<(bool success, string? errorMessage)> WithdrawFromAccountAsync(
    Guid userId, decimal amount, string reason, Guid? relatedCardId = null,
    Guid? relatedCardTransactionId = null, string? referenceId = null, Guid? initiatedBy = null)
     {
     if (amount <= 0)
{
    return (false, "Invalid withdrawal amount");
    }

      var balance = await GetAccountBalanceAsync(userId);
     if (balance == null)
{
     return (false, "Account not found");
         }

            if (balance.AvailableBalance < amount)
            {
     _logger.LogWarning(
     "Insufficient funds for user {UserId}. Required: ${Amount}, Available: ${AvailableBalance}",
        userId, amount, balance.AvailableBalance);
  return (false, "Insufficient funds");
  }

  var balanceBefore = balance.AvailableBalance;
         balance.DeductBalance(amount);
balance.UpdatedAt = DateTime.UtcNow;

    try
       {
        await _accountBalanceRepository.UpdateAsync(balance);

      var transaction = new AccountTransaction
  {
        AccountBalanceId = balance.Id,
   TransactionType = "WITHDRAWAL",
    Amount = amount,
    BalanceBefore = balanceBefore,
     BalanceAfter = balance.AvailableBalance,
      Description = reason,
          ReferenceId = referenceId,
      InitiatedBy = initiatedBy,
      Currency = balance.Currency,
   Status = "COMPLETED",
 CreatedAt = DateTime.UtcNow,
    CompletedAt = DateTime.UtcNow,
    RelatedCardId = relatedCardId,
RelatedCardTransactionId = relatedCardTransactionId
      };

 await _accountTransactionRepository.CreateAsync(transaction);

_logger.LogInformation(
  "Withdrawal of ${Amount} from account {UserId}. Balance: ${BalanceBefore} -> ${BalanceAfter}",
       amount, userId, balanceBefore, balance.AvailableBalance);

  return (true, null);
    }
  catch (Exception ex)
            {
    _logger.LogError(ex, "Error withdrawing from account for user {UserId}", userId);
 return (false, "Withdrawal failed");
          }
        }

   public async Task<(List<AccountTransaction> items, int total)> GetAccountTransactionsPaginatedAsync(
  Guid userId, int pageNumber, int pageSize, string? transactionType = null)
  {
    try
{
         var balance = await GetAccountBalanceAsync(userId);
     if (balance == null)
   return (new List<AccountTransaction>(), 0);

        var allTransactions = balance.Transactions ?? new List<AccountTransaction>();

   if (!string.IsNullOrEmpty(transactionType))
    {
  allTransactions = allTransactions
     .Where(t => t.TransactionType == transactionType)
           .ToList();
           }

     allTransactions = allTransactions.OrderByDescending(t => t.CreatedAt).ToList();

         var totalCount = allTransactions.Count;
        var paginatedItems = allTransactions
       .Skip((pageNumber - 1) * pageSize)
       .Take(pageSize)
  .ToList();

       return (paginatedItems, totalCount);
            }
 catch (Exception ex)
     {
     _logger.LogError(ex, "Error getting account transactions for user {UserId}", userId);
    return (new List<AccountTransaction>(), 0);
      }
        }

 public async Task<(decimal available, decimal totalFunded, decimal totalWithdrawn, int cardsFunded, int activeTransactions)> GetAccountSummaryAsync(Guid userId)
    {
try
         {
        var balance = await GetAccountBalanceAsync(userId);
    if (balance == null)
              {
       return (0, 0, 0, 0, 0);
     }

var transactions = balance.Transactions ?? new List<AccountTransaction>();
        var cardsFunded = transactions
        .Where(t => t.TransactionType == "FUNDING" && t.Status == "COMPLETED")
       .Select(t => t.RelatedCardId)
  .Distinct()
      .Count();

       var activeTransactions = transactions
         .Where(t => t.Status == "PENDING")
    .Count();

      return (
         balance.AvailableBalance,
          balance.TotalFunded,
balance.TotalWithdrawn,
  cardsFunded,
          activeTransactions
           );
}
   catch (Exception ex)
     {
 _logger.LogError(ex, "Error getting account summary for user {UserId}", userId);
      return (0, 0, 0, 0, 0);
    }
        }

    public async Task<bool> ReverseTransactionAsync(Guid transactionId, string reason)
        {
      try
       {
         var allTransactions = await _accountTransactionRepository.GetAllAsync();
     var transaction = allTransactions.FirstOrDefault(t => t.Id == transactionId);

  if (transaction == null || transaction.Status == "REVERSED")
         return false;

        var balance = await _accountBalanceRepository.GetByIdAsync(transaction.AccountBalanceId);
   if (balance == null)
  return false;

         if (transaction.TransactionType == "FUNDING")
           {
    balance.AvailableBalance -= transaction.Amount;
        balance.TotalFunded -= transaction.Amount;
    }
         else if (transaction.TransactionType == "WITHDRAWAL")
    {
     balance.AvailableBalance += transaction.Amount;
 balance.TotalWithdrawn -= transaction.Amount;
    }

  balance.UpdatedAt = DateTime.UtcNow;
       await _accountBalanceRepository.UpdateAsync(balance);

        transaction.Status = "REVERSED";
           transaction.CompletedAt = DateTime.UtcNow;
     await _accountTransactionRepository.UpdateAsync(transaction);

         _logger.LogInformation(
    "Account transaction {TransactionId} reversed. Reason: {Reason}",
   transactionId, reason);

          return true;
   }
   catch (Exception ex)
       {
       _logger.LogError(ex, "Error reversing account transaction {TransactionId}", transactionId);
      return false;
         }
}

    public async Task<List<AccountTransaction>> GetTransactionsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
       try
    {
         var balance = await GetAccountBalanceAsync(userId);
     if (balance == null)
      return new List<AccountTransaction>();

      var transactions = balance.Transactions ?? new List<AccountTransaction>();

    return transactions
  .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
 .OrderByDescending(t => t.CreatedAt)
        .ToList();
       }
    catch (Exception ex)
 {
_logger.LogError(ex, "Error getting account transactions by date range for user {UserId}", userId);
         return new List<AccountTransaction>();
            }
 }
    }
}
