using System.Linq.Expressions;
using H.Wallet.Api.Exceptions;
using H.Wallet.Api.Models;
using H.Wallet.Api.Repositories;
using H.Wallet.Api.Services;
using H.Wallet.Api.Utils;
using Moq;

namespace H.Wallet.Api.Tests.UnitTests.Services;

using Xunit;

public class HUserServiceTests
{ 
    private readonly Mock<IConfiguration> _configurationMock = new Mock<IConfiguration>();
    private readonly Mock<IHUserRepository> _repositoryMock = new Mock<IHUserRepository>();
    private readonly Mock<IValidatorFactory> _validatorFactoryMock = new Mock<IValidatorFactory>();
    private readonly HUserService _service;

    public HUserServiceTests()
    {
        _service = new HUserService(
            _configurationMock.Object,
            new LoggerFactory().CreateLogger<HUserService>(),
            _repositoryMock.Object,
            _validatorFactoryMock.Object
            );
    }
    
    [Fact]
    public async Task Register_PasswordsDoNotMatch_ThrowsBadRequestException()
    {
        var registration = new HUserRegistration("testuser", "233245776479","Password123",  "Password1234");

        await Assert.ThrowsAsync<BadRequestException>(() => _service.Register(registration));
    }
    
    [Fact]
    public async Task Register_PhoneNumberNotValid_ThrowsBadRequestException()
    {
        var registration = new HUserRegistration("testuser", "233245776479", "Password123", "Password123");
        MockPhoneNumberValidatorToReturn(false, registration.PhoneNumber);

        await Assert.ThrowsAsync<BadRequestException>(() => _service.Register(registration));
    }

    [Fact]
    public async Task Register_PhoneNumberExists_ThrowsAlreadyExistsException()
    {
        var registration = new HUserRegistration("testuser", "233245776479", "Password123", "Password123");
        MockPhoneNumberValidatorToReturn(true, registration.PhoneNumber);
        MockRepositoryToReturn(new HUser());

        await Assert.ThrowsAsync<AlreadyExistsException>(() => _service.Register(registration));
    }

    [Fact]
    public async Task Register_ValidRegistration_ReturnsSuccess()
    {
        var registration = new HUserRegistration("testuser", "233245776479","Password123", "Password123");
        MockPhoneNumberValidatorToReturn(true, registration.PhoneNumber);
        MockRepositoryToReturn((HUser) null);

        var result = await _service.Register(registration);
        
        Assert.True(result.Success);
    }

    [Fact]
    public async Task Login_UserDoesNotExist_ThrowsUnauthorizedException()
    {
        var login = new HUserLogin ( phoneNumber: "233245776479", password : "Password123" );
        MockRepositoryToReturn((HUser) null);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.Login(login));
    }

    [Fact]
    public async Task Login_InvalidPassword_ThrowsUnauthorizedException()
    {
        var password = "Password123";
        var registration = new HUserRegistration("testuser", "233245776479", password,  password);
        HashingUtils.CreateHashAndSaltFor(password, out byte[] hash, out byte[] salt);
        var user = new HUser
        {
            Username = "testuser",
            PhoneNumber = "233245776479",
            PasswordHash = hash, 
            PasswordSalt = salt
        };
        MockRepositoryToReturn(user);
        
        var login = new HUserLogin(phoneNumber: registration.PhoneNumber, password : "WrongPassword" );

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.Login(login));
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccess()
    {
        var password = "Password123";
        HashingUtils.CreateHashAndSaltFor(password, out byte[] hash, out byte[] salt);
        var user = new HUser
        {
            Username = "testuser",
            PhoneNumber = "233245776479",
            PasswordHash = hash, 
            PasswordSalt = salt
        };
        var login = new HUserLogin (phoneNumber: user.PhoneNumber, password : password );
        MockRepositoryToReturn(user);
        var jwtKey = "test_secret_jwt_key_goes_here_must_be_32_bytes_or_more";
        var jwtKeyMock = new Mock<IConfigurationSection>();
        jwtKeyMock.Setup(x => x.Value).Returns(jwtKey);
        var jwtDuration = "1440";
        var jwtDurationMock = new Mock<IConfigurationSection>();
        jwtDurationMock.Setup(x => x.Value).Returns(jwtDuration);
        _configurationMock.Setup(x => x.GetSection("Jwt:Key")).Returns(jwtKeyMock.Object);
        _configurationMock.Setup(x => x.GetSection("Jwt:Duration")).Returns(jwtDurationMock.Object);


        var result = await _service.Login(login);
        
        Assert.True(result.Success);
    }

    private void MockPhoneNumberValidatorToReturn(bool valid, string phoneNumber)
    {
        var phoneValidatorMock = new Mock<IPhoneNumberValidator>();
        phoneValidatorMock.Setup(x => x.Validate(phoneNumber)).Returns(valid);

        _validatorFactoryMock.Setup(vf => vf.GetPhoneNumberValidator()).Returns(phoneValidatorMock.Object);
    }

    private void MockRepositoryToReturn(HUser? value)
    {
        _repositoryMock.Setup(r => r.Get(It.IsAny<Expression<Func<HUser, bool>>>()))
            .ReturnsAsync(value);
    }
}
