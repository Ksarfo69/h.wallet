using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using H.Wallet.Api.Exceptions;
using H.Wallet.Api.Models;
using H.Wallet.Api.Repositories;
using H.Wallet.Api.Utils;
using Microsoft.IdentityModel.Tokens;

namespace H.Wallet.Api.Services;


public interface IHUserService
{
    Task<ApiResponse<string>> Register(HUserRegistration r);
    Task<ApiResponse<string>> Login(HUserLogin l);
    Task<ApiResponse<HUserResponseDto>> GetHUserDetails(string phoneNumber);
    Task<HUser> GetAuthenticatedUser(ClaimsPrincipal user);
    Task<HUser?> GetHUser(string phoneNumber);
}

public class HUserService : IHUserService
{
    public IConfiguration Configuration { get; set; }
    private readonly ILogger<HUserService> _logger;
    private readonly IHUserRepository _repository;
    private readonly IValidatorFactory _validatorFactory;
    
    public HUserService(
        IConfiguration configuration, 
        ILogger<HUserService> logger, 
        IHUserRepository repository,
        IValidatorFactory validatorFactory)
    {
        Configuration = configuration;
        _logger = logger;
        _repository = repository;
        _validatorFactory = validatorFactory;
    }


    public async Task<ApiResponse<string>> Register(HUserRegistration r)
    {
        _logger.LogInformation($"Registering new user: {r.PhoneNumber}");
        
        // check passwords match
        if (!string.Equals(r.Password, r.ConfirmPassword))
        {
            var msg = "Provided passwords do not match.";
            _logger.LogInformation($"Failed to register new user: {r.PhoneNumber}. Reason: {msg}");
            throw new BadRequestException(msg);
        }
        
        // check phone number valid
        var validator = _validatorFactory.GetPhoneNumberValidator();
        bool validNumber = validator.Validate(r.PhoneNumber);
        if (!validNumber)
        {
            var msg = $"The provided phone number: {r.PhoneNumber} is not valid.";
            _logger.LogInformation($"Failed to register new user: {r.PhoneNumber}. Reason: {msg}");
            throw new BadRequestException(msg);
        }
        
        // check user with same credentials does not exist
        HUser? exists = await GetHUser(r.PhoneNumber);
        if(exists != default)
        {
            var msg = $"User with phone number: {r.PhoneNumber} already exists.";
            _logger.LogInformation($"Failed to register new user: {r.PhoneNumber}. Reason: {msg}");
            throw new AlreadyExistsException(msg);
        }
        
        // create password hash and salt
        byte[] hash, salt;
        _logger.LogInformation("Generating password hash and salt.");
        HashingUtils.CreateHashAndSaltFor(r.Password, out hash, out salt);
        _logger.LogInformation("Generated password hash and salt successfully.");
        
        // create and add user to db
        HUser created = new HUser
        {
            Username = r.Username,
            PhoneNumber = r.PhoneNumber,
            PasswordSalt = salt,
            PasswordHash = hash
        };
        await _repository.Add(created);
        
        _logger.LogInformation($"Registered new user: {r.PhoneNumber} successfully.");
        
        return new ApiResponse<string>
        {
            Message = "User created successfully",
            Data = created.PhoneNumber
        };
    }

    public async Task<ApiResponse<string>> Login(HUserLogin l)
    {
        _logger.LogInformation($"Logging in user: {l.PhoneNumber}");
        
        // check user with provided credentials exists
        HUser? exists = await GetHUser(l.PhoneNumber);
        if (exists == default)
        {
            _logger.LogInformation($"Failed to log in user: {l.PhoneNumber}. Reason: User does not exist.");
            throw new UnauthorizedException("Invalid credentials.");
        }
        
        // check supplied password is valid for found user
        bool valid = HashingUtils.VerifyHashFor(l.Password, exists.PasswordHash, exists.PasswordSalt);
        if (!valid)
        {
            _logger.LogInformation($"Failed to log in user: {l.PhoneNumber}. Reason: Provided password incorrect.");
            throw new UnauthorizedException("Invalid credentials.");
        }
        
        // create JWT token
        _logger.LogInformation($"Generating JWT token for user: {exists.PhoneNumber}.");
        var token = CreateToken(exists.PhoneNumber);
        _logger.LogInformation($"Generated JWT token for user: {exists.PhoneNumber} successfully.");

        _logger.LogInformation($"Logged in user: {l.PhoneNumber} successfully.");
        
        return new ApiResponse<string>
        {
            Message = "User logged in successfully.",
            Data = token
        };
    }

    public async Task<ApiResponse<HUserResponseDto>> GetHUserDetails(string phoneNumber)
    {
        _logger.LogInformation($"Retrieving account details for user: {phoneNumber}");
        
        // check user with provided credentials exists
        HUser? exists = await GetHUser(phoneNumber);
        if (exists == default)
        {
            var msg = $"User with phone number: {phoneNumber} does not exist";
            _logger.LogInformation($"Failed to retrieve account details for user: {phoneNumber}. Reason: {msg}");
            throw new BadRequestException(msg);
        }

        HUserResponseDto responseDto = exists.ToResponseDto();
        
        _logger.LogInformation($"Retrieved account details for user: {phoneNumber} successfully.");
        
        return new ApiResponse<HUserResponseDto>
        {
            Message = "Retrieved user details successfully.",
            Data = responseDto
        };
    }
    
    public async Task<HUser?> GetHUser(string phoneNumber)
    {
        HUser? exists = await _repository.Get(u => string.Equals(u.PhoneNumber, phoneNumber));
        return exists;
    }
    
    public async Task<HUser> GetAuthenticatedUser(ClaimsPrincipal user)
    {
        _logger.LogInformation($"Retrieving authenticated user details.");
        
        var phoneNumber = user.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.MobilePhone));
        
        var authenticatedUser = await GetHUser(phoneNumber?.Value);
        
        if (authenticatedUser == default)
        {
            _logger.LogError($"Failed to retrieve authenticated user details. Reason: Account does not exist.");
            throw new UnauthorizedException("Invalid credentials");
        }
        
        _logger.LogInformation($"Retrieved authenticated user details successfully.");
        
        return authenticatedUser;
    }
    
    private string CreateToken(string phoneNumber)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.MobilePhone,phoneNumber)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("Jwt:Key").Value!));
        var duration = int.Parse(Configuration.GetSection("Jwt:Duration").Value!);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
        
        var now = DateTime.UtcNow;
        
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddMinutes(duration),
            signingCredentials: credentials
        );
        
        var tokenSerial = new JwtSecurityTokenHandler().WriteToken(token);
        
        return tokenSerial;
    }
}