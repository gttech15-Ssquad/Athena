using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace virtupay_corporate.Helpers
{
    /// <summary>
    /// Interface for JWT token operations.
    /// </summary>
    public interface IJwtTokenHelper
    {
        /// <summary>
        /// Generates a JWT token for a user.
        /// </summary>
        string GenerateToken(int userId, string email, string role);

        /// <summary>
        /// Validates and retrieves claims from a token.
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);

      /// <summary>
        /// Extracts the user ID from a token.
        /// </summary>
   int? ExtractUserId(string token);

   /// <summary>
        /// Extracts the role from a token.
        /// </summary>
        string? ExtractRole(string token);
    }

    /// <summary>
    /// Implementation of JWT token helper.
    /// </summary>
    public class JwtTokenHelper : IJwtTokenHelper
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
      private readonly int _expirationMinutes = 1440; // 24 hours

        public JwtTokenHelper(string secret, string issuer, string audience)
        {
       _secret = secret;
      _issuer = issuer;
     _audience = audience;
        }

        public string GenerateToken(int userId, string email, string role)
        {
            var key = Encoding.ASCII.GetBytes(_secret);
         var tokenDescriptor = new SecurityTokenDescriptor
    {
  Subject = new ClaimsIdentity(new[]
       {
             new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Email, email),
  new Claim(ClaimTypes.Role, role)
    }),
      Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
   Issuer = _issuer,
           Audience = _audience,
     SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
  };

      var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
      }

   public ClaimsPrincipal? ValidateToken(string token)
        {
         try
            {
                var key = Encoding.ASCII.GetBytes(_secret);
      var tokenHandler = new JwtSecurityTokenHandler();

     var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
  {
       ValidateIssuerSigningKey = true,
  IssuerSigningKey = new SymmetricSecurityKey(key),
 ValidateIssuer = true,
    ValidIssuer = _issuer,
   ValidateAudience = true,
    ValidAudience = _audience,
            ValidateLifetime = true,
ClockSkew = TimeSpan.Zero
     }, out SecurityToken validatedToken);

                return principal;
      }
    catch
{
            return null;
            }
        }

        public int? ExtractUserId(string token)
        {
      var principal = ValidateToken(token);
      if (principal == null) return null;

       var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
         return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
 }

        public string? ExtractRole(string token)
  {
   var principal = ValidateToken(token);
    return principal?.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
