using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using H.Wallet.Api.Enums;
using H.Wallet.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace H.Wallet.Api.Tests.IntegrationTests;

public class WalletControllerIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly HttpClient _client;
    private readonly string REGISTER_ENDPOINT = "/api/v1/user/register";
    private readonly string LOGIN_ENDPOINT = "/api/v1/user/login";
    private readonly string CREATE_WALLET_ENDPOINT = "api/v1/wallet/new";
    private readonly string GET_WALLET_ENDPOINT = "api/v1/wallet/{id}";
    private readonly string GET_USER_WALLETS_ENDPOINT = "api/v1/wallet/all";
    private readonly string DELETE_WALLET_ENDPOINT = "api/v1/wallet/{id}";

    public WalletControllerIntegrationTests(WebApplicationFactory<Startup> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        RegisterAndLoginNewUser("0").GetAwaiter().GetResult();
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_InvalidVisaSchemeNumber_ReturnsBadRequest()
    {
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Visa, PAN: "5123456789012345" );
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_InvalidMastercardSchemeNumber_ReturnsBadRequest()
    {
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Mastercard, PAN: "4123456789012345" );
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_InvalidMtnSchemeNumber_ReturnsBadRequest()
    {
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Mtn, PAN: "233268776654" );
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_InvalidVodafoneSchemeNumber_ReturnsBadRequest()
    {
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Vodafone, PAN: "233248776654" );
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_InvalidAirtelTigoSchemeNumber_ReturnsBadRequest()
    {
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.AirtelTigo, PAN: "233248776654" );
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_MaxWalletsReached_ReturnsForbidden()
    {
        await RegisterAndLoginNewUser("1");
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Visa, PAN: "4123556789012345" );
        
        for(int i = 0; i<5; i++) {
            var wr = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Visa, PAN: $"4123{i}56789012345" );
            await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, wr); // add 4 wallets
        }
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_CardWalletAlreadyExists_ReturnsConflict()
    {
        await RegisterAndLoginNewUser("2");
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Visa, PAN: "4123456789012345" );
        await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration); // first wallet
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_MomoWalletAlreadyExists_ReturnsConflict()
    {
        await RegisterAndLoginNewUser("3");
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Mtn, PAN: "233549087657" );
        await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_ValidVisaWalletDetails_ReturnsSuccess()
    {
        await RegisterAndLoginNewUser("4");
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Visa, PAN: "4123456789012345" );
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_ValidMastercardWalletDetails_ReturnsSuccess()
    {
        await RegisterAndLoginNewUser("5");
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Mastercard, PAN: "5123456789012345" );
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_ValidMtnWalletDetails_ReturnsSuccess()
    {
        await RegisterAndLoginNewUser("6");
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Mtn, PAN: "233249885566" );
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_ValidVodafoneWalletDetails_ReturnsSuccess()
    {
        await RegisterAndLoginNewUser("7");
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Vodafone, PAN: "233209885566" );
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWalletEndpoint_ValidAirtelTigoWalletDetails_ReturnsSuccess()
    {
        await RegisterAndLoginNewUser("8");
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.AirtelTigo, PAN: "233269885566" );
        
        var response = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task GetWalletByIdEndpoint_InvalidWalletId_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        
        var response = await _client.GetAsync(GET_WALLET_ENDPOINT.Replace("{id}", id.ToString()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetWalletByIdEndpoint_ValidWalletId_ReturnsSuccess()
    {
        await RegisterAndLoginNewUser("9");
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Visa, PAN: "4123456789012345" );
        var registrationResponse = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);
        var serviceResponse = await As<ApiResponse<string>>(registrationResponse);
        
        var response = await _client.GetAsync(GET_WALLET_ENDPOINT.Replace("{id}", serviceResponse.Data));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWalletsByUser_Endpoint_ReturnsSuccess()
    {
        var response = await _client.GetAsync(GET_USER_WALLETS_ENDPOINT);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteWalletEndpoint_InvalidWalletId_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        
        var response = await _client.DeleteAsync(DELETE_WALLET_ENDPOINT.Replace("{id}", id.ToString()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWalletEndpoint_ValidWalletId_ReturnsSuccess()
    {
        await RegisterAndLoginNewUser("1");
        var walletRegistration = new WalletRegistration ( name: "Test Models.Wallet", scheme: WalletScheme.Visa, PAN: "4123456789012345" );
        var registrationResponse = await _client.PostAsJsonAsync(CREATE_WALLET_ENDPOINT, walletRegistration);
        var serviceResponse = await As<ApiResponse<string>>(registrationResponse);
        
        var response = await _client.DeleteAsync(DELETE_WALLET_ENDPOINT.Replace("{id}", serviceResponse.Data));
        
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }
    
    private async Task RegisterAndLoginNewUser(String id)
    {
        // register
        var registration = new HUserRegistration
        (
            username : "testuser",
            phoneNumber : $"23326000000{id}",
            password : "Password123",
            confirmPassword : "Password123"
        );
        var registrationContent = new StringContent(JsonSerializer.Serialize(registration), Encoding.UTF8, "application/json");
        await _client.PostAsync(REGISTER_ENDPOINT, registrationContent);
        
        // login
        var login = new HUserLogin
        (
            phoneNumber : registration.PhoneNumber,
            password : registration.Password
        );
        var loginContent = new StringContent(JsonSerializer.Serialize(login), Encoding.UTF8, "application/json");
        var loginResponse = await _client.PostAsync(LOGIN_ENDPOINT, loginContent);
        
        // extract service response
        var serviceResponse = await As<ApiResponse<string>>(loginResponse);
        
        // add token to client
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serviceResponse.Data);
    }

    private async Task<T> As<T>(HttpResponseMessage response)
    {
        var jsonString = await response.Content.ReadAsStringAsync();
        var o = JsonSerializer.Deserialize<T>(jsonString);
        return o;
    }
}