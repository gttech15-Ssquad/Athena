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

  public AuthService(
 IUserRepository userRepository,
     IPasswordHasher passwordHasher,
     IJwtTokenHelper jwtTokenHelper,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor)
   {
     _userRepository = userRepository;
            _passwordHasher = passwordHasher;
_jwtTokenHelper = jwtTokenHelper;
     _auditService = auditService;
       _httpContextAccessor = httpContextAccessor;
  }

        public async Task<User?> RegisterAsync(string email, string password, string role, string? firstName = null, string? lastName = null, int? departmentId = null)
        {
     // Check if user already exists
        if (await _userRepository.GetByEmailAsync(email) != null)
            return null;

        var user = new User
 {
              Email = email,
                PasswordHash = _passwordHasher.Hash(password),
     Role = role,
             FirstName = firstName,
           LastName = lastName,
  DepartmentId = departmentId,
        Status = "Active"
  };

   await _userRepository.CreateAsync(user);

            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
   await _auditService.LogActionAsync(user.Id, "USER_REGISTERED", "User", user.Id, null, ipAddress, null);

  return user;
        }

 public async Task<string?> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || !_passwordHasher.Verify(password, user.PasswordHash))
        return null;

            if (user.Status != "Active")
        return null;

            var token = _jwtTokenHelper.GenerateToken(user.Id, user.Email, user.Role);

     var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    await _auditService.LogActionAsync(user.Id, "USER_LOGIN", "User", user.Id, null, ipAddress, null);

   return token;
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
            await _auditService.LogActionAsync(userId, "PASSWORD_CHANGED", "User", userId, null, ipAddress, null);

            return true;
        }

        public async Task<bool> SuspendUserAsync(int userId)
   {
      var user = await _userRepository.GetByIdAsync(userId);
   if (user == null) return false;

          user.Status = "Suspended";
            await _userRepository.UpdateAsync(user);

            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            await _auditService.LogActionAsync(userId, "USER_SUSPENDED", "User", userId, null, ipAddress, null);

   return true;
        }

        public async Task<bool> ReactivateUserAsync(int userId)
        {
 var user = await _userRepository.GetByIdAsync(userId);
 if (user == null) return false;

          user.Status = "Active";
   await _userRepository.UpdateAsync(user);

       var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            await _auditService.LogActionAsync(userId, "USER_REACTIVATED", "User", userId, null, ipAddress, null);

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

        public async Task LogActionAsync(int? userId, string action, string resource, int? resourceId, string? changes, string? ipAddress, string? userAgent, string status = "SUCCESS", string? errorMessage = null)
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

     public async Task<List<AuditLog>> GetUserAuditLogsAsync(int userId)
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
