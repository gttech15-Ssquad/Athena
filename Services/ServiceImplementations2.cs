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
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationUserRepository _organizationUserRepository;

     public CardServiceImpl(
            ICardRepository cardRepository, 
            IAuditService auditService, 
            IHttpContextAccessor httpContextAccessor,
            IOrganizationService organizationService,
            IOrganizationUserRepository organizationUserRepository)
    {
       _cardRepository = cardRepository;
    _auditService = auditService;
_httpContextAccessor = httpContextAccessor;
            _organizationService = organizationService;
            _organizationUserRepository = organizationUserRepository;
        }

     public async Task<VirtualCard?> CreateCardAsync(Guid organizationId, Guid ownerMembershipId, string cardholderName, string? nickname = null, Guid? departmentId = null)
        {
            // Verify membership exists and belongs to organization
            var membership = await _organizationUserRepository.GetByIdAsync(ownerMembershipId);
            if (membership == null || membership.OrganizationId != organizationId || membership.Status != "Active")
            {
                return null;
            }

   var cardNumber = GenerateCardNumber();
    var cvv = GenerateCVV();
   var expiryDate = DateTime.UtcNow.AddYears(4);

     var card = new VirtualCard
     {
   OrganizationId = organizationId,
            OwnerMembershipId = ownerMembershipId,
            UserId = membership.UserId, // Backward compatibility
     CardNumber = cardNumber,
            CVV = cvv,
    ExpiryDate = expiryDate,
 CardholderName = cardholderName,
  Nickname = nickname,
Status = "Active", // Card is immediately active for owner-created cards
            CardType = "CREDIT",
      Currency = "NGN",
            AllowInternational = true,
       CreatedAt = DateTime.UtcNow,
 UpdatedAt = DateTime.UtcNow
      };

  var created = await _cardRepository.CreateAsync(card);

         var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        await _auditService.LogActionAsync(membership.UserId, "CARD_CREATED", "VirtualCard", created.Id, $"Organization: {organizationId}", ipAddress, null);

      return created;
      }

        public async Task<VirtualCard?> GetCardAsync(Guid cardId)
        {
  return await _cardRepository.GetByIdAsync(cardId);
        }

     public async Task<List<VirtualCard>> GetOrganizationCardsAsync(Guid organizationId)
        {
            return await _cardRepository.GetByOrganizationIdAsync(organizationId);
        }

        public async Task<List<VirtualCard>> GetUserCardsAsync(Guid organizationId, Guid userId)
  {
            // Get cards for user within the organization
            var allOrgCards = await _cardRepository.GetByOrganizationIdAsync(organizationId);
            return allOrgCards.Where(c => c.OwnerMembership?.UserId == userId || c.UserId == userId).ToList();
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

    public async Task<bool> FreezeCardAsync(Guid cardId, string reason, Guid requestedByMembershipId)
        {
      var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null) return false;

            // Verify requester belongs to same organization
            var membership = await _organizationUserRepository.GetByIdAsync(requestedByMembershipId);
            if (membership == null || membership.OrganizationId != card.OrganizationId)
            {
                return false;
            }

   card.Status = "FROZEN";
      card.FreezeReason = reason;
      card.FrozenAt = DateTime.UtcNow;
            card.UpdatedAt = DateTime.UtcNow;

   await _cardRepository.UpdateAsync(card);
    return true;
        }

        public async Task<bool> UnfreezeCardAsync(Guid cardId, Guid requestedByMembershipId)
        {
   var card = await _cardRepository.GetByIdAsync(cardId);
 if (card == null || card.Status != "FROZEN") return false;

            // Verify requester belongs to same organization
            var membership = await _organizationUserRepository.GetByIdAsync(requestedByMembershipId);
            if (membership == null || membership.OrganizationId != card.OrganizationId)
            {
                return false;
            }

            card.Status = "ACTIVE";
   card.FreezeReason = null;
  card.FrozenAt = null;
        card.UpdatedAt = DateTime.UtcNow;

    await _cardRepository.UpdateAsync(card);
            return true;
        }

  public async Task<bool> CancelCardAsync(Guid cardId, Guid requestedByMembershipId)
    {
            var card = await _cardRepository.GetByIdAsync(cardId);
     if (card == null) return false;

            // Verify requester belongs to same organization
            var membership = await _organizationUserRepository.GetByIdAsync(requestedByMembershipId);
            if (membership == null || membership.OrganizationId != card.OrganizationId)
            {
                return false;
            }

        await _cardRepository.DeleteAsync(cardId);
       return true;
        }

        public async Task<(List<VirtualCard> items, int total)> GetOrganizationCardsPaginatedAsync(Guid organizationId, int pageNumber, int pageSize)
     {
            var allCards = await _cardRepository.GetByOrganizationIdAsync(organizationId);
            var total = allCards.Count;
            var items = allCards
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return (items, total);
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
    Id = Guid.NewGuid(),
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
        private readonly IOrganizationService _organizationService;
        private readonly ICardRepository _cardRepository;
        private readonly IOrganizationUserRepository _organizationUserRepository;

        public ApprovalServiceImpl(
            IApprovalRepository approvalRepository,
            IOrganizationService organizationService,
            ICardRepository cardRepository,
            IOrganizationUserRepository organizationUserRepository)
        {
    _approvalRepository = approvalRepository;
            _organizationService = organizationService;
            _cardRepository = cardRepository;
            _organizationUserRepository = organizationUserRepository;
        }

public async Task<CardApproval?> RequestApprovalAsync(Guid cardId, string actionType, Guid requestedByMembershipId, string? actionData = null)
        {
            // Get card to find organization
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null) return null;

            // Get requester membership
            var requesterMembership = await _organizationUserRepository.GetByIdAsync(requestedByMembershipId);
            if (requesterMembership == null || requesterMembership.OrganizationId != card.OrganizationId)
            {
                return null;
            }

  var approval = new CardApproval
            {
            OrganizationId = card.OrganizationId,
            CardId = cardId,
       ActionType = actionType,
      RequestedByMembershipId = requestedByMembershipId,
      RequestedBy = requesterMembership.UserId, // Backward compatibility
   Status = "PENDING",
              ActionData = actionData,
  CreatedAt = DateTime.UtcNow,
          ExpiresAt = DateTime.UtcNow.AddHours(48)
  };

    return await _approvalRepository.CreateAsync(approval);
  }

    public async Task<List<CardApproval>> GetPendingApprovalsAsync(Guid organizationId)
      {
            var allPending = await _approvalRepository.GetPendingAsync();
            return allPending.Where(a => a.OrganizationId == organizationId).ToList();
        }

      public async Task<bool> ApproveAsync(Guid approvalId, Guid approverMembershipId, string? reason = null)
      {
            var approval = await _approvalRepository.GetByIdAsync(approvalId);
       if (approval == null || approval.Status != "PENDING") return false;

            // Get approver membership
            var approverMembership = await _organizationUserRepository.GetByIdAsync(approverMembershipId);
            if (approverMembership == null || approverMembership.OrganizationId != approval.OrganizationId)
            {
                return false;
            }

            // Get requester membership to check role hierarchy
            var requesterMembership = await _organizationUserRepository.GetByIdAsync(approval.RequestedByMembershipId);
            if (requesterMembership == null || requesterMembership.OrganizationId != approval.OrganizationId)
            {
                return false;
            }

            // Verify approver has higher or equal role (Owner > Admin > Approver > Viewer)
            var canApprove = await _organizationService.HasRoleOrHigherAsync(
                approval.OrganizationId, 
                approverMembershipId, 
                requesterMembership.OrgRole);
            
            if (!canApprove)
            {
                return false; // Approver doesn't have sufficient role
            }

    approval.Status = "APPROVED";
         approval.ApprovedByMembershipId = approverMembershipId;
         approval.ApprovedBy = approverMembership.UserId; // Backward compatibility
     approval.Reason = reason;
     approval.ResolvedAt = DateTime.UtcNow;

    await _approvalRepository.UpdateAsync(approval);
            return true;
        }

     public async Task<bool> RejectAsync(Guid approvalId, Guid approverMembershipId, string reason)
 {
   var approval = await _approvalRepository.GetByIdAsync(approvalId);
            if (approval == null || approval.Status != "PENDING") return false;

            // Get approver membership
            var approverMembership = await _organizationUserRepository.GetByIdAsync(approverMembershipId);
            if (approverMembership == null || approverMembership.OrganizationId != approval.OrganizationId)
            {
                return false;
            }

     approval.Status = "REJECTED";
       approval.ApprovedByMembershipId = approverMembershipId;
       approval.ApprovedBy = approverMembership.UserId; // Backward compatibility
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

    /// <summary>
    /// Implementation of organization service.
    /// </summary>
    public class OrganizationServiceImpl : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationUserRepository _organizationUserRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<OrganizationServiceImpl> _logger;

        public OrganizationServiceImpl(
            IOrganizationRepository organizationRepository,
            IOrganizationUserRepository organizationUserRepository,
            IUserRepository userRepository,
            ILogger<OrganizationServiceImpl> logger)
        {
            _organizationRepository = organizationRepository;
            _organizationUserRepository = organizationUserRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<(Organization organization, OrganizationUser membership)> CreateOrganizationAsync(string name, Guid creatorUserId, string? industry = null)
        {
            // Check if organization name already exists
            var existing = await _organizationRepository.GetByNameAsync(name);
            if (existing != null)
            {
                throw new InvalidOperationException($"Organization with name '{name}' already exists");
            }

            // Verify creator user exists
            var creator = await _userRepository.GetByIdAsync(creatorUserId);
            if (creator == null)
            {
                throw new InvalidOperationException("Creator user not found");
            }

            // Create organization
            var organization = new Organization
            {
                Name = name,
                Industry = industry,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            organization = await _organizationRepository.CreateAsync(organization);

            // Create membership for creator as Owner
            var membership = new OrganizationUser
            {
                OrganizationId = organization.Id,
                UserId = creatorUserId,
                OrgRole = "Owner",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            membership = await _organizationUserRepository.CreateAsync(membership);

            _logger.LogInformation("Organization {OrgName} created with Owner {UserId}", name, creatorUserId);
            return (organization, membership);
        }

        public async Task<Organization?> GetOrganizationAsync(Guid organizationId)
        {
            return await _organizationRepository.GetByIdAsync(organizationId);
        }

        public async Task<OrganizationUser?> AddMemberAsync(Guid organizationId, Guid userId, string role)
        {
            // Verify organization exists
            var organization = await _organizationRepository.GetByIdAsync(organizationId);
            if (organization == null)
            {
                return null;
            }

            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            // Check if membership already exists
            var existing = await _organizationUserRepository.GetByOrganizationAndUserAsync(organizationId, userId);
            if (existing != null)
            {
                // Update existing membership
                existing.OrgRole = role;
                existing.Status = "Active";
                existing.UpdatedAt = DateTime.UtcNow;
                return await _organizationUserRepository.UpdateAsync(existing);
            }

            // Create new membership
            var membership = new OrganizationUser
            {
                OrganizationId = organizationId,
                UserId = userId,
                OrgRole = role,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _organizationUserRepository.CreateAsync(membership);
        }

        public async Task<OrganizationUser?> GetMembershipAsync(Guid organizationId, Guid userId)
        {
            return await _organizationUserRepository.GetByOrganizationAndUserAsync(organizationId, userId);
        }

        public async Task<List<OrganizationUser>> GetOrganizationMembersAsync(Guid organizationId)
        {
            return await _organizationUserRepository.GetByOrganizationIdAsync(organizationId);
        }

        public async Task<bool> UpdateMemberRoleAsync(Guid organizationId, Guid userId, string newRole)
        {
            var membership = await _organizationUserRepository.GetByOrganizationAndUserAsync(organizationId, userId);
            if (membership == null)
            {
                return false;
            }

            membership.OrgRole = newRole;
            membership.UpdatedAt = DateTime.UtcNow;
            await _organizationUserRepository.UpdateAsync(membership);
            return true;
        }

        public async Task<bool> RemoveMemberAsync(Guid organizationId, Guid userId)
        {
            var membership = await _organizationUserRepository.GetByOrganizationAndUserAsync(organizationId, userId);
            if (membership == null)
            {
                return false;
            }

            membership.Status = "Inactive";
            membership.UpdatedAt = DateTime.UtcNow;
            await _organizationUserRepository.UpdateAsync(membership);
            return true;
        }

        public async Task<List<Organization>> GetUserOrganizationsAsync(Guid userId)
        {
            var memberships = await _organizationUserRepository.GetByUserIdAsync(userId);
            var organizationIds = memberships.Select(m => m.OrganizationId).ToList();
            
            var organizations = new List<Organization>();
            foreach (var orgId in organizationIds)
            {
                var org = await _organizationRepository.GetByIdAsync(orgId);
                if (org != null)
                {
                    organizations.Add(org);
                }
            }
            return organizations;
        }

        public async Task<bool> HasRoleAsync(Guid organizationId, Guid userId, string role)
        {
            var membership = await _organizationUserRepository.GetByOrganizationAndUserAsync(organizationId, userId);
            return membership != null && membership.OrgRole == role && membership.Status == "Active";
        }

        public async Task<bool> HasRoleOrHigherAsync(Guid organizationId, Guid userId, string requiredRole)
        {
            var membership = await _organizationUserRepository.GetByOrganizationAndUserAsync(organizationId, userId);
            if (membership == null || membership.Status != "Active")
            {
                return false;
            }

            // Role hierarchy: Owner > Admin > Approver > Viewer > Auditor
            var roleHierarchy = new Dictionary<string, int>
            {
                { "Owner", 5 },
                { "Admin", 4 },
                { "Approver", 3 },
                { "Viewer", 2 },
                { "Auditor", 1 }
            };

            var userRoleLevel = roleHierarchy.GetValueOrDefault(membership.OrgRole, 0);
            var requiredRoleLevel = roleHierarchy.GetValueOrDefault(requiredRole, 0);

            return userRoleLevel >= requiredRoleLevel;
        }

        public async Task<List<OrganizationUser>> GetApproversAsync(Guid organizationId)
        {
            var approvers = new List<OrganizationUser>();
            
            // Get Owners
            var owners = await _organizationUserRepository.GetByOrganizationAndRoleAsync(organizationId, "Owner");
            approvers.AddRange(owners);

            // Get Admins
            var admins = await _organizationUserRepository.GetByOrganizationAndRoleAsync(organizationId, "Admin");
            approvers.AddRange(admins);

            // Get Approvers
            var approverRole = await _organizationUserRepository.GetByOrganizationAndRoleAsync(organizationId, "Approver");
            approvers.AddRange(approverRole);

            return approvers.Where(a => a.Status == "Active").ToList();
        }
    }
}
