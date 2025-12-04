using Microsoft.EntityFrameworkCore;
using virtupay_corporate.Data;
using virtupay_corporate.Models;

namespace virtupay_corporate.Repositories
{
    /// <summary>
    /// Implementation of user repository.
    /// </summary>
public class UserRepository : IUserRepository
    {
private readonly CorporateDbContext _context;

    public UserRepository(CorporateDbContext context)
        {
 _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
 {
 return await _context.Users
       .Where(u => !u.IsDeleted)
     .FirstOrDefaultAsync(u => u.Id == id);
        }

 public async Task<User?> GetByEmailAsync(string email)
        {
 return await _context.Users
       .Where(u => !u.IsDeleted && u.Email == email)
    .FirstOrDefaultAsync();
    }

        public async Task<List<User>> GetAllAsync(int? departmentId = null, string? role = null)
        {
  var query = _context.Users
.Where(u => !u.IsDeleted)
    .Include(u => u.Department)
   .AsQueryable();

            if (departmentId.HasValue)
     query = query.Where(u => u.DepartmentId == departmentId);

  if (!string.IsNullOrEmpty(role))
 query = query.Where(u => u.Role == role);

            return await query.ToListAsync();
        }

  public async Task<User> CreateAsync(User user)
{
       _context.Users.Add(user);
            await _context.SaveChangesAsync();
       return user;
        }

  public async Task<User> UpdateAsync(User user)
        {
   _context.Users.Update(user);
       await _context.SaveChangesAsync();
     return user;
      }

  public async Task<bool> DeleteAsync(int id)
        {
      var user = await GetByIdAsync(id);
  if (user == null) return false;

  user.IsDeleted = true;
 await _context.SaveChangesAsync();
 return true;
  }

   public async Task<(List<User> items, int total)> GetPaginatedAsync(int pageNumber, int pageSize)
        {
  var query = _context.Users.Where(u => !u.IsDeleted);
      var total = await query.CountAsync();

      var items = await query
      .Skip((pageNumber - 1) * pageSize)
   .Take(pageSize)
      .ToListAsync();

 return (items, total);
        }
    }

    /// <summary>
    /// Implementation of department repository.
    /// </summary>
  public class DepartmentRepository : IDepartmentRepository
    {
     private readonly CorporateDbContext _context;

   public DepartmentRepository(CorporateDbContext context)
   {
          _context = context;
        }

        public async Task<Department?> GetByIdAsync(int id)
 {
     return await _context.Departments
    .Where(d => !d.IsDeleted)
         .FirstOrDefaultAsync(d => d.Id == id);
    }

        public async Task<List<Department>> GetAllAsync()
    {
      return await _context.Departments
    .Where(d => !d.IsDeleted)
    .Include(d => d.Users)
  .ToListAsync();
       }

        public async Task<Department> CreateAsync(Department department)
    {
        _context.Departments.Add(department);
     await _context.SaveChangesAsync();
          return department;
  }

        public async Task<Department> UpdateAsync(Department department)
  {
    _context.Departments.Update(department);
      await _context.SaveChangesAsync();
     return department;
  }

 public async Task<bool> DeleteAsync(int id)
   {
       var department = await GetByIdAsync(id);
    if (department == null) return false;

    department.IsDeleted = true;
      await _context.SaveChangesAsync();
         return true;
 }

        public async Task<(List<Department> items, int total)> GetPaginatedAsync(int pageNumber, int pageSize)
      {
            var query = _context.Departments.Where(d => !d.IsDeleted);
   var total = await query.CountAsync();

   var items = await query
               .Skip((pageNumber - 1) * pageSize)
 .Take(pageSize)
            .ToListAsync();

  return (items, total);
   }
    }

    /// <summary>
   /// Implementation of card repository.
    /// </summary>
    public class CardRepository : ICardRepository
   {
      private readonly CorporateDbContext _context;

  public CardRepository(CorporateDbContext context)
     {
            _context = context;
  }

     public async Task<VirtualCard?> GetByIdAsync(int id)
  {
return await _context.VirtualCards
       .Where(c => !c.IsDeleted)
      .Include(c => c.CardLimits)
        .Include(c => c.MerchantRestrictions)
 .Include(c => c.CardBalance)
         .FirstOrDefaultAsync(c => c.Id == id);
      }

        public async Task<VirtualCard?> GetByCardNumberAsync(string cardNumber)
   {
       return await _context.VirtualCards
   .Where(c => !c.IsDeleted && c.CardNumber == cardNumber)
  .FirstOrDefaultAsync();
       }

  public async Task<List<VirtualCard>> GetByUserIdAsync(int userId)
   {
 return await _context.VirtualCards
       .Where(c => !c.IsDeleted && c.UserId == userId)
         .Include(c => c.CardBalance)
         .ToListAsync();
        }

  public async Task<List<VirtualCard>> GetAllAsync()
      {
 return await _context.VirtualCards
      .Where(c => !c.IsDeleted)
     .Include(c => c.CardBalance)
       .ToListAsync();
      }

        public async Task<VirtualCard> CreateAsync(VirtualCard card)
      {
 _context.VirtualCards.Add(card);
  await _context.SaveChangesAsync();
      return card;
    }

        public async Task<VirtualCard> UpdateAsync(VirtualCard card)
        {
      _context.VirtualCards.Update(card);
    await _context.SaveChangesAsync();
         return card;
  }

   public async Task<bool> DeleteAsync(int id)
        {
 var card = await GetByIdAsync(id);
  if (card == null) return false;

    card.IsDeleted = true;
        await _context.SaveChangesAsync();
 return true;
    }

    public async Task<(List<VirtualCard> items, int total)> GetPaginatedByUserIdAsync(int userId, int pageNumber, int pageSize)
    {
         var query = _context.VirtualCards
           .Where(c => !c.IsDeleted && c.UserId == userId);
   var total = await query.CountAsync();

     var items = await query
     .Skip((pageNumber - 1) * pageSize)
     .Take(pageSize)
    .Include(c => c.CardBalance)
           .ToListAsync();

   return (items, total);
        }
    }

/// <summary>
    /// Implementation of transaction repository.
    /// </summary>
   public class TransactionRepository : ITransactionRepository
   {
   private readonly CorporateDbContext _context;

        public TransactionRepository(CorporateDbContext context)
    {
       _context = context;
     }

        public async Task<CardTransaction?> GetByIdAsync(int id)
     {
  return await _context.CardTransactions.FirstOrDefaultAsync(t => t.Id == id);
    }

        public async Task<List<CardTransaction>> GetByCardIdAsync(int cardId)
   {
   return await _context.CardTransactions
        .Where(t => t.CardId == cardId)
     .OrderByDescending(t => t.CreatedAt)
  .ToListAsync();
    }

        public async Task<(List<CardTransaction> items, int total)> GetPaginatedByCardIdAsync(int cardId, int pageNumber, int pageSize)
    {
        var query = _context.CardTransactions.Where(t => t.CardId == cardId);
         var total = await query.CountAsync();

  var items = await query
    .OrderByDescending(t => t.CreatedAt)
         .Skip((pageNumber - 1) * pageSize)
         .Take(pageSize)
 .ToListAsync();

    return (items, total);
    }

      public async Task<CardTransaction> CreateAsync(CardTransaction transaction)
    {
        _context.CardTransactions.Add(transaction);
   await _context.SaveChangesAsync();
        return transaction;
    }

      public async Task<CardTransaction> UpdateAsync(CardTransaction transaction)
    {
        _context.CardTransactions.Update(transaction);
       await _context.SaveChangesAsync();
 return transaction;
    }

    public async Task<List<CardTransaction>> GetByDateRangeAsync(int cardId, DateTime startDate, DateTime endDate)
   {
    return await _context.CardTransactions
           .Where(t => t.CardId == cardId && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
      .OrderByDescending(t => t.CreatedAt)
     .ToListAsync();
   }
    }

    /// <summary>
    /// Implementation of approval repository.
    /// </summary>
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly CorporateDbContext _context;

   public ApprovalRepository(CorporateDbContext context)
   {
       _context = context;
   }

   public async Task<CardApproval?> GetByIdAsync(int id)
      {
  return await _context.CardApprovals
        .Include(a => a.RequestedByUser)
  .Include(a => a.ApprovedByUser)
     .FirstOrDefaultAsync(a => a.Id == id);
    }

        public async Task<List<CardApproval>> GetPendingAsync()
     {
     return await _context.CardApprovals
      .Where(a => a.Status == "PENDING" && a.ExpiresAt > DateTime.UtcNow)
   .Include(a => a.RequestedByUser)
           .ToListAsync();
      }

  public async Task<List<CardApproval>> GetByCardIdAsync(int cardId)
        {
     return await _context.CardApprovals
          .Where(a => a.CardId == cardId)
   .OrderByDescending(a => a.CreatedAt)
      .ToListAsync();
   }

    public async Task<(List<CardApproval> items, int total)> GetPaginatedAsync(string? status = null, int pageNumber = 1, int pageSize = 20)
       {
      var query = _context.CardApprovals.AsQueryable();

       if (!string.IsNullOrEmpty(status))
    query = query.Where(a => a.Status == status);

     var total = await query.CountAsync();

        var items = await query
   .OrderByDescending(a => a.CreatedAt)
             .Skip((pageNumber - 1) * pageSize)
  .Take(pageSize)
       .Include(a => a.RequestedByUser)
             .Include(a => a.ApprovedByUser)
  .ToListAsync();

   return (items, total);
   }

   public async Task<CardApproval> CreateAsync(CardApproval approval)
        {
   _context.CardApprovals.Add(approval);
    await _context.SaveChangesAsync();
       return approval;
 }

  public async Task<CardApproval> UpdateAsync(CardApproval approval)
    {
  _context.CardApprovals.Update(approval);
      await _context.SaveChangesAsync();
 return approval;
   }
    }

  /// <summary>
    /// Implementation of audit log repository.
    /// </summary>
  public class AuditLogRepository : IAuditLogRepository
  {
     private readonly CorporateDbContext _context;

      public AuditLogRepository(CorporateDbContext context)
        {
    _context = context;
      }

   public async Task<AuditLog?> GetByIdAsync(int id)
       {
   return await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == id);
     }

   public async Task<List<AuditLog>> GetByResourceAsync(string resource, int? resourceId = null)
     {
   var query = _context.AuditLogs.Where(a => a.Resource == resource).AsQueryable();

         if (resourceId.HasValue)
     query = query.Where(a => a.ResourceId == resourceId);

    return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
    }

 public async Task<(List<AuditLog> items, int total)> GetPaginatedAsync(
            int pageNumber = 1,
      int pageSize = 20,
    string? action = null,
           string? resource = null,
           DateTime? startDate = null,
           DateTime? endDate = null)
       {
          var query = _context.AuditLogs.AsQueryable();

     if (!string.IsNullOrEmpty(action))
          query = query.Where(a => a.Action == action);

      if (!string.IsNullOrEmpty(resource))
         query = query.Where(a => a.Resource == resource);

      if (startDate.HasValue)
     query = query.Where(a => a.Timestamp >= startDate);

  if (endDate.HasValue)
         query = query.Where(a => a.Timestamp <= endDate);

      var total = await query.CountAsync();

     var items = await query
 .OrderByDescending(a => a.Timestamp)
         .Skip((pageNumber - 1) * pageSize)
         .Take(pageSize)
     .Include(a => a.User)
           .ToListAsync();

       return (items, total);
      }

   public async Task<AuditLog> CreateAsync(AuditLog auditLog)
       {
    _context.AuditLogs.Add(auditLog);
      await _context.SaveChangesAsync();
         return auditLog;
       }

   public async Task<List<AuditLog>> GetByUserIdAsync(int userId)
    {
   return await _context.AuditLogs
      .Where(a => a.UserId == userId)
        .OrderByDescending(a => a.Timestamp)
          .ToListAsync();
   }
    }
}
