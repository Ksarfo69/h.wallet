using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using H.Wallet.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace H.Wallet.Api.Tests.IntegrationTests;

public class HUserControllerIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly HttpClient _client;
    private readonly string REGISTER_ENDPOINT = "/api/v1/user/register";
    private readonly string LOGIN_ENDPOINT = "/api/v1/user/login";
    private readonly string USER_DETAILS_ENDPOINT = "/api/v1/user/me";

    public HUserControllerIntegrationTests(WebApplicationFactory<Startup> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task Register_PasswordsDoNotMatch_ReturnsBadRequest()
    {   var registration = new HUserRegistration
        (
            username : "testuser",
            phoneNumber : "249889890",
            password : "Password123",
            confirmPassword : "UnmatchingPassword"
        );
        var content = new StringContent(JsonSerializer.Serialize(registration), Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync(REGISTER_ENDPOINT, content);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidPhoneNumber_ReturnsBadRequest()
    {   var registration = new HUserRegistration
        (
            username : "testuser",
            phoneNumber : "InvalidPhoneNumber",
            password : "Password123",
            confirmPassword : "Password123"
        );
        var content = new StringContent(JsonSerializer.Serialize(registration), Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync(REGISTER_ENDPOINT, content);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task Register_PhoneNumberAlreadyExists_ReturnsConflict()
    {   
        var registration = new HUserRegistration
        (
            username : "testuser",
            phoneNumber : "249889891",
            password : "Password123",
            confirmPassword : "Password123"
        );
        
        await RegisterNewUser(registration);
        
        var content = new StringContent(JsonSerializer.Serialize(registration), Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync(REGISTER_ENDPOINT, content);
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [Fact]
    public async Task Register_ValidRegistration_ReturnsSuccess()
    {   
        var registration = new HUserRegistration
        (
            username : "testuser",
            phoneNumber : "249889892",
            password : "Password123",
            confirmPassword : "Password123"
        );
        
        var content = new StringContent(JsonSerializer.Serialize(registration), Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync(REGISTER_ENDPOINT, content);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {   
        var login = new HUserLogin
        (
            phoneNumber : "1234567890",
            password : "Password123"
        );
        
        var content = new StringContent(JsonSerializer.Serialize(login), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(LOGIN_ENDPOINT, content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var registration = new HUserRegistration
        (
            username : "testuser",
            phoneNumber : "249889893",
            password : "Password123",
            confirmPassword : "Password123"
        );
        
        await RegisterNewUser(registration);
        
        var login = new HUserLogin
        (
            phoneNumber : registration.PhoneNumber,
            password : registration.Password
        );
        
        var content = new StringContent(JsonSerializer.Serialize(login), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(LOGIN_ENDPOINT, content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetMyAccountDetails_InvalidToken_ReturnsUnathorized()
    {
        var token = "invalid_token";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await _client.GetAsync(USER_DETAILS_ENDPOINT);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task GetMyAccountDetails_ReturnsUserDetails()
    {
        var registration = new HUserRegistration
        (
            username : "testuser",
            phoneNumber : "249889894",
            password : "Password123",
            confirmPassword : "Password123"
        );
        
        await RegisterNewUser(registration);
        
        var login = new HUserLogin
        (
            phoneNumber : registration.PhoneNumber,
            password : registration.Password
        );
        
        var content = new StringContent(JsonSerializer.Serialize(login), Encoding.UTF8, "application/json");
        var loginResponse = await _client.PostAsync(LOGIN_ENDPOINT, content);
        
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        
        string responseContent = await loginResponse.Content.ReadAsStringAsync();

        var tokenString = responseContent.Split(",").First().Split(":").Last();
        var token = tokenString.Substring(1, tokenString.Length - 2);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await _client.GetAsync(USER_DETAILS_ENDPOINT);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    private async Task RegisterNewUser(HUserRegistration registration)
    {
        var content = new StringContent(JsonSerializer.Serialize(registration), Encoding.UTF8, "application/json");
        
        await _client.PostAsync(REGISTER_ENDPOINT, content);
    }
}
