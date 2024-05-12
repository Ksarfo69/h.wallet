using System.ComponentModel.DataAnnotations;
using H.Wallet.Api.Enums;

namespace H.Wallet.Api.Models;

public class Wallet : BaseModel
{
    [Required] 
    public string Name { get; set; }
    
    [Required] 
    public WalletType Type { get; init; }
    
    [Required] 
    public WalletScheme Scheme { get; init; }
    
    [Required] 
    [StringLength(50, MinimumLength = 6)]
    public string Number { get; init; }
    
    [Required] public HUser Owner { get; init; }

    public WalletResponseDto ToResponseDto()
    {
        return new WalletResponseDto
        (
            id : Id,
            name : Name,
            number : Number,
            scheme : Scheme,
            type : Type,
            createdAt: CreatedAt,
            owner: Owner.PhoneNumber
        );
    }
}

public record WalletRegistration
{
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Wallet name must be between 1 and 50 characters long.")]
    public string Name { get; init; }

    [Required]
    [EnumDataType(typeof(WalletScheme))]
    public WalletScheme Scheme { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "PAN must be between 6 and 50 characters long.")]
    public string PAN { get; init; }

    public WalletRegistration(string name, WalletScheme scheme, string PAN)
    {
        Name = name;
        Scheme = scheme;
        this.PAN = PAN;
    }
}

public record WalletResponseDto(
    Guid id,
    string name,
    WalletType type,
    WalletScheme scheme,
    string number,
    DateTime createdAt,
    string owner
);