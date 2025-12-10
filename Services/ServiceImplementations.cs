using virtupay_corporate.Helpers;
using virtupay_corporate.Models;
using virtupay_corporate.Repositories;

namespace virtupay_corporate.Services
{
    /// <summary>
    /// Implementation of authentication service.
    /// </summary>
    public class AuthService : IAuthService
    {
 private readonly IUserRepository _userRepository;
     private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenHelper _jwtTokenHelper;
        private readonly IAuditService _auditService;
      private readonly IHttpContextAccessor _httpContextAccessor;
 private readonly IRepository<AccountBalance> _accountBalanceRepository;
        private readonly IOrganizationService _organizationService;

  public AuthService(
     IUserRepository userRepository,
            IPasswordHasher passwordHasher,
   IJwtTokenHelper jwtTokenHelper,
   IAuditService auditService,
         IHttpContextAccessor httpContextAccessor,
     IRepository<AccountBalance> accountBalanceRepository,
            IOrganizationService organizationService)
        {
_userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenHelper = jwtTokenHelper;
            _auditService = auditService;
   _httpContextAccessor = httpContextAccessor;
_accountBalanceRepository = accountBalanceRepository;
            _organizationService = organizationService;
    }

        public async Task<(User user, Organization organization, OrganizationUser membership)?> RegisterAsync(string email, string password, string role, string organizationName, string? firstName = null, string? lastName = null, string? industry = null)
      {
      // Check if user already exists
            if (await _userRepository.GetByEmailAsync(email) != null)
    return null;

            // Validate role (must be APP or VIEW)
      if (!new[] { "APP", "VIEW" }.Contains(role))
     {
     role = "VIEW";  // Default to VIEW if invalid role
}

      // Generate unique 10-digit account number
       string accountNumber = await GenerateUniqueAccountNumber();

   var user = new User
   {
  Email = email,
        PasswordHash = _passwordHasher.Hash(password),
 Role = role,  // APP (Approver) or VIEW (Viewer)
 FirstName = firstName,
    LastName = lastName,
    Status = "Active",
    AccountNumber = accountNumber
      };

  await _userRepository.CreateAsync(user);

  // Create organization with user as Owner
        var (organization, membership) = await _organizationService.CreateOrganizationAsync(organizationName, user.Id, industry);

  // Create AccountBalance for the organization
     var accountBalance = new AccountBalance
  {
         OrganizationId = organization.Id,
         UserId = user.Id, // Optional: user-specific balance
         OrganizationUserId = membership.Id,
       AvailableBalance = 0,
  TotalFunded = 0,
TotalWithdrawn = 0,
    Currency = "NGN",
    CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
     };

    await _accountBalanceRepository.CreateAsync(accountBalance);

  var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
   await _auditService.LogActionAsync(user.Id, "USER_REGISTERED", "User", user.Id, $"Role: {role}, Organization: {organizationName}", ipAddress, null);

   return (user, organization, membership);
      }

 private async Task<string> GenerateUniqueAccountNumber()
        {
      // Generate random 10-digit account number (1000000000 to 9999999999)
 var random = new Random();
    string accountNumber;

            do
         {
      // Generate a number between 1000000000 and 9999999999
    long randomLong = random.NextInt64(1000000000, 10000000000);
   accountNumber = randomLong.ToString();
  } while (await _userRepository.GetByAccountNumberAsync(accountNumber) != null);

  return accountNumber;
        }

        public async Task<(string token, Guid? organizationId, Guid? membershipId, string? orgRole)?> LoginAsync(string email, string password)
      {
            var user = await _userRepository.GetByEmailAsync(email);

          if (user == null || !_passwordHasher.Verify(password, user.PasswordHash))
    return null;

     if (user.Status != "Active")
            return null;

            // Get user's first active organization membership
            var organizations = await _organizationService.GetUserOrganizationsAsync(user.Id);
            var firstOrg = organizations.FirstOrDefault();
            OrganizationUser? membership = null;
            
            if (firstOrg != null)
            {
                membership = await _organizationService.GetMembershipAsync(firstOrg.Id, user.Id);
            }

            Guid? orgId = membership?.OrganizationId;
            Guid? membershipId = membership?.Id;
            string? orgRole = membership?.OrgRole ?? user.Role;

       var token = _jwtTokenHelper.GenerateToken(user.Id, user.Email, orgRole, orgId, membershipId);

            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
      await _auditService.LogActionAsync(user.Id, "USER_LOGIN", "User", user.Id, $"Organization: {orgId}", ipAddress, null);

 return (token, orgId, membershipId, orgRole);
   }

        public async Task<bool> UserExistsAsync(string email)
        {
   return await _userRepository.GetByEmailAsync(email) != null;
        }

        public async Task<User?> GetUserAsync(int userId)
        {
        return await _userRepository.GetByIdAsync(userId);
    }

  public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
        var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !_passwordHasher.Verify(oldPassword, user.PasswordHash))
return false;

    user.PasswordHash = _passwordHasher.Hash(newPassword);
         await _userRepository.UpdateAsync(user);

   var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
     await _auditService.LogActionAsync(user.Id, "PASSWORD_CHANGED", "User", user.Id, null, ipAddress, null);

    return true;
        }

   public async Task<bool> SuspendUserAsync(int userId)
        {
var user = await _userRepository.GetByIdAsync(userId);
    if (user == null) return false;

 user.Status = "Suspended";
          await _userRepository.UpdateAsync(user);

   var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
  await _auditService.LogActionAsync(user.Id, "USER_SUSPENDED", "User", user.Id, null, ipAddress, null);

            return true;
  }

        public async Task<bool> ReactivateUserAsync(int userId)
        {
  var user = await _userRepository.GetByIdAsync(userId);
       if (user == null) return false;

            user.Status = "Active";
    await _userRepository.UpdateAsync(user);

    var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
     await _auditService.LogActionAsync(user.Id, "USER_REACTIVATED", "User", user.Id, null, ipAddress, null);

    return true;
    }
    }

    /// <summary>
 /// Implementation of audit service.
  /// </summary>
    public class AuditService : IAuditService
  {
    private readonly IAuditLogRepository _auditLogRepository;

        public AuditService(IAuditLogRepository auditLogRepository)
     {
_auditLogRepository = auditLogRepository;
        }

        public async Task LogActionAsync(Guid? userId, string action, string resource, Guid? resourceId, string? changes, string? ipAddress, string? userAgent, string status = "SUCCESS", string? errorMessage = null)
 {
     var auditLog = new AuditLog
            {
     UserId = userId,
         Action = action,
      Resource = resource,
        ResourceId = resourceId,
      Changes = changes,
         IpAddress = ipAddress,
  UserAgent = userAgent,
         Status = status,
     ErrorMessage = errorMessage
          };

         await _auditLogRepository.CreateAsync(auditLog);
        }

        public async Task<(List<AuditLog> items, int total)> GetAuditLogsAsync(int pageNumber, int pageSize, string? action = null, string? resource = null, DateTime? startDate = null, DateTime? endDate = null)
  {
 return await _auditLogRepository.GetPaginatedAsync(pageNumber, pageSize, action, resource, startDate, endDate);
        }

        public async Task<List<AuditLog>> GetUserAuditLogsAsync(Guid userId)
        {
   return await _auditLogRepository.GetByUserIdAsync(userId);
   }

    public async Task<string> ExportAuditLogsAsync(string format, DateTime? startDate = null, DateTime? endDate = null)
    {
        // TODO: Implement export logic (CSV, PDF)
    await Task.CompletedTask;
  throw new NotImplementedException();
        }
    }
}
