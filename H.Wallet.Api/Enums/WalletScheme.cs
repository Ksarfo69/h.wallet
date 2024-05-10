namespace H.Wallet.Api.Enums;

public enum WalletScheme
{
    Visa, Mastercard, Mtn, Vodafone, AirtelTigo
}

public static class WalletSchemeExtension
{
    public static WalletType AsWalletType(this WalletScheme ws)
    {
        switch (ws)
        {
            case WalletScheme.Visa:
            case WalletScheme.Mastercard:
                return WalletType.Card;
            case WalletScheme.Mtn:
            case WalletScheme.Vodafone:
            case WalletScheme.AirtelTigo:
                return WalletType.Momo;
        }

        throw new ArgumentOutOfRangeException($"No wallet type defined for wallet scheme: {ws}");
    }
}