using System.ComponentModel.DataAnnotations;

namespace H.Wallet.Api.Models;

public class BaseModel
{
    [Key] public Guid Id { get; init; } = Guid.NewGuid();
    [Required] public DateTime CreatedAt { get; init; } = DateTime.Now;
}