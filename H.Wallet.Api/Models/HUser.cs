using System.ComponentModel.DataAnnotations;

namespace H.Wallet.Api.Models;

public class HUser : BaseModel
{
    [Required] 
    [StringLength(10, MinimumLength = 6)]
    public string Username { get; set; }
    
    [Required]
    [StringLength(20, MinimumLength = 6)]
    public string PhoneNumber { get; init; }
    
    [Required] 
    public byte[] PasswordHash { get; set; }
    
    [Required] 
    public byte[] PasswordSalt { get; set; }
    
    public HUserResponseDto ToResponseDto()
    {
        return new HUserResponseDto(Username, PhoneNumber, CreatedAt);
    }
}

public record HUserRegistration
{
    [Required]
    [StringLength(10, MinimumLength = 6, ErrorMessage = "Username must be between 6 and 10 characters long.")]
    public string Username { get; init; }
    
    [Required]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 50 characters long.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain only numbers.")]
    public string PhoneNumber { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 50 characters long.")]
    public string Password { get; init; }

    [Required]
    [Compare("Password", ErrorMessage = "Confirm password field does not match password.")]
    public string ConfirmPassword { get; init; }

    public HUserRegistration(string username, string phoneNumber, string password, string confirmPassword)
    {
        Username = username;
        Password = password;
        PhoneNumber = phoneNumber;
        ConfirmPassword = confirmPassword;
    }
}

public record HUserLogin
{
    [Required]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Phone number must be between 6 and 50 characters long.")]
    public string PhoneNumber { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 50 characters long.")]
    public string Password { get; init; }

    public HUserLogin(string phoneNumber, string password)
    {
        PhoneNumber = phoneNumber;
        Password = password;
    }
};

public record HUserResponseDto(
    string username,
    string phoneNumber,
    DateTime createdAt
);