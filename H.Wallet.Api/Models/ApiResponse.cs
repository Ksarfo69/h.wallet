using System.Text.Json;
using System.Text.Json.Serialization;

namespace H.Wallet.Api.Models;


public class ApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class ApiResponse<T> : ApiResponse
{
    [JsonPropertyName("data")]
    public T Data { get; set; }
    
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}