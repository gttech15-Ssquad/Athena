using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using virtupay_corporate.DTOs;
using virtupay_corporate.Services;
using virtupay_corporate.Helpers;

namespace virtupay_corporate.Controllers
{
    /// <summary>
    /// Authentication and User Profile Management
    /// 
    /// Handles user registration, login, authentication, password management, and profile retrieval.
    /// Provides JWT token generation and management for secure API access. All authentication endpoints 
    /// are publicly accessible without prior authentication.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtTokenHelper _jwtTokenHelper;
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService, 
            IJwtTokenHelper jwtTokenHelper,
            IOrganizationService organizationService,
            ILogger<AuthController> logger)
        {
       _authService = authService;
            _jwtTokenHelper = jwtTokenHelper;
            _organizationService = organizationService;
     _logger = logger;
        }

        /// <summary>
        /// Register a new user account
        /// 
        /// Creates a new user account with email and password.
        /// Automatically generates a unique 10-digit account number and creates an AccountBalance record.
  /// Returns a JWT token upon successful registration for immediate API access.
        /// </summary>
        /// <param name="request">Registration details (email, password, optional name)</param>
        /// <returns>Authentication response with JWT token and user profile including account number</returns>
  [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
 [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
     public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
     try
   {
     if (!ModelState.IsValid)
        return BadRequest(new ErrorResponse
      {
       Code = "VALIDATION_ERROR",
        Message = "Validation failed",
    Details = ModelState.Values.SelectMany(v => v.Errors)
  });

    // Check if user already exists
        if (await _authService.UserExistsAsync(request.Email))
      {
              _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
      return BadRequest(new ErrorResponse
 {
        Code = "USER_EXISTS",
             Message = "User with this email already exists"
     });
      }

       var result = await _authService.RegisterAsync(
         request.Email, 
     request.Password, 
   request.Role,  // ? Use selected role (APP or VIEW)
         request.OrganizationName,
request.FirstName, 
    request.LastName,
         request.Industry);

          if (result == null)
    return BadRequest(new ErrorResponse
 {
     Code = "REGISTRATION_FAILED",
        Message = "Failed to register user"
     });

      var (user, organization, membership) = result.Value;

      // Generate token with organization context
          var token = _jwtTokenHelper.GenerateToken(user.Id, user.Email, membership.OrgRole, organization.Id, membership.Id);

       var response = new AuthResponse
        {
    Token = token,
         ExpiresAt = DateTime.UtcNow.AddMinutes(1440),
         OrganizationId = organization.Id,
         OrgRole = membership.OrgRole,
         MembershipId = membership.Id,
 User = new UserResponse
    {
     Id = user.Id,
      Email = user.Email,
   AccountNumber = user.AccountNumber,
   Role = membership.OrgRole, // Use org role instead of global role
         FirstName = user.FirstName,
        LastName = user.LastName,
         Status = user.Status,
        CreatedAt = user.CreatedAt
         }
        };

     _logger.LogInformation("User registered successfully: {Email}, AccountNumber: {AccountNumber}, Organization: {OrgName}", request.Email, user.AccountNumber, organization.Name);
          return CreatedAtAction(nameof(Register), response);
       }
   catch (Exception ex)
  {
            _logger.LogError(ex, "Error during registration");
     return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
         {
       Code = "INTERNAL_ERROR",
          Message = "An error occurred during registration"
      });
       }
    }

        /// <summary>
        /// Login and obtain JWT token
        /// 
        /// Authenticates a user with email and password, returning a JWT token for subsequent API calls.
        /// Token expires in 24 hours and must be included in the Authorization header for protected endpoints.
        /// 
        /// Include the returned token in all subsequent requests as: Authorization: Bearer {token}
   /// </summary>
        /// <param name="request">Login credentials (email and password)</param>
        /// <returns>Authentication response with JWT token valid for 24 hours and user profile</returns>
      [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
     [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
   public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
try
   {
    if (!ModelState.IsValid)
   return BadRequest(new ErrorResponse
             {
  Code = "VALIDATION_ERROR",
         Message = "Validation failed"
            });

          var loginResult = await _authService.LoginAsync(request.Email, request.Password);

      if (loginResult == null)
        {
   _logger.LogWarning("Login attempt failed for: {Email}", request.Email);
      return Unauthorized(new ErrorResponse
      {
Code = "INVALID_CREDENTIALS",
    Message = "Invalid email or password"
     });
           }

      var (token, organizationId, membershipId, orgRole) = loginResult.Value;
      var userIdString = _jwtTokenHelper.ExtractUserId(token)?.ToString();
      if (!Guid.TryParse(userIdString, out var userId))
      {
        return Unauthorized(new ErrorResponse
        {
          Code = "INVALID_USER_ID",
          Message = "Invalid user ID in token"
        });
      }

      var user = await _authService.GetUserAsync(userId);

      if (user == null)
      {
        return Unauthorized(new ErrorResponse
        {
          Code = "USER_NOT_FOUND",
          Message = "User not found"
        });
      }

       var response = new AuthResponse
      {
     Token = token,
           ExpiresAt = DateTime.UtcNow.AddMinutes(1440),
         OrganizationId = organizationId,
         OrgRole = orgRole,
         MembershipId = membershipId,
          User = new UserResponse
   {
 Id = user.Id,
      Email = user.Email,
   AccountNumber = user.AccountNumber,
   Role = orgRole ?? user.Role,
        FirstName = user.FirstName,
    LastName = user.LastName,
         Status = user.Status,
         CreatedAt = user.CreatedAt
   }
      };

    _logger.LogInformation("User logged in successfully: {Email}", request.Email);
        return Ok(response);
   }
    catch (Exception ex)
  {
    _logger.LogError(ex, "Error during login");
    return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
         {
  Code = "INTERNAL_ERROR",
       Message = "An error occurred during login"
  });
      }
   }

        /// <summary>
      /// Change user password
        /// 
        /// Updates the authenticated user's password by verifying the old password and setting a new one.
        /// The user must be logged in and provide both old and new passwords.
        /// This action is restricted to authenticated users only.
        /// </summary>
        /// <param name="request">Old password and new password</param>
     /// <returns>Success message if password was changed</returns>
        [HttpPost("change-password")]
        [Authorize]
 [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
 {
       try
       {
   if (!ModelState.IsValid)
   return BadRequest(new ErrorResponse
            {
       Code = "VALIDATION_ERROR",
     Message = "Validation failed"
         });

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
  return Unauthorized();

   var result = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);

 if (!result)
    return BadRequest(new ErrorResponse
       {
Code = "PASSWORD_CHANGE_FAILED",
      Message = "Failed to change password. Old password may be incorrect"
    });

        _logger.LogInformation("User {UserId} changed password", userId);
  return Ok(new { message = "Password changed successfully" });
  }
       catch (Exception ex)
  {
     _logger.LogError(ex, "Error changing password");
    return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
      {
   Code = "INTERNAL_ERROR",
 Message = "An error occurred while changing password"
    });
   }
    }

        /// <summary>
        /// Get current user profile
        /// 
        /// Retrieves the complete profile information of the currently authenticated user including
        /// email, role, name, status, and account creation date.
        /// Requires valid JWT token in Authorization header.
      /// </summary>
        /// <returns>User profile information</returns>
    [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
 [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
     try
    {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
  if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
      return Unauthorized();

 var user = await _authService.GetUserAsync(userId);

    if (user == null)
  return NotFound(new ErrorResponse
 {
   Code = "USER_NOT_FOUND",
  Message = "User not found"
      });

    var response = new UserResponse
    {
Id = user.Id,
     Email = user.Email,
   AccountNumber = user.AccountNumber,
  Role = user.Role,
 FirstName = user.FirstName,
            LastName = user.LastName,
     Status = user.Status,
 CreatedAt = user.CreatedAt
       };

 return Ok(response);
      }
  catch (Exception ex)
      {
   _logger.LogError(ex, "Error getting profile");
return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
      {
        Code = "INTERNAL_ERROR",
      Message = "An error occurred while fetching profile"
    });
  }
    }
    }
}
