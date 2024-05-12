using H.Wallet.Api.Enums;
using H.Wallet.Api.Utils;

namespace H.Wallet.Api.Tests.UnitTests.Utils;

using Xunit;

public class ValidatorTests
{
    [Fact]
    public void VisaSchemeValidator_ValidatesCorrectly()
    {
        var validator = new VisaSchemeValidator();
        Assert.True(validator.Validate("4123456789012345")); // Valid Visa
        Assert.False(validator.Validate("5123456789012345")); // Invalid Visa
        Assert.False(validator.Validate("412345678901234")); // Invalid length
    }

    [Fact]
    public void MastercardSchemeValidator_ValidatesCorrectly()
    {
        var validator = new MastercardSchemeValidator();
        Assert.True(validator.Validate("5123456789012345")); // Valid Mastercard
        Assert.False(validator.Validate("4123456789012345")); // Invalid Mastercard
        Assert.False(validator.Validate("512345")); // Invalid length
    }

    [Fact]
    public void MtnSchemeValidator_ValidatesCorrectly()
    {
        var mtnPrefixes = new string[] {"23324"};
        
        var validator = new MtnSchemeValidator();

        foreach (var p in mtnPrefixes)
        {
            Assert.True(validator.Validate($"{p}1234567")); // Valid MTN
            Assert.False(validator.Validate($"{p}123456")); // Invalid length
        }
        
        Assert.False(validator.Validate("233701234567")); // Invalid prefix
    }
    
    [Fact]
    public void VodafoneSchemeValidator_ValidatesCorrectly()
    {
        var vodafonePrefixes = new string[] {"23320"};
        
        var validator = new VodafoneSchemeValidator();
        
        foreach (var p in vodafonePrefixes)
        {
            Assert.True(validator.Validate("233201234567")); // Valid Vodafone
            Assert.False(validator.Validate("23320123456")); // Invalid length
        }
        
        
        Assert.False(validator.Validate("233701234567")); // Invalid prefix
    }
    
    [Fact]
    public void AirteltigoSchemeValidator_ValidatesCorrectly()
    {
        var airteltigoPrefixes = new string[] {"23326"};
        
        var validator = new AirtelTigoSchemeValidator();
        
        foreach (var p in airteltigoPrefixes)
        {
            Assert.True(validator.Validate($"{p}1234567")); // Valid AirtelTigo
            Assert.False(validator.Validate($"{p}123456")); // Invalid length
        }
        
        Assert.False(validator.Validate("233701234567")); // Invalid prefix
    }
    
    [Theory]
    [InlineData(WalletScheme.Visa, typeof(VisaSchemeValidator))]
    [InlineData(WalletScheme.Mastercard, typeof(MastercardSchemeValidator))]
    [InlineData(WalletScheme.Mtn, typeof(MtnSchemeValidator))]
    [InlineData(WalletScheme.Vodafone, typeof(VodafoneSchemeValidator))]
    [InlineData(WalletScheme.AirtelTigo, typeof(AirtelTigoSchemeValidator))]
    public void GetWalletValidator_ShouldReturnCorrectValidatorType(WalletScheme scheme, Type expectedType)
    {
        var validatorFactory = new ValidatorFactory();
        Assert.IsType<VisaSchemeValidator>(validatorFactory.GetWalletSchemeValidator(WalletScheme.Visa));
        Assert.IsType<MastercardSchemeValidator>(validatorFactory.GetWalletSchemeValidator(WalletScheme.Mastercard));
        Assert.IsType<MtnSchemeValidator>(validatorFactory.GetWalletSchemeValidator(WalletScheme.Mtn));
    }
}
