using H.Wallet.Api.Models;
using H.Wallet.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace H.Wallet.Api.Controllers;

[ApiController]
[Route("api/v1/user")]
public class HUserController : Controller
{
    private readonly IHUserService _service;

    public HUserController(IHUserService service)
    {
        _service = service;
    }
    
    [HttpPost("register")]
    [SwaggerOperation(Summary = "Register a new user", Description = "Registers a new user with phone number, username, and password")]
    [SwaggerResponse(201, "User created successfully", typeof(ApiResponse<string>))]
    [SwaggerResponse(400, "Bad request if the phone number is invalid, passwords do not match, or user already exists", typeof(string))]
    public async Task<IActionResult> Register(HUserRegistration r)
    {
        var res = await _service.Register(r);
        return Created("", res);
    }
    
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Log in a user", Description = "Logs in a user with their phone number and password")]
    [SwaggerResponse(200, "User logged in successfully, returns JWT token", typeof(ApiResponse<string>))]
    [SwaggerResponse(401, "Unauthorized if the user does not exist or password is incorrect", typeof(string))]
    public async Task<IActionResult> Login(HUserLogin l)
    {
        var res = await _service.Login(l);
        return Ok(res);
    }
    
    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(Summary = "Get user details", Description = "Retrieves user details by phone number")]
    [SwaggerResponse(200, "User details retrieved successfully", typeof(ApiResponse<HUserResponseDto>))]
    [SwaggerResponse(401, "Unauthorized if the user does not exist", typeof(string))]
    public async Task<IActionResult> GetMyAccountDetails()
    {
        var res = await _service.GetHUserDetails(User);
        return Ok(res);
    }
}