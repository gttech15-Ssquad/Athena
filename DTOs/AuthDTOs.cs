using System.ComponentModel.DataAnnotations;

namespace virtupay_corporate.DTOs
{
    /// <summary>
    /// DTO for user registration request.
    /// </summary>
    public class RegisterRequest
    {
   /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
      [EmailAddress(ErrorMessage = "Invalid email format")]
      public required string Email { get; set; }

  /// <summary>
        /// Gets or sets the password.
        /// </summary>
     [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
     public required string Password { get; set; }

   /// <summary>
  /// Gets or sets the user role.
        /// </summary>
    [Required(ErrorMessage = "Role is required")]
        public required string Role { get; set; }

      /// <summary>
        /// Gets or sets the first name.
        /// </summary>
     public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
    /// </summary>
        public string? LastName { get; set; }

      /// <summary>
/// Gets or sets the department ID.
 /// </summary>
    public int? DepartmentId { get; set; }
    }

    /// <summary>
    /// DTO for user login request.
    /// </summary>
    public class LoginRequest
    {
      /// <summary>
     /// Gets or sets the email address.
    /// </summary>
        [Required(ErrorMessage = "Email is required")]
     public required string Email { get; set; }

    /// <summary>
      /// Gets or sets the password.
        /// </summary>
      [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
    }

    /// <summary>
    /// DTO for auth response containing JWT token.
    /// </summary>
    public class AuthResponse
    {
      /// <summary>
        /// Gets or sets the JWT token.
  /// </summary>
        public string? Token { get; set; }

       /// <summary>
        /// Gets or sets the token expiration time.
        /// </summary>
      public DateTime ExpiresAt { get; set; }

    /// <summary>
   /// Gets or sets the user information.
 /// </summary>
      public UserResponse? User { get; set; }
    }

    /// <summary>
    /// DTO for user response.
    /// </summary>
    public class UserResponse
{
       /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
       public int Id { get; set; }

   /// <summary>
        /// Gets or sets the email.
   /// </summary>
        public string? Email { get; set; }

  /// <summary>
        /// Gets or sets the role.
        /// </summary>
  public string? Role { get; set; }

      /// <summary>
        /// Gets or sets the first name.
        /// </summary>
     public string? FirstName { get; set; }

     /// <summary>
        /// Gets or sets the last name.
     /// </summary>
     public string? LastName { get; set; }

        /// <summary>
  /// Gets or sets the status.
 /// </summary>
        public string? Status { get; set; }

      /// <summary>
        /// Gets or sets the creation timestamp.
   /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for change password request.
    /// </summary>
    public class ChangePasswordRequest
    {
      /// <summary>
        /// Gets or sets the old password.
      /// </summary>
     [Required(ErrorMessage = "Old password is required")]
  public required string OldPassword { get; set; }

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
     [Required(ErrorMessage = "New password is required")]
   [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
      public required string NewPassword { get; set; }

  /// <summary>
        /// Gets or sets the password confirmation.
     /// </summary>
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
      public required string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// DTO for error response.
    /// </summary>
 public class ErrorResponse
    {
      /// <summary>
        /// Gets or sets the error code.
    /// </summary>
        public string? Code { get; set; }

  /// <summary>
        /// Gets or sets the error message.
        /// </summary>
   public string? Message { get; set; }

       /// <summary>
        /// Gets or sets additional details.
        /// </summary>
 public object? Details { get; set; }

       /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Generic paginated response DTO.
    /// </summary>
    /// <typeparam name="T">The type of items in the response.</typeparam>
    public class PaginatedResponse<T>
    {
       /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

      /// <summary>
      /// Gets or sets the total count.
        /// </summary>
   public int Total { get; set; }

      /// <summary>
        /// Gets or sets the page number.
       /// </summary>
    public int PageNumber { get; set; }

     /// <summary>
        /// Gets or sets the page size.
     /// </summary>
  public int PageSize { get; set; }

     /// <summary>
        /// Gets or sets the total pages.
 /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);

    /// <summary>
    /// Gets a value indicating whether there is a next page.
        /// </summary>
  public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Gets a value indicating whether there is a previous page.
/// </summary>
      public bool HasPreviousPage => PageNumber > 1;
    }
}
