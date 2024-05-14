using H.Wallet.Api.Models;
using H.Wallet.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace H.Wallet.Api.Controllers;

[ApiController]
[Route("api/v1/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly IHUserService _hUserService;

    public WalletController(IWalletService walletService, IHUserService hUserService)
    {
        _walletService = walletService;
        _hUserService = hUserService;
    }

    [HttpPost("new")]
    [SwaggerOperation(Summary = "Create a new wallet", Description = "Creates a new wallet for a user")]
    [SwaggerResponse(201, "Wallet created successfully", typeof(ApiResponse<string>))]
    [SwaggerResponse(400, "Bad request if the PAN is not valid for the particular scheme.", typeof(string))]
    [SwaggerResponse(401, "Unauthorized", typeof(string))]
    [SwaggerResponse(403, "Forbidden if maximum wallet count is reached.", typeof(string))]
    [SwaggerResponse(409, "Conflict if wallet already added.", typeof(string))]
    public async Task<IActionResult> CreateWallet([FromBody] WalletRegistration walletRegistration)
    {
        var authenticatedHUser = await _hUserService.GetAuthenticatedUser(User);
        var result = await _walletService.NewWallet(authenticatedHUser, walletRegistration);
        return Created("", result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get wallet details by ID", Description = "Retrieves wallet details by wallet ID")]
    [SwaggerResponse(200, "Wallet details retrieved successfully", typeof(ApiResponse<WalletResponseDto>))]
    [SwaggerResponse(401, "Unauthorized", typeof(string))]
    [SwaggerResponse(404, "Not found if wallet does not exist", typeof(string))]
    public async Task<IActionResult> GetWalletById(Guid id)
    {
        var authenticatedHUser = await _hUserService.GetAuthenticatedUser(User);
        var result = await _walletService.GetWalletById(authenticatedHUser, id);
        return Ok(result);
    }
    
    [HttpGet("all")]
    [SwaggerOperation(Summary = "Get all user's wallets", Description = "Retrieves all wallets associated with a user")]
    [SwaggerResponse(200, "All user wallets retrieved successfully", typeof(ApiResponse<List<WalletResponseDto>>))]
    [SwaggerResponse(401, "Unauthorized", typeof(string))]
    public async Task<IActionResult> GetWalletsByUser()
    {
        var authenticatedHUser = await _hUserService.GetAuthenticatedUser(User);
        var result = await _walletService.GetWalletsByUser(authenticatedHUser);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete a wallet by ID", Description = "Deletes a wallet by its ID")]
    [SwaggerResponse(202, "Wallet deleted successfully", typeof(ApiResponse))]
    [SwaggerResponse(401, "Unauthorized", typeof(string))]
    [SwaggerResponse(404, "Not found if wallet does not exist", typeof(string))]
    public async Task<IActionResult> DeleteWallet(Guid id)
    {
        var authenticatedHUser = await _hUserService.GetAuthenticatedUser(User);
        var result = await _walletService.DeleteWalletById(authenticatedHUser, id);
        return Accepted(result);
    }
}
