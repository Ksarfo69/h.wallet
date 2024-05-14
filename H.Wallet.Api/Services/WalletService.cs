using H.Wallet.Api.Enums;
using H.Wallet.Api.Exceptions;
using H.Wallet.Api.Models;
using H.Wallet.Api.Repositories;
using H.Wallet.Api.Utils;

namespace H.Wallet.Api.Services;

public interface IWalletService
{
    Task<ApiResponse<string>> NewWallet(HUser authenticatedHUser, WalletRegistration wr);
    Task<ApiResponse<WalletResponseDto>> GetWalletById(HUser authenticatedHUser, Guid id);
    Task<ApiResponse<List<WalletResponseDto>>> GetWalletsByUser(HUser user);
    Task<ApiResponse> DeleteWalletById(HUser authenticatedHUser, Guid id);
    Task<Models.Wallet?> FetchWalletById(Guid id);
}

public class WalletService : IWalletService
{
    private readonly ILogger<WalletService> _logger;
    private readonly IWalletRepository _repository;
    private readonly IValidatorFactory _validatorFactory;

    private readonly int MAX_WALLET_COUNT = 5;
    
    public WalletService(
        ILogger<WalletService> logger, 
        IWalletRepository repository, 
        IValidatorFactory validatorFactory)
    {
        _logger = logger;
        _repository = repository;
        _validatorFactory = validatorFactory;
    }
    
    public async Task<ApiResponse<string>> NewWallet(HUser authenticatedHUser, WalletRegistration wr)
    {
        _logger.LogInformation($"Creating wallet for user: {authenticatedHUser.PhoneNumber}");
        
        // validate scheme
        bool validScheme = _validatorFactory.GetWalletSchemeValidator(wr.Scheme).Validate(wr.PAN);
        if (!validScheme)
        {
            var msg = $"The provided scheme number: {wr.PAN} is not valid for scheme: {wr.Scheme}.";
            _logger.LogInformation($"Failed to create wallet for user: {authenticatedHUser.PhoneNumber}. Reason: {msg}");
            throw new BadRequestException(msg);
        }
        
        // get wallet type and generate wallet number
        var walletType = wr.Scheme.AsWalletType();
        var walletNumber = walletType == WalletType.Card ? wr.PAN.Substring(0, 6) : wr.PAN;
        
        // check if wallet already added by user
        if (authenticatedHUser.Wallets.FirstOrDefault(w => w.Number.Equals(walletNumber)) != default)
        {
            var msg = "Wallet already exists.";
            _logger.LogInformation($"Failed to create wallet for user: {authenticatedHUser.PhoneNumber}. Reason: {msg}");
            throw new AlreadyExistsException("Wallet already exists.");
        }
        
        // check user's existing wallets count
        if (authenticatedHUser.Wallets.Count >= MAX_WALLET_COUNT)
        {
            var msg = "Maximum number of wallets reached.";
            _logger.LogInformation($"Failed to create wallet for user: {authenticatedHUser.PhoneNumber}. Reason: {msg}");
            throw new ForbiddenException("Maximum number of wallets reached.");
        }
        
        // create and add wallet to db
        var wallet = new Models.Wallet
        {
            Name = wr.Name,
            Scheme = wr.Scheme,
            Type = walletType,
            Number = walletNumber,
            Owner = authenticatedHUser
        };
        await _repository.Add(wallet);
        
        _logger.LogInformation($"Wallet created successfully for user: {authenticatedHUser.PhoneNumber}");
        
        return new ApiResponse<string>
        {
            Message = "Wallet created successfully",
            Data = wallet.Id.ToString()
        };
    }

    public async Task<ApiResponse<WalletResponseDto>> GetWalletById(HUser authenticatedUser, Guid id)
    {
        _logger.LogInformation($"Retrieving wallet with id: {id}");
        
        // check if wallet exists
        var existing = await _repository.Get(w => w.Id == id && w.Owner.Id == authenticatedUser.Id);
        if (existing == default)
        {
            var msg = $"Wallet with id: {id} does not exist.";
            _logger.LogInformation($"Failed to retrieve wallet with id: {id}. Reason: {msg}");
            throw new NotFoundException(msg);
        }

        var responseDto = existing.ToResponseDto();
        
        _logger.LogInformation($"Retrieved wallet with id: {id} successfully.");
        
        return new ApiResponse<WalletResponseDto>
        {
            Message = "Retrieved user wallet successfully.",
            Data = responseDto
        };
    }

    public async Task<ApiResponse<List<WalletResponseDto>>> GetWalletsByUser(HUser authenticatedHUser)
    {
        _logger.LogInformation($"Retrieving all wallets for user: {authenticatedHUser.PhoneNumber}");

        var responseDtos = authenticatedHUser.Wallets.Select(w => w.ToResponseDto()).ToList();
        
        _logger.LogInformation($"Retrieved all wallets for user: {authenticatedHUser.PhoneNumber} successfully.");
        
        return new ApiResponse<List<WalletResponseDto>>
        {
            Message = "Retrieved all user wallets successfully.",
            Data = responseDtos
        };
    }

    public async Task<ApiResponse> DeleteWalletById(HUser authenticatedUser, Guid id)
    {
        _logger.LogInformation($"Deleting wallet with id: {id}");
        
        var existing = await _repository.Get(w => w.Id == id && w.Owner.Id == authenticatedUser.Id);

        if (existing == default)
        {
            var msg = $"Wallet with id: {id} does not exist.";
            _logger.LogInformation($"Failed to delete wallet with id: {id}. Reason: {msg}");
            throw new NotFoundException(msg);
        };

        await _repository.Remove(existing);
        
        _logger.LogInformation($"Deleted wallet with id: {id} successfully.");
        
        return new ApiResponse
        {
            Message = "Wallet deleted successfully."
        };
    }

    public async Task<Models.Wallet?> FetchWalletById(Guid id)
    {
       return await _repository.Get(w => w.Id == id);
    }
}