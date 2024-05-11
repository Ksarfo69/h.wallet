using System.Text.Json;

namespace H.Wallet.Api.Models;


public class ApiResponse
{
    public bool Success { get; set; } = true;
    public string Message { get; set; }
    
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class ApiResponse<T> : ApiResponse
{
    public T Data { get; set; }
    
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}