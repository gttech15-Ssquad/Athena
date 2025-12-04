using BCrypt.Net;

namespace virtupay_corporate.Helpers
{
    /// <summary>
    /// Interface for password hashing operations.
    /// </summary>
  public interface IPasswordHasher
  {
        /// <summary>
        /// Hashes a password using BCrypt.
 /// </summary>
        string Hash(string password);

        /// <summary>
        /// Verifies a password against a hash.
  /// </summary>
 bool Verify(string password, string hash);
    }

    /// <summary>
    /// Implementation of password hasher using BCrypt.
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12; // BCrypt work factor

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
        }

      public bool Verify(string password, string hash)
        {
    try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
    }
 catch
        {
      return false;
     }
        }
    }
}
