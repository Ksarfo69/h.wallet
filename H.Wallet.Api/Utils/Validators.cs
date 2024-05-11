using H.Wallet.Api.Enums;

namespace H.Wallet.Api.Utils;

public interface IPhoneNumberValidator
{
    bool Validate(string phoneNumber);
}

public class UniversalPhoneNumberValidator : IPhoneNumberValidator
{
    public bool Validate(string phoneNumber)
    {
        return true; //logic can be extended to validate based on country
    }
}

public interface IWalletSchemeValidator
{
    bool Validate(string PAN);
}

public class VisaSchemeValidator : IWalletSchemeValidator
{
    public bool Validate(string PAN)
    {
        return PAN.Length == 16 && PAN.StartsWith("4");
    }
}

public class MastercardSchemeValidator : IWalletSchemeValidator
{
    public bool Validate(string PAN)
    {
        return PAN.Length == 16 && PAN.StartsWith("5");
    }
}

public class MtnSchemeValidator : IWalletSchemeValidator
{
    HashSet<string> _mtnPrefixes = new HashSet<string>
    {
        "23324", "23354", "23355", "233591", "233592", "233593", "233594", "233595", "233596", "2333080", "2333081",
        "2333082", "2333180", "2333280", "23333800", "23334800", "23335800", "23336800", "23337800", "23338800",
        "23339800"
    };
    
    public bool Validate(string PAN)
    {
        if (PAN.Length != 12) return false;
        
        for(int i = 5; i < PAN.Length; i++)
        {
            if (_mtnPrefixes.Contains(PAN.Substring(0, i))) return true;
        }

        return false;
    }
}

public class VodafoneSchemeValidator : IWalletSchemeValidator
{
    HashSet<string> _vodafonePrefixes = new HashSet<string>
    {
        "23320", "23330", "23331", "23332", "23333", "23334", "23335", "23336", "23337", "23338", "23339", "23350"
    };
    public bool Validate(string PAN)
    {
        if (PAN.Length != 12) return false;
        
        for(int i = 5; i < PAN.Length; i++)
        {
            if (_vodafonePrefixes.Contains(PAN.Substring(0, i))) return true;
        }

        return false;
    }
}

public class AirtelTigoSchemeValidator : IWalletSchemeValidator
{
    HashSet<string> _airteltigoPrefixes = new HashSet<string>
    {
        "23326", "23356", "233307", "233317", "233327", "233337", "233347", "233357", "233367", "233377", "233387",
        "233397", "23327", "23329", "23357"
    };
    
    public bool Validate(string PAN)
    {
        if (PAN.Length != 12) return false;
        
        for(int i = 5; i < PAN.Length; i++)
        {
            if (_airteltigoPrefixes.Contains(PAN.Substring(0, i))) return true;
        }

        return false;
    }
}

public interface IValidatorFactory
{
    IWalletSchemeValidator GetWalletSchemeValidator(WalletScheme scheme);
    IPhoneNumberValidator GetPhoneNumberValidator();
}

public class ValidatorFactory : IValidatorFactory
{
    public IWalletSchemeValidator GetWalletSchemeValidator(WalletScheme scheme)
    {
        switch (scheme)
        {
            case WalletScheme.Visa:
                return new VisaSchemeValidator();
            case WalletScheme.Mastercard:
                return new MastercardSchemeValidator();
            case WalletScheme.Mtn:
                return new MtnSchemeValidator();
            case WalletScheme.Vodafone:
                return new VodafoneSchemeValidator();
            case WalletScheme.AirtelTigo:
                return new AirtelTigoSchemeValidator();
        }

        throw new ArgumentOutOfRangeException($"No validator defined for wallet scheme: {scheme}");
    }

    public IPhoneNumberValidator GetPhoneNumberValidator()
    {
        return new UniversalPhoneNumberValidator();
    }
}

