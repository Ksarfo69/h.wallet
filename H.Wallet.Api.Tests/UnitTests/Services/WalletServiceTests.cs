using System.Linq.Expressions;
using H.Wallet.Api.Enums;
using H.Wallet.Api.Exceptions;
using H.Wallet.Api.Models;
using H.Wallet.Api.Repositories;
using H.Wallet.Api.Services;
using H.Wallet.Api.Utils;
using Moq;
using Xunit;

namespace H.Wallet.Api.Tests.UnitTests.Services;

public class WalletServiceTests
{
    private readonly Mock<IWalletRepository> _repositoryMock = new Mock<IWalletRepository>();
    private readonly Mock<IValidatorFactory> _validatorFactoryMock = new Mock<IValidatorFactory>();
    private readonly WalletService _service;
    private readonly HUser _authenticatedHUser = new HUser { Username = "testuser" };

    public WalletServiceTests()
    {
        _service = new WalletService(
            new LoggerFactory().CreateLogger<WalletService>(),
            _repositoryMock.Object,
            _validatorFactoryMock.Object);
    }

    public List<Models.Wallet> CreateWallets(int count)
    {
        var wallets = new List<Models.Wallet>();
        for (int i = 0; i < count; i++)
        {
            var PAN = $"5123{i}56789012345";
            wallets.Add(
                new Models.Wallet { 
                    Owner = _authenticatedHUser, 
                    Scheme = WalletScheme.Mastercard, 
                    Type = WalletType.Card, 
                    Number = PAN.Substring(0, 6), 
                    Name = "My Wallet"
                }
            );
            
        }
        _authenticatedHUser.Wallets.Clear();
        _authenticatedHUser.Wallets.AddRange(wallets);

        return wallets;
    }

    [Fact]
    public async Task NewWallet_InvalidSchemeNumber_ThrowsBadRequestException()
    {
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Visa, PAN: "1234567890123456" );
        MockWalletSchemeValidatorToReturn(false, walletRegistration);
        _authenticatedHUser.Wallets.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() => _service.NewWallet(_authenticatedHUser, walletRegistration));
    }

    [Fact]
    public async Task NewWallet_MaxWalletsReached_ThrowsForbiddenException()
    {
        CreateWallets(5);
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Visa, PAN: "4123456789012345" );
        MockWalletSchemeValidatorToReturn(true, walletRegistration);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.NewWallet(_authenticatedHUser, walletRegistration));
    }

    [Fact]
    public async Task NewWallet_CardWalletAlreadyExists_ThrowsAlreadyExistsException()
    {
        var PAN = "5123456789012345";
        var wallet = new Models.Wallet
        {
            Owner = _authenticatedHUser,
            Scheme = WalletScheme.Mastercard,
            Type = WalletType.Card,
            Number = PAN.Substring(0, 6),
            Name = "My Wallet"
        };
        _authenticatedHUser.Wallets.Clear();
        _authenticatedHUser.Wallets.Add(wallet);
        var walletRegistration =
            new WalletRegistration(name: "Test Models.Wallet", scheme: wallet.Scheme, PAN: PAN);
        
        MockWalletSchemeValidatorToReturn(true, walletRegistration);

        await Assert.ThrowsAsync<AlreadyExistsException>(() => _service.NewWallet(_authenticatedHUser, walletRegistration));
    }
    
    [Fact]
    public async Task NewWallet_MomoWalletAlreadyExists_ThrowsAlreadyExistsException()
    {
        var PAN = "23356774356";
        var wallet = new Models.Wallet
        {
            Owner = _authenticatedHUser,
            Scheme = WalletScheme.AirtelTigo,
            Type = WalletType.Momo,
            Number = PAN,
            Name = "My Wallet"
        };
        _authenticatedHUser.Wallets.Clear();
        _authenticatedHUser.Wallets.Add(wallet);
        var walletRegistration =
            new WalletRegistration(name: "Test Models.Wallet", scheme: wallet.Scheme, PAN: PAN);
        
        MockWalletSchemeValidatorToReturn(true, walletRegistration);

        await Assert.ThrowsAsync<AlreadyExistsException>(() => _service.NewWallet(_authenticatedHUser, walletRegistration));
    }

    [Fact]
    public async Task NewWallet_CardSuccess_ReturnsSuccessMessage()
    {
        CreateWallets(4);
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Visa, PAN: "4123456789012345" );
        MockWalletSchemeValidatorToReturn(true, walletRegistration);

        var response = await _service.NewWallet(_authenticatedHUser, walletRegistration);
        
        Assert.True(response.Success);
    }
    
    [Fact]
    public async Task NewWallet_MomoSuccess_ReturnsSuccessMessage()
    {
        CreateWallets(4);
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Mtn, PAN: "233249884356" );
        MockWalletSchemeValidatorToReturn(true, walletRegistration);

        var response = await _service.NewWallet(_authenticatedHUser, walletRegistration);
        
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetWalletById_NotFound_ThrowsNotFoundException()
    {
        Guid walletId = Guid.NewGuid();
        MockRepositoryToReturn((Models.Wallet) null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetWalletById(walletId));
    }

    [Fact]
    public async Task GetWalletById_Success_ReturnsWalletDetails()
    {
        Guid walletId = Guid.NewGuid();
        var wallet = new Models.Wallet { Owner = _authenticatedHUser, Scheme = WalletScheme.Mastercard, Type = WalletType.Card, Number = "512345", Name = "My Wallet" };
        MockRepositoryToReturn(wallet);

        var response = await _service.GetWalletById(walletId);

        Assert.Equal(wallet.Name, response.Data.name);
    }
    
    [Fact]
    public async Task GetAllWalletByUser_Success_ReturnsAllWalletDetails()
    {
        var wallets = CreateWallets(4);

        var response = await _service.GetWalletsByUser(_authenticatedHUser);

        for (int i = 0; i < response.Data.Count; i++)
        {
            Assert.Equal(wallets[i].Name, response.Data[i].name);
        }
    }

    [Fact]
    public async Task DeleteWalletById_NotFound_ThrowsNotFoundException()
    {
        Guid walletId = Guid.NewGuid();
        MockRepositoryToReturn((Models.Wallet) null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteWalletById(walletId));
    }

    [Fact]
    public async Task DeleteWalletById_Success_ReturnsSuccessMessage()
    {
        Guid walletId = Guid.NewGuid();
        var wallet = new Models.Wallet { Owner = _authenticatedHUser };
        MockRepositoryToReturn(wallet);

        var response = await _service.DeleteWalletById(walletId);

        Assert.True(response.Success);
    }

    private void MockWalletSchemeValidatorToReturn(bool valid, WalletRegistration wr)
    {
        var schemeValidatorMock = new Mock<IWalletSchemeValidator>();
        schemeValidatorMock.Setup(x => x.Validate(wr.PAN)).Returns(valid);

        _validatorFactoryMock.Setup(vf => vf.GetWalletSchemeValidator(wr.Scheme))
            .Returns(schemeValidatorMock.Object);
    }
    
    private void MockRepositoryToReturn(Models.Wallet? value)
    {
        _repositoryMock.Setup(x => x.Get(It.IsAny<Expression<Func<Models.Wallet, bool>>>()))
            .ReturnsAsync(value);
    }
}
