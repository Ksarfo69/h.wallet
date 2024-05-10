using System.ComponentModel.DataAnnotations;

namespace H.Wallet.Api.Models;

public class BaseModel
{
    [Key] public Guid Id { get; private set; } = Guid.NewGuid();
    [Required] public DateTime CreatedAt { get; } = DateTime.Now;
}